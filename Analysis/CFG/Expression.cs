using Mono.Cecil;

namespace CSynth.Analysis;

public abstract class Expression
{ }

public class BinaryExpression : Expression
{
    public Expression Left { get; set; }
    public Expression Right { get; set; }
    public string Operator { get; set; }

    public BinaryExpression(Expression left, Expression right, string op)
    {
        Left = left;
        Right = right;
        Operator = op;
    }

    public override string ToString()
    {
        return $"({Left} {Operator} {Right})";
    }
}

public class BoolExpression : Expression
{
    public static BoolExpression True => new(true);
    public static BoolExpression False => new(false);
    
    public bool Value { get; set; }

    private BoolExpression(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}

public class NumberExpression : Expression
{
    public double Value { get; set; }

    public NumberExpression(double value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class StringExpression : Expression
{
    public string Value { get; set; }

    public StringExpression(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"\"{Value}\"";
    }
}

public class CallExpression : Expression
{
    public MethodReference Method { get; set; }
    public List<Expression> Arguments { get; set; }

    public CallExpression(MethodReference method, List<Expression> arguments)
    {
        Method = method;
        Arguments = arguments;
    }

    public override string ToString()
    {
        return $"{Method.Name}({string.Join(", ", Arguments)})";
    }
}

public class VariableExpression : Expression
{
    public string Name { get; set; }

    public VariableExpression(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class IfExpression : Expression {
    public Expression Condition { get; set; }
    public Expression Then { get; set; }
    public Expression Else { get; set; }

    public IfExpression(Expression condition, Expression then, Expression @else)
    {
        Condition = condition;
        Then = then;
        Else = @else;
    }

    public override string ToString()
    {
        return $"if ({Condition}) then {Then} else {Else}";
    }
}