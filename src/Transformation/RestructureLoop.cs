using CSynth.AST;

namespace CSynth.Transformation;

public class RestructureLoop
{
    private CFG cfg;
    private BlockCollection blocks;
    private RegionCollection regions;
    private SCC scc = default!;

    private List<int> loop = default!;

    private RestructureLoop(CFG cfg) {
        this.cfg = cfg;
        blocks = cfg.Blocks;
        regions = cfg.Regions;
        scc = new(blocks);
    }


    public static List<int> Restructure(CFG cfg) {

        var restructure = new RestructureLoop(cfg);

        /*
              │ ╭───╮  
              │ │ 1 ├─╮
       ╭──╮   │ ╰─┬─╯ │
       │╭─┴─╮ │ ╭─┴─╮ │
       ↑│ 1 │ │ │ 2 │ ↑
       │╰─┬─╯ │ ╰─┬─╯ │
       ╰──╯   │ ╭─┴─╮ │
              │ │ 3 ├─╯
              │ ╰───╯
        */
        var regions = new List<int>();
        restructure.scc.FindSCCs(restructure.blocks.GetEnumerableIds());
        
        while (restructure.scc.Count > 0) {
            var scc = restructure.scc.Pop();

            if (scc.Count <= 1 && !cfg.Blocks.Successors(scc[0]).Contains(scc[0])) {
                continue;
            }

            restructure.loop = scc;
            var (header, control) = restructure.RestructureSingle();
            var region = restructure.regions.AddLoopRegion(scc, header, control);
            regions.Add(region);

            restructure.scc.FindSCCs(scc.Except(new[] { header }));
        }

        return regions;
    }

    private Tuple<int, int> RestructureSingle() {
        var (entries, exits) = FindEntriesExits();

        var header = ConstructSingleHeader(entries);

        // All blocks that target the headers of the loop
        var backEdgeBlocks = entries.Where(id => blocks.Predecessors(id).Intersect(loop).Any()).ToList();

        if (backEdgeBlocks.Count == 1
            && exits.Count == 1
            && blocks.Predecessors(exits[0]).Count() == 1
            && backEdgeBlocks[0] == exits[0]) {
            /*
            We have our dream scenario:
              ↓
            ╭─┴─╮
            │ 0 ├╮
            ╰─┬─╯│
              ↓  ↑
            ╭─┴─╮│
            │ 1 ├╯
            ╰─┬─╯
              ↓
            */

            return Tuple.Create(header, exits[0]);
        }

        var exit = ConstructSingleExit(exits);
        var control = ConstructSingleControl(header, exit);

        return Tuple.Create(header, control);
    }


    private Tuple<List<int>, List<int>> FindEntriesExits() {
        // Entries are blocks that have predecessors outside the loop
        // Exits are blocks that have predecessors inside the loop
        
        var entries = new List<int>();
        var exits = new List<int>();

        foreach (var block in loop) {
            if (blocks.Predecessors(block).Except(loop).Any()) {
                entries.Add(block);
            }

            exits.AddRange(blocks.Successors(block).Except(loop));
        }

        if (exits.Count == 0 && entries.Count > 0) {
            exits.Add(entries[0]);
        }

        return Tuple.Create(entries, exits.Distinct().ToList());
    }

    private int ConstructSingleHeader(List<int> entries) {
        /*
         ╭───╮ ╭───╮     ╭───╮ ╭───╮ ╭───╮
         │ 0 │ │ 1 │     │ 0 │ │ 1 │ │ 2 ├╮
         ╰─┬─╯ ╰─┬─╯     ╰─┬─╯ ╰─┬─╯ ╰─┬─╯↑
           ├─────╯         │     │     │
         ╭─┴─╮ ╭───╮     ╭─┴─╮ ╭─┴─╮ ╭─┴─╮
         │ 3 ├←┤ 2 │  →  │ A │ │ A │ │ A │
         ╰─┬─╯ ╰─┬─╯     ╰─┬─╯ ╰─┬─╯ ╰─┬─╯
           ↓     ↑         ╰─────┼─────╯
           ╰Loops╯             ╭─┴─╮
                  New Header > │ B │
                               ╰─┬─╯
                               ╭─┴─╮
                  Old Header > │ 3 │
                               ╰─┬─╯
                                 ↓
         If more than one entry exists.
         Inserts AssignmentBlocks (A) on each entry arc (blocks to entries)
         and funnels control through a single BranchBlock (B), the new header.
        */

        // If theres only one entry, we can just use that as the header
        if (entries.Count == 1) {
            return entries[0];
        }

        var variable = BranchVariable.HeaderExit;
        var header = blocks.AddBranch(variable);
        loop.Add(header);

        var counter = -1;

        foreach (var block in entries) {
            counter++;

            foreach (var predecessors in blocks.Predecessors(block).ToArray()) {
                
                var assignment = blocks.AddAssignment(variable, counter);
                blocks.AddEdge(assignment, header);
                if (loop.Contains(predecessors))
                    loop.Add(assignment);
                
                blocks.ReplaceEdge(predecessors, block, assignment);
            }

            blocks.AddEdge(header, block);
        }

        return header;
    }

