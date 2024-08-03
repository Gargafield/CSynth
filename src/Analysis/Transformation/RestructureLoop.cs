
namespace CSynth.Analysis.Transformation;

public class RestructureLoop
{
    private CFG cfg;
    private HashSet<Block> loop = default!;

    private RestructureLoop(CFG cfg) {
        this.cfg = cfg;
    }

    public static void Restructure(CFG cfg) {

        var restructure = new RestructureLoop(cfg);

        var sccs = SCC.ComputeSCC(cfg);

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
        var loops = sccs.Where(scc => scc.Count > 1 || scc.First().TargetsBlock(scc.First())).ToList();

        foreach (var loop in loops) {
            restructure.loop = loop;
            var (header, control) = restructure.RestructureSingle();

            var region = LoopRegion.Create(cfg, loop, header, control);
        }
    }

    private Tuple<Block, Block> RestructureSingle() {
        var (entries, exits) = FindEntriesExits();

        var header = ConstructSingleHeader(entries);

        // All blocks that target the headers of the loop
        var backEdgeBlocks = entries.Where(block => block.Predecessors.Intersect(loop).Any()).ToList();

        if (backEdgeBlocks.Count == 1
            && exits.Count == 1
            && exits[0].Predecessors.Count == 1
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


    private Tuple<List<Block>, List<Block>> FindEntriesExits() {
        // Entries are blocks that have predecessors outside the loop
        // Exits are blocks that have predecessors inside the loop
        
        var entries = new List<Block>();
        var exits = new List<Block>();

        foreach (var block in loop) {
            if (block.Predecessors.Except(loop).Any()) {
                entries.Add(block);
            }

            exits.AddRange(block.Successors.Except(loop));
        }

        return Tuple.Create(entries, exits.Distinct().ToList());
    }

    private Block ConstructSingleHeader(List<Block> entries) {
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

        var variable = BlockVariable.HeaderExit;
        var header = BranchBlock.Create(cfg, variable);
        loop.Add(header);

        var counter = -1;

        foreach (var block in entries) {
            counter++;

            foreach (var predecessors in block.Predecessors) {
                var assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(header);
                assignment.AddVariable(variable, counter);
                if (loop.Contains(predecessors))
                    loop.Add(assignment);
                
                predecessors.ReplaceTarget(block, assignment);
            }

            header.AddBranch(counter, block);
        }

        return header;
    }

    private Block ConstructSingleExit(List<Block> exits) {
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

        var exitVariable = BlockVariable.HeaderExit;
        var exit = BranchBlock.Create(cfg, exitVariable);

        var counter = -1;

        foreach (var block in exits) {
            counter++;

            foreach (var predecessors in block.Predecessors.Intersect(loop)) {
                var assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(exit);
                assignment.AddVariable(exitVariable, counter);

                predecessors.ReplaceTarget(block, assignment);
            }

            exit.AddBranch(counter, block);
        }
        
        return exit;
    }

    private Block ConstructSingleControl(Block header, Block exit) {
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

        var repetitions = header.Predecessors.Intersect(loop).ToList();

        if (repetitions.Count == 1 && repetitions[0].Successors.Count == 2) {
            return repetitions[0];
        }

        var variable = BlockVariable.LoopControl;
        var control = BranchBlock.Create(cfg, variable);

        foreach (var predecessors in repetitions) {
            if (predecessors is AssignmentBlock assignment) {
                assignment.ReplaceTarget(header, control);
            }
            else {
                assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(control);
                predecessors.ReplaceTarget(header, assignment);
            }

            assignment.AddVariable(variable, true);
        }

        foreach (var predecessors in exit.Predecessors) {
            if (predecessors is AssignmentBlock assignment) {
                assignment.ReplaceTarget(exit, control);
            }
            else {
                assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(control);
                predecessors.ReplaceTarget(exit, assignment);
            }

            assignment.AddVariable(variable, false);
        }

        control.AddBranch(0, exit);
        control.AddBranch(1, header);

        return control;
    }
}
