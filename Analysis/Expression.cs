namespace CSynth.Analysis;

public abstract class Expression {}

public class CallExpression : Expression {
    public string Method { get; set; } = string.Empty;
    public List<Expression> Arguments { get; set; } = new();

    public override string ToString() {
        return $"{Method}({string.Join(", ", Arguments)})";
    }
}

public class BinaryExpression : Expression {
    public Expression Left { get; set; } = null!;
    public Expression Right { get; set; } = null!;
    public string Operator { get; set; } = string.Empty;

    public override string ToString() {
        return $"{Left} {Operator} {Right}";
    }
}
