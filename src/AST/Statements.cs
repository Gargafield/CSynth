using System.Text;
using Mono.Cecil;

namespace CSynth.AST;

public abstract class Statement { }

public class AssignmentStatement : Statement {
    public Reference Variable { get; set; }
    public Expression Expression { get; set; }

    public AssignmentStatement(string variable, Expression expression) {
        Variable = new VariableExpression(variable);
        Expression = expression;
    }

    public AssignmentStatement(Reference variable, Expression expression) {
        Variable = variable;
        Expression = expression;
    }

    public override string ToString() {
        return $"{Variable} = {Expression}";
    }
}

public class IfStatement : Statement {
    public List<Tuple<Expression?, List<Statement>>> Conditions { get; set; } = new();

    public IfStatement(List<Tuple<Expression?, List<Statement>>> conditions) {
        Conditions = conditions;
    }

    public override string ToString() {
        var builder = new StringBuilder();
        var first = true;
        foreach (var (expr, stats) in Conditions) {
            
            var stat = first ? "if" : (expr == null ? "else" : "elseif");
            first = false;
            builder.Append($"{stat} ({expr}) then\n");
            foreach (var statement in stats) {
                builder.Append(statement);
                builder.Append("\n");
            }
        }
        builder.Append("end\n");

        return builder.ToString();
    }
}

public class ThrowStatement : Statement {
    public Expression Expression { get; set; }

    public ThrowStatement(Expression expression) {
        Expression = expression;
    }

    public override string ToString() {
        return $"throw {Expression}";
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


public class GotoStatement : Statement {
    public Statement Target { get; set; }

    public GotoStatement(Statement target) {
        Target = target;
    }

    public override string ToString() {
        return $"goto {Target}";
    }
}

public class BranchStatement : GotoStatement {
    public string Variable { get; set; }

    public BranchStatement(string variable, Statement target) : base(target) {
        Variable = variable;
        Target = target;
    }

    public override string ToString() {
        return $"if {Variable} then goto {Target}";
    }
}

public class ReturnStatement : Statement {
    public Expression? Expression { get; set; }

    public ReturnStatement(Expression? expression) {
        Expression = expression;
    }

    public override string ToString() {
        return $"return {Expression}";
    }
}

public class DoWhileStatement : Statement {
    public List<Statement> Body { get; set; }
    public Expression Condition { get; set; }

    public DoWhileStatement(List<Statement> body, Expression condition) {
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

public class DefineVariablesStatement : Statement {
    public List<string> Variables { get; set; }

    public DefineVariablesStatement(List<string> variables) {
        Variables = variables;
    }

    public override string ToString() {
        return $"local {string.Join(", ", Variables)}";
    }
}

public class CallStatement : Statement {
    public CallExpression Expression { get; set; }

    public CallStatement(CallExpression expression) {
        Expression = expression;
    }

    public override string ToString() {
        return Expression.ToString();
    }
}

public class ModuleDefinitionStatement : Statement {
    public ModuleDefinition Module { get; set; }

    public ModuleDefinitionStatement(ModuleDefinition module) {
        Module = module;
    }

    public override string ToString() {
        return $"module {Module.Name}";
    }
}

public class MethodDefinitionStatement : Statement {
    public MethodDefinition Method { get; set; }
    public List<Statement> Body { get; set; }

    public MethodDefinitionStatement(MethodDefinition method, List<Statement> body) {
        Method = method;
        Body = body;
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append($"function {Method.Name}(");
        builder.Append(string.Join(", ", Method.Parameters.Select(p => p.Name)));
        builder.Append(")\n");
        foreach (var statement in Body) {
            builder.Append(statement);
            builder.Append("\n");
        }
        builder.Append("end\n");
        return builder.ToString();
    }
}

public class TypeDefinitionStatement : Statement {
    public TypeDefinition Type { get; set; }

    public TypeDefinitionStatement(TypeDefinition type) {
        Type = type;
    }

    public override string ToString() {
        return $"type {Type.Name}";
    }
}
