namespace CSynth.Analysis;

public abstract class Statement { }

public class CallStatement : Statement {
    public string Method { get; set; } = string.Empty;
    public List<Expression> Arguments { get; set; } = new();

    public override string ToString() {
        return $"{Method}({string.Join(", ", Arguments)})";
    }
}

public class AssignmentStatement : Statement {
    public Variable Variable { get; set; } = null!;
    public Expression Value { get; set; } = null!;

    public override string ToString() {
        return $"{Variable} = {Value}";
    }
}

public class ReturnStatement : Statement {
    public Expression Value { get; set; } = null!;

    public override string ToString() {
        return $"return {Value}";
    }
}

public class IfStatement : Statement {
    public Branch Branch { get; set; } = null!;
    public List<Statement> Statements { get; set; } = new();

    public override string ToString() {
        return $"if ({Branch}) {{\n{string.Join("\n", Statements)}\n}}";
    }
}