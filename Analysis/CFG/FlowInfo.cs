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

    public static FlowInfo From(ICollection<Statement> statements)
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

    private void Build(ICollection<Statement> statements) {
        blockOffsets.Add(0);

        foreach (var statement in statements) {

            if (statement is BranchStatement branch) {
                AddTarget(branch.Offset, branch.Target);
                AddTarget(branch.Offset, branch.Offset + 1);
            }
            else if (statement is GotoStatement @goto) {
                AddTarget(@goto.Offset, @goto.Target);
                blockOffsets.Add(@goto.Offset + 1);
            }
            else if (statement is ReturnStatement @return) {
                AddTarget(@return.Offset, -1);
            }
        }

        var last = statements.Last();
        returnOffset = last.Offset + 1;
        blockOffsets.Add(returnOffset);
    }

    private void BuildCFG(ICollection<Statement> statements) {
        
        // Collect block offsets into pairs start, end
        var orderedOffsets = blockOffsets.OrderBy(x => x)
            .Take(blockOffsets.Count);
        var offsets = orderedOffsets.Zip(orderedOffsets.Skip(1), (start, end) => (start, end)).ToList();
        
        var offsetMap = new Dictionary<int, Block>();

        var entry = EntryBlock.Create(CFG);

        // Create blocks
        foreach (var (start, end) in offsets) {
            var blockStatements = statements.Where(x => x.Offset >= start && x.Offset < end).ToList();

            if (blockStatements.Count == 0)
                continue;

            var block = BasicBlock.Create(CFG, blockStatements);
            Blocks.Add(block);
            offsetMap[start] = block;

            if (!targets.ContainsKey(blockStatements.Last().Offset)) {
                // Add target to next block
                AddTarget(blockStatements.Last().Offset, end);
            }
        }

        entry.AddTarget(Blocks[0]);
        var exit = ExitBlock.Create(CFG);

        // Connect blocks
        foreach (var block in Blocks) {
            var last = block.Statements.Last();
            var targets = this.targets[last.Offset];

            if (last is BranchStatement branch) {
                block.Statements.RemoveAt(block.Statements.Count - 1);

                var mediator = BranchBlock.Create(CFG, branch.Variable);
                block.AddTarget(mediator);

                foreach (var target in targets) {
                    mediator.AddTarget(offsetMap[target]);
                }
            }
            else if (last is GotoStatement @goto) {
                block.Statements.RemoveAt(block.Statements.Count - 1);

                block.AddTarget(offsetMap[@goto.Target]);
            }
            else if (last is ReturnStatement @return) {
                block.AddTarget(exit);
            }
            else {
                foreach (var target in targets) {
                    block.AddTarget(offsetMap[target]);
                }
            }
        }
    }
}
