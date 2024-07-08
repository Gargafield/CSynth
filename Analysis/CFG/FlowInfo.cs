using System.Diagnostics;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace CSynth.Analysis;

public class FlowInfo
{
    public CFG CFG { get; set; } = new();
    public List<BasicBlock> Blocks { get; set; } = new();

    private HashSet<int> blockOffsets { get; set; } = new();
    private Dictionary<int, List<int>> targets { get; set; } = new();
    private int returnOffset { get; set; }

    internal FlowInfo() { }

    public static FlowInfo From(Collection<Instruction> instructions)
    {
        var flowInfo = new FlowInfo();
        flowInfo.Build(instructions);
        flowInfo.BuildCFG(instructions);
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

    private void Build(Collection<Instruction> instructions) {
        blockOffsets.Add(0);

        foreach (var instruction in instructions) {
            var opCode = instruction.OpCode;

            if (opCode.IsConditionalBranch()) {
                var target = (Instruction)instruction.Operand;
                AddTarget(instruction.Offset, target.Offset);
                AddTarget(instruction.Offset, instruction.Next.Offset);
            }
            else if (opCode.IsUnconditionalBranch()) {
                var target = (Instruction)instruction.Operand;
                AddTarget(instruction.Offset, target.Offset);
            }
            else if (opCode.IsReturn()) {
                AddTarget(instruction.Offset, -1);
            }
        }

        var lastInstruction = instructions.Last();
        returnOffset = lastInstruction.Offset + lastInstruction.GetSize();
        blockOffsets.Add(returnOffset);
    }

    private void BuildCFG(Collection<Instruction> instructions) {
        
        // Collect block offsets into pairs start, end
        var orderedOffsets = blockOffsets.OrderBy(x => x)
            .Take(blockOffsets.Count);
        var offsets = orderedOffsets.Zip(orderedOffsets.Skip(1), (start, end) => (start, end)).ToList();

        var entry = EntryBlock.Create(CFG);

        // Create blocks
        foreach (var (start, end) in offsets) {
            var blockInstructions = instructions.Where(x => x.Offset >= start && x.Offset < end).ToList();
            var block = BasicBlock.Create(CFG, blockInstructions);
            Blocks.Add(block);
        }

        entry.AddTarget(Blocks[0]);
        var exit = ExitBlock.Create(CFG);

        // Add targets to blocks
        foreach (var block in Blocks) {
            var lastInstruction = block.Instructions.Last();

            if (targets.ContainsKey(lastInstruction.Offset)) {
                foreach (var targetOffset in targets[lastInstruction.Offset]) {

                    if (targetOffset == -1) {
                        block.AddTarget(exit);
                        continue;
                    }

                    var targetBlock = Blocks.First(x => x.Offset == targetOffset);
                    block.AddTarget(targetBlock);
                }
            }
            else {
                block.AddTarget(Blocks.First(x => x.Offset == lastInstruction.Next.Offset));
            }
        }
    }
}
