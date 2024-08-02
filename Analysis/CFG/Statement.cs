using System.Text;

namespace CSynth.Analysis;

public abstract class Statement {
    public int Offset { get; set; }

    private Statement() { }

    public Statement(int offset) {
        Offset = offset;
    }
}

public class AssignmentStatement : Statement {
    public string Variable { get; set; }
    public Expression Expression { get; set; }

    public AssignmentStatement(int offset, string variable, Expression expression) : base(offset) {
        Variable = variable;
        Expression = expression;
    }

    public override string ToString() {
        return $"{Variable} = {Expression}";
    }
}

public class IfStatement : Statement {
    public List<Tuple<Expression, List<Statement>>> Conditions { get; set; } = new();

    public IfStatement(int offset, List<Tuple<Expression, List<Statement>>> conditions) : base(offset) {
        Conditions = conditions;
    }

    public override string ToString() {
        var builder = new StringBuilder();
        var first = true;
        foreach (var (expr, stats) in Conditions) {
            
            var stat = first ? "if" : "elseif";
            first = false;
            builder.Append($"{stat} ({expr}) then\n");
            foreach (var statement in stats) {
                builder.Append(statement);
                builder.Append("\n");
            }
            builder.Append("end\n");
        }

        return builder.ToString();
    }
}

public class LoopStatement : Statement {
    public List<Statement> Body { get; set; }
    public Expression Condition { get; set; }

    public LoopStatement(int offset, List<Statement> body, Expression condition) : base(offset) {
        Body = body;
        Condition = condition;
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append("repeat\n");
        foreach (var statement in Body) {
            builder.Append(statement);
            builder.Append("\n");
        }
        builder.Append("until ");
        builder.Append(Condition);
        builder.Append("\n");
        return builder.ToString();
    }
}


public class GotoStatement : Statement {
    public int Target { get; set; }

    public GotoStatement(int offset, int target) : base(offset) {
        Target = target;
    }

    public override string ToString() {
        return $"goto {Target}";
    }
}

public class BranchStatement : GotoStatement {
    public string Variable { get; set; }

    public BranchStatement(int offset, string variable, int target) : base(offset, target) {
        Variable = variable;
        Target = target;
    }

    public override string ToString() {
        return $"if {Variable} then goto {Target}";
    }
}

public class ReturnStatement : Statement {
    public Expression? Expression { get; set; }

    public ReturnStatement(int offset, Expression? expression) : base(offset) {
        Expression = expression;
    }

    public override string ToString() {
        return $"return {Expression}";
    }
}

public class DoWhileStatement : Statement {
    public List<Statement> Body { get; set; }
    public Expression Condition { get; set; }

    public DoWhileStatement(int offset, List<Statement> body, Expression condition) : base(offset) {
        Body = body;
        Condition = condition;
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append("do\n");
        foreach (var statement in Body) {
            builder.Append(statement);
            builder.Append("\n");
        }
        builder.Append("while ");
        builder.Append(Condition);
        builder.Append("\n");
        return builder.ToString();
    }
}