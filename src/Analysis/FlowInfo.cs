using CSynth.AST;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public class FlowInfo
{
    public CFG CFG { get; set; } = new();
    public List<BasicBlock> Blocks { get; set; } = new();

    private class Chunk {
        public int Start { get; set; }
        public int End { get; set; }
        public required List<int> Targets { get; set; }
        public required List<Instruction> Instructions { get; set; }
    }

    private List<Chunk> chunks { get; set; } = new();

    internal FlowInfo() { }

    public static FlowInfo From(IList<Instruction> instructions)
    {
        var flowInfo = new FlowInfo();
        flowInfo.ChunkInstructions(instructions);
        flowInfo.BuildCFG();
        return flowInfo;
    }

    private void ChunkInstructions(IList<Instruction> instructions) {
        var chunkIndexes = new HashSet<int>() { 0, instructions.Count };

        for (int i = 0; i < instructions.Count; i++) {
            Instruction instruction = instructions[i];
            OpCode opCode = instruction.OpCode;

            if (instruction.Operand is Instruction target) {
                chunkIndexes.Add(i + 1);
                chunkIndexes.Add(instructions.IndexOf(target));
            }
        }

        var sorted = chunkIndexes.OrderBy(x => x);
        var pairs = sorted.Zip(sorted.Skip(1), (start, end) => (start, end))
            .ToList();

        int pairIndexOf(int index) {
            for (int i = 0; i < pairs.Count; i++) {
                var pair = pairs[i];
                if (pair.start <= index && index < pair.end) {
                    return i;
                }
            }
            return pairs.Count - 1;
        }

        for (int i = 0; i < pairs.Count; i++) {
            var (start, end) = pairs[i];

            var chunkInstructions = instructions.Skip(start).Take(end - start).ToList();
            var chunkTargets = new List<int>();

            var last = chunkInstructions.Last();
            
            switch (last.OpCode.FlowControl) {
                case FlowControl.Branch:
                    chunkTargets.Add(pairIndexOf(instructions.IndexOf((Instruction)last.Operand)));
                    break;
                case FlowControl.Cond_Branch:
                    chunkTargets.Add(pairIndexOf(instructions.IndexOf((Instruction)last.Operand)));
                    chunkTargets.Add(i + 1);
                    break;
                case FlowControl.Return:
                    chunkTargets.Add(-1);
                    break;
                default:
                    chunkTargets.Add(pairIndexOf(end + 1));
                    break;
            }

            chunks.Add(new Chunk {
                Start = start,
                End = end,
                Targets = chunkTargets,
                Instructions = chunkInstructions
            });
        }
    }

    private void BuildCFG() {
        var entry = EntryBlock.Create(CFG);
        var exit = ExitBlock.Create(CFG);

        var blocks = new List<BasicBlock>();

        foreach (var chunk in chunks) {
            var block = BasicBlock.Create(CFG, chunk.Instructions);
            blocks.Add(block);
        }

        CFG.Blocks.AddEdge(entry.Id, blocks[0].Id);

        for (int i = 0; i < blocks.Count; i++) {
            var block = blocks[i];
            var targets = chunks[i].Targets;

            foreach (var target in targets) {
                if (target == -1) {
                    CFG.Blocks.AddEdge(block.Id, exit.Id);
                } else {
                    CFG.Blocks.AddEdge(block.Id, blocks[target].Id);
                }
            }
        }
    }
}
