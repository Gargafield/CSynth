using System.Text;

namespace CSynth.Compiler;

public abstract class Statement {
}

public class IfStatement : Statement {
    public List<Tuple<Expression, List<Statement>>> Conditions { get; set; } = new();

    public IfStatement(List<Tuple<Expression, List<Statement>>> conditions) {
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

    public LoopStatement(List<Statement> body, Expression condition) {
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

public class ExpressionStatement : Statement {
    public Expression Expression { get; set; }

    public ExpressionStatement(Expression expression) {
        Expression = expression;
    }

    public override string ToString() {
        return "ERROR!"; // Should not leak to output
    }
}

public class InstructionStatement : Statement {
    public string Instruction { get; set; }

    public InstructionStatement(string instruction) {
        Instruction = instruction;
    }

    public override string ToString() {
        return Instruction;
    }
}