    private int ConstructSingleExit(List<int> exits) {
        /*
          ╭───←──╮ ↓      ╭───←──╮ ↓
        ╭─┴─╮   ╭┴─┴╮   ╭─┴─╮   ╭┴─┴╮
        │ 0 ├─→─┤ 1 │   │ 0 ├─→─┤ 1 │ < Old Exits
        ╰─┬─╯   ╰─┬─╯   ╰─┬─╯   ╰─┬─╯
          │       │    →  │       │
        ╭─┴─╮   ╭─┴─╮   ╭─┴─╮   ╭─┴─╮  
        │ 2 │   │ 3 │   │ A │   │ A │
        ╰─┬─╯   ╰─┬─╯   ╰─┬─╯   ╰─┬─╯
          ↓       ↓       ╰───┬───╯
                            ╭─┴─╮
                            │ B │ < New Exit
                            ╰┬─┬╯
                          ╭──╯ ╰──╮
                        ╭─┴─╮   ╭─┴─╮
                        │ 2 │   │ 3 │
                        ╰─┬─╯   ╰─┬─╯
                          ↓       ↓
         If more than one exit exists.
         Inserts AssignmentBlocks (A) on each exit arc (blocks from inside loop to exits)
         and funnels control through a single BranchBlock (B), the new exit.
        */

        if (exits.Count == 1) {
            return exits[0];
        }

        var exitVariable = BranchVariable.HeaderExit;
        var exit = blocks.AddBranch(exitVariable);

        var counter = -1;

        foreach (var block in exits) {

            foreach (var predecessors in blocks.Predecessors(block).Intersect(loop).ToArray()) {

                var assignment = blocks.AddAssignment(exitVariable, counter++);
                blocks.AddEdge(assignment, exit);

                blocks.ReplaceEdge(predecessors, block, assignment);
            }

            blocks.AddEdge(exit, block);
        }
        
        return exit;
    }

    private int ConstructSingleControl(int header, int exit) {
        /*
               ↓                 ↓
             ╭─┴─╮             ╭─┴─╮    
       ╭─────┤ 0 ├─────╮       │ 0 ├─────╮
       │     ╰─┬─╯     │       ╰─┬─╯     │
       │ ╭───╮ ↓ ╭───╮ │ → ╭───╮ ↓ ╭───╮ │
       ↑ │ 1 ├─┴─┤ 2 │ ↑   │ 1 ├─┴─┤ 2 │ │
       │ ╰─┬─╯   ╰─┬─╯ │   ╰─┬─╯   ╰─┬─╯ │
       ╰───┤       ├───╯   ╭─┴─╮   ╭─┴─╮ ↑
           ↓       ↓       │ A │   │ A │ │
                           ╰─┬─╯   ╰─┬─╯ │
                             ╰───┬───╯   │
                               ╭─┴─╮     │
                 New Control > │ B ├─────╯
                               ╰─┬─╯
                                 ↓
         If more than one repition
         Inserts AssignmentBlocks (A) on each repetition arc (blocks from inside loop to header)
         and funnel control through a single BranchBlock (B), the new control.
        */

        var repetitions = blocks.Predecessors(header).Intersect(loop).ToList();

        if (repetitions.Count == 1 && blocks.Successors(repetitions[0]).Count() == 2) {
            return repetitions[0];
        }

        var variable = BranchVariable.LoopControl;
        var control = blocks.AddBranch(variable);

        foreach (var predecessor in repetitions) {
            var assignment = blocks.AddAssignment(variable, 1);
            blocks.AddEdge(assignment, control);
            blocks.ReplaceEdge(predecessor, header, assignment);
        }

        foreach (var predecessor in blocks.Predecessors(exit).ToArray()) {
            var assignment = blocks.AddAssignment(variable, 0);
            blocks.AddEdge(assignment, control);
            blocks.ReplaceEdge(predecessor, exit, assignment);
        }

        blocks.AddEdge(control, exit);
        blocks.AddEdge(control, header);

        return control;
    }
}
