using CSynth.AST;

namespace CSynth.Analysis;

public class FlowInfo
{
    public CFG CFG { get; set; } = new();
    public List<BasicBlock> Blocks { get; set; } = new();

    private HashSet<int> blockOffsets { get; set; } = new();
    private Dictionary<int, List<int>> targets { get; set; } = new();
    private int returnOffset { get; set; }

    internal FlowInfo() { }

    public static FlowInfo From(IList<Statement> statements)
    {
        var flowInfo = new FlowInfo();
        flowInfo.Build(statements);
        flowInfo.BuildCFG(statements);
        return flowInfo;
    }

    private void AddTarget(int source, int target)
    {
        if (!targets.ContainsKey(source))
            targets[source] = new List<int>();
        targets[source].Add(target);

        if (target > 0)
            blockOffsets.Add(target);
    }

    private void Build(IList<Statement> statements) {
        blockOffsets.Add(0);

        for (int i = 0; i < statements.Count; i++) {
            var statement = statements.ElementAt(i);

            if (statement is BranchStatement branch) {
                AddTarget(i, statements.IndexOf(branch.Target));
                AddTarget(i, i + 1);
            }
            else if (statement is GotoStatement @goto) {
                AddTarget(i, statements.IndexOf(@goto.Target));
                blockOffsets.Add(i + 1);
            }
            else if (statement is ReturnStatement @return) {
                AddTarget(i, -1);
            }
        }

        var last = statements.Last();
        returnOffset = statements.Count + 1;
        blockOffsets.Add(returnOffset);
    }

    private void BuildCFG(IList<Statement> statements) {
        
        // Collect block offsets into pairs start, end
        var orderedOffsets = blockOffsets.OrderBy(x => x)
            .Take(blockOffsets.Count);
        var offsets = orderedOffsets.Zip(orderedOffsets.Skip(1), (start, end) => (start, end)).ToList();
        
        var offsetMap = new Dictionary<int, Block>();

        Block FindBlock(int offset) {
            return offsetMap.FirstOrDefault(x => x.Key >= offset).Value;
        }

        var entry = EntryBlock.Create(CFG);

        // Create blocks
        foreach (var (start, end) in offsets) {
            var blockStatements = statements.Skip(start).Take(end - start).ToList();

            if (blockStatements.Count == 0)
                continue;

            var block = BasicBlock.Create(CFG, blockStatements);
            Blocks.Add(block);
            offsetMap[start] = block;
        }

        CFG.Blocks.AddEdge(entry.Id, Blocks[0].Id);
        var exit = ExitBlock.Create(CFG);

        // Connect blocks
        for (int i = 0; i < Blocks.Count; i++) {
            var block = Blocks[i];
            var last = block.Statements.Last();
            
            var index = statements.IndexOf(last);
            if (!this.targets.ContainsKey(index)) {
                CFG.Blocks.AddEdge(block.Id, Blocks[i + 1].Id);
                continue;
            }
            
            var targets = this.targets[statements.IndexOf(last)];

            if (last is BranchStatement branch) {
                block.Statements.RemoveAt(block.Statements.Count - 1);

                var mediator = BranchBlock.Create(CFG.Blocks, branch.Variable);
                CFG.Blocks.AddEdge(block.Id, mediator.Id);

                foreach (var target in targets.AsEnumerable().Reverse()) {
                    CFG.Blocks.AddEdge(mediator.Id, FindBlock(target).Id);
                }
            }
            else if (last is GotoStatement @goto) {
                block.Statements.RemoveAt(block.Statements.Count - 1);

                CFG.Blocks.AddEdge(block.Id, offsetMap[statements.IndexOf(@goto.Target)].Id);
            }
            else if (last is ReturnStatement @return) {
                CFG.Blocks.AddEdge(block.Id, exit.Id);
            }
            else {
                foreach (var target in targets) {
                    CFG.Blocks.AddEdge(block.Id, FindBlock(target).Id);
                }
            }
        }
    }
}
