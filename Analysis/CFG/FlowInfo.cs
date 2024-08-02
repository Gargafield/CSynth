namespace CSynth.Analysis;

public class FlowInfo
{
    public CFG CFG { get; set; } = new();
    public List<Block> Blocks { get; set; } = new();

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
                blockOffsets.Add(branch.Offset - 1);
                AddTarget(branch.Offset, branch.Target);
                AddTarget(branch.Offset, branch.Offset + 1);
            }
            else if (statement is GotoStatement @goto) {
                AddTarget(@goto.Offset, @goto.Target);
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
        var targets = new Dictionary<Block, List<int>>();

        var entry = EntryBlock.Create(CFG);

        // Create blocks
        foreach (var (start, end) in offsets) {
            var blockStatements = statements.Where(x => x.Offset >= start && x.Offset < end).ToList();

            Block block;

            var last = blockStatements.Last();
            if (last is BranchStatement branch && blockStatements.Count == 1) {
                block = BranchBlock.Create(CFG, branch.Variable);
                targets[block] = new List<int> { branch.Target, branch.Offset + 1 };
            }
            else {
                block = BasicBlock.Create(CFG, blockStatements);
                targets[block] = this.targets[last.Offset];
            }

            Blocks.Add(block);
        }

        entry.AddTarget(Blocks[0]);
        var exit = ExitBlock.Create(CFG);

        // Add targets to blocks
        foreach (var block in Blocks) {
            if (targets.ContainsKey(block)) {
                foreach (var targetOffset in targets[block]) {

                    if (targetOffset == -1) {
                        block.AddTarget(exit);
                        continue;
                    }

                    var targetBlock = Blocks.First(x => x.Offset >= targetOffset);
                    block.AddTarget(targetBlock);
                }
            }
            else {
                var nextBlock = Blocks.First(x => x.Offset > lastInstruction.Offset);
                block.AddTarget(nextBlock);
            }
        }

        // Remove Branch and Goto statements
        foreach (var block in Blocks) {
            block.Statements.RemoveAll(x => x is BranchStatement || x is GotoStatement);
        }
    }
}
