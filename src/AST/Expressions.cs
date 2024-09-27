using Mono.Cecil;

namespace CSynth.AST;

public abstract class Expression
{
    public abstract void Accept(ExpressionVisitor visitor);
}

public enum Operator {
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    And,
    Or
}

public class BinaryExpression : Expression
{
    public Expression Left { get; set; }
    public Expression Right { get; set; }
    public Operator Operator { get; set; }

    public BinaryExpression(Expression left, Expression right, Operator op)
    {
        Left = left;
        Right = right;
        Operator = op;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitBinaryExpression(this)) return;
        Left.Accept(visitor);
        Right.Accept(visitor);
    }

    public override string ToString() =>  $"({Left} {Operator} {Right})";
}

public class UnaryExpression : Expression
{
    public Expression Operand { get; set; }

    public UnaryExpression(Expression operand)
    {
        Operand = operand;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitUnaryExpression(this)) return;
        Operand.Accept(visitor);
    }

    public override string ToString() => $"!{Operand}";
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

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitBoolExpression(this);
    }

    public override string ToString() => Value ? "true" : "false";
}

public class NumberExpression : Expression
{
    public double Value { get; set; }

    public NumberExpression(double value)
    {
        Value = value;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitNumberExpression(this);
    }

    public override string ToString() => Value.ToString();
}

public class StringExpression : Expression
{
    public string Value { get; set; }

    public StringExpression(string value)
    {
        Value = value;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitStringExpression(this);
    }

    public override string ToString() => $"\"{Value}\"";
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

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitCallExpression(this)) return;
        foreach (var arg in Arguments) {
            arg.Accept(visitor);
        }
    }

    public override string ToString() => $"{Method.Name}({string.Join(", ", Arguments)})";
}

public abstract class Reference : Expression {
    public virtual string Name { get; set; } = "";
    public abstract string GetFullName();
}

public class VariableExpression : Reference
{
    public VariableExpression(string name) {
        Name = name;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitVariableExpression(this);
    }

    public override string GetFullName() => Name;
    public override string ToString() => Name;
}

public class FieldExpression : Reference
{
    public Reference Value { get; set; }
    public string Field => Name;

    public FieldExpression(string field, Reference value)
    {
        Name = field;
        Value = value;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitFieldExpression(this)) return;
        Value.Accept(visitor);
    }

    public override string GetFullName() => $"{Field}.{Name}";

    public override string ToString()
    {
        return GetFullName();
    }
}

public class IndexExpression : Reference
{
    public Reference Value { get; set; }
    public Expression Index { get; set; }

    public IndexExpression(Reference value, Expression index)
    {
        Value = value;
        Index = index;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitIndexExpression(this)) return;
        Value.Accept(visitor);
        Index.Accept(visitor);
    }

    public override string GetFullName() => $"{Value}[{Index}]";

    public override string ToString()
    {
        return GetFullName();
    }
}

public class CreateObjectExpression : Expression
{
    public TypeReference Type { get; set; }

    public CreateObjectExpression(TypeReference type)
    {
        Type = type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitCreateObjectExpression(this)) return;
    }

    public override string ToString() => $"new {Type.Name}";
}

public class TypeExpression : Reference
{
    public TypeReference Type { get; set; }

    public TypeExpression(TypeReference type)
    {
        Type = type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitTypeExpression(this);
    }

    public override string GetFullName() => Type.Name;
    public override string ToString() => Type.Name;
}

public class SelfExpression : Reference {

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitSelfExpression(this);
    }

    public override string GetFullName() => "self";
    public override string ToString() => "self";
}

public class ParameterExpression : Reference {
    public ParameterDefinition Parameter { get; set; }

    public ParameterExpression(ParameterDefinition parameter)
    {
        Parameter = parameter;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitParameterExpression(this);
    }

    public override string GetFullName() => Parameter.Name;
    public override string ToString() => Parameter.Name;
}

public class ArrayExpression : Expression
{
    public TypeReference Type { get; set; }
    public Expression Size { get; set; }

    public ArrayExpression(TypeReference type, Expression size)
    {
        Type = type;
        Size = size;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitArrayExpression(this)) return;
        Size.Accept(visitor);
    }

    public override string ToString() => $"new {Type.Name}[{Size}]";
}

public class LengthExpression : Expression {
    public Reference Value { get; set; }

    public LengthExpression(Reference value)
    {
        Value = value;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitLengthExpression(this)) return;
        Value.Accept(visitor);
    }

    public override string ToString() => $"{Value}.Length";
}
