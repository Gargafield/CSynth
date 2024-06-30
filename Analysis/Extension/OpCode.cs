using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public static class OpCodeExtension
{
    public static bool IsConditionalBranch(this OpCode opCode) {
        return opCode.FlowControl == FlowControl.Cond_Branch;
    }

    public static bool IsUnconditionalBranch(this OpCode opCode) {
        return opCode.FlowControl == FlowControl.Branch;
    }

    public static bool IsReturn(this OpCode opCode) {
        return opCode.FlowControl == FlowControl.Return;
    }
}
