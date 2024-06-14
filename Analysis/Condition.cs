namespace CSynth.Analysis;

public abstract class Condition : Expression
{ }

public class BinaryCondition : Condition
{
    public Expression Left { get; set; } = null!;
    public Expression Right { get; set; } = null!;
    public string Operator { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Left} {Operator} {Right}";
    }
}

public class TruthyCondition : Condition {
    public Expression Value { get; set; } = null!;

    public override string ToString() {
        return $"{Value}";
    }
}

public class FalsyCondition : Condition {
    public Expression Value { get; set; } = null!;

    public override string ToString() {
        return $"not {Value}";
    }
}