using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public abstract class Branch
{ }

public class FalseBranch : Branch {
    public Condition Condition { get; set; } = null!;

    public override string ToString() {
        return $"!{Condition} do";
    }
}

public class TrueBranch : Branch {
    public Condition Condition { get; set; } = null!;

    public override string ToString() {
        return $"{Condition} do";
    }
}
