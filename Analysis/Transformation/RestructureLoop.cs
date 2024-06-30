namespace CSynth.Analysis.Transformation;

public static class RestructureLoop
{
    public static void Restructure(CFG cfg) {

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
        var loops = sccs.Where(scc => scc.Count > 1 || scc[0].TargetsBlock(scc[0])).ToList();

        foreach (var loop in loops) {
            RestructureSingle(cfg, loop);
        }
    }

    private static void RestructureSingle(CFG cfg, List<Block> loop) {

        var (headers, entries) = FindHeadersEntries(cfg, loop);
        var (exiting, exits) = FindExitingExits(cfg, loop);

        Block header;
        if (headers.Count > 1) {
            header = ConstructSingleHeader(cfg, entries, headers);
            loop.Add(header);
        } else {
            header = headers[0];
        }

        // All blocks that target the headers of the loop
        var backEdgeBlocks = cfg.Where(block => block.Targets.Intersect(headers).Any()).ToList();

        if (backEdgeBlocks.Count == 1
            && exiting.Count == 1
            && backEdgeBlocks[0] == exiting[0]) {
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

            return;
        }

        Block exit;
        if (exits.Count > 1) {
            exit = ConstructSingleExit(cfg, exiting, exits);
            loop.Add(exit);
        } else {
            exit = exits[0];
        }
    }


    private static Tuple<List<Block>, List<Block>> FindHeadersEntries(CFG cfg, List<Block> loop) {
        var headers = new List<Block>();
        var entries = new List<Block>();

        foreach (var block in cfg.Except(loop)) {
            var targets = block.Targets.Intersect(loop).ToList();
            
            headers.AddRange(targets);

            if (targets.Count > 0) {
                entries.Add(block);
            }
        }

        return Tuple.Create(
            headers.Distinct().ToList(),
            entries
        );
    }

    private static Tuple<List<Block>, List<Block>> FindExitingExits(CFG cfg, List<Block> loop) {
        var exiting = new List<Block>();
        var exits = new List<Block>();

        foreach (var block in loop) {
            var targets = block.Targets.Except(loop).ToList();
            
            exits.AddRange(targets);

            if (targets.Count > 0) {
                exiting.Add(block);
            }
        }

        return Tuple.Create(
            exiting,
            exits.Distinct().ToList()
        );
    }

    private static Block ConstructSingleHeader(CFG cfg, List<Block> predecessors, List<Block> succesors) {
        /*
         ╭───╮   ╭───╮    ╭───╮        ╭───╮
         │ 0 │   │ 1 │    │ 0 │        │ 1 │
         ╰┬─┬╯   ╰─┬─╯    ╰┬─┬╯        ╰─┬─╯
          ↓ ╰→───╮ ↓       ↓ ╰──→─╮      │
        ╭─┴─╮   ╭┴─┴╮    ╭─┴─╮  ╭─┴─╮  ╭─┴─╮
        │ 2 ├─→─┤ 3 │ →  │ A │  │ A │  │ A │
        ╰┬─┬╯   ╰┬──╯    ╰─┬─╯  ╰─┬─╯  ╰─┬─╯
         ↓ ╰──←──╯         ╰─→─┬─←╯──←───╯
                             ╭─┴─╮
                             │ B │
                             ╰┬─┬╯
                           ╭─←╯ ╰→─╮
                         ╭─┴─╮   ╭─┴─╮
                         │ 2 ├─→─┤ 3 │
                         ╰┬─┬╯   ╰─┬─╯
                          ↓ ╰──←───╯
        */
        var variable = Variable.Create(cfg);
        var header = BranchBlock.Create(cfg, variable);

        var counter = -1;

        foreach (var block in predecessors) {
            foreach (var target in block.Targets.Intersect(succesors).ToList()) {
                var assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(header);
                assignment.AddVariable(variable, counter++);

                block.ReplaceTarget(target, assignment);
            }
        }

        foreach (var block in succesors)
            header.AddTarget(block);

        return header;
    }

    private static Block ConstructSingleExit(CFG cfg, List<Block> predecessors, List<Block> succesors) {
        /*
          ╭───←──╮ ↓      ╭───←──╮ ↓
        ╭─┴─╮   ╭┴─┴╮   ╭─┴─╮   ╭┴─┴╮
        │ 0 ├─→─┤ 1 │   │ 0 ├─→─┤ 1 │
        ╰─┬─╯   ╰─┬─╯   ╰─┬─╯   ╰─┬─╯
          ↓       ↓    →  ↓       ↓
        ╭─┴─╮   ╭─┴─╮   ╭─┴─╮   ╭─┴─╮  
        │ 2 │   │ 3 │   │ A │   │ A │
        ╰───╯   ╰───╯   ╰─┬─╯   ╰─┬─╯
                          ╰─→─┬──←╯
                            ╭─┴─╮
                            │ B │
                            ╰┬─┬╯
                          ╭─←╯ ╰→─╮
                        ╭─┴─╮   ╭─┴─╮
                        │ 2 │   │ 3 │
                        ╰───╯   ╰───╯
        */

        var variable = Variable.Create(cfg);
        var exit = BranchBlock.Create(cfg, variable);

        var counter = -1;

        foreach (var block in succesors) {
            foreach (var target in block.Targets.Intersect(predecessors).ToList()) {
                var assignment = AssignmentBlock.Create(cfg);
                assignment.AddTarget(exit);
                assignment.AddVariable(variable, counter++);

                block.ReplaceTarget(target, assignment);
            }
        }

        foreach (var block in predecessors)
            block.AddTarget(exit);
        
        return exit;
    }
}
