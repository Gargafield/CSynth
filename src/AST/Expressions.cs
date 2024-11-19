using Mono.Cecil;

namespace CSynth.AST;

public abstract class Expression
{
    public virtual TypeReference Type { get; set; } = null!;
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
    Or,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitwiseNot,
    RightShift,
    LeftShift,
}

public class BinaryExpression : Expression
{
    public Expression Left { get; set; }
    public Expression Right { get; set; }
    public Operator Operator { get; set; }

    public BinaryExpression(Expression left, Expression right, Operator op, TypeReference type)
    {
        Left = left;
        Right = right;
        Operator = op;
        Type = type;
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

    public UnaryExpression(Expression operand, TypeReference type)
    {
        Operand = operand;
        Type = type;
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
        Type = TypeResolver.BoolType;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitBoolExpression(this);
    }

    public override string ToString() => Value ? "true" : "false";
}

public class NumberExpression : Expression
{
    public object Value { get; set; }

    public NumberExpression(object value, TypeReference type)
    {
        Value = value;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitNumberExpression(this);
    }

    public override string ToString() => Value.ToString()!;
}

public class StringExpression : Expression
{
    public string Value { get; set; }

    public StringExpression(string value)
    {
        Value = value;
        Type = TypeResolver.StringType;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitStringExpression(this);
    }

    public override string ToString() => $"\"{Value}\"";
}

public class ByteArrayExpression : Expression {
    private static TypeReference _type = TypeResolver.GetTypeReference<byte[]>();
    public byte[] Value { get; set; }

    public ByteArrayExpression(byte[] value)
    {
        Value = value;
        Type = _type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitByteArrayExpression(this);
    }

    public override string ToString() => $"byte[{Value.Length}]";
}

public class NullExpression : Expression
{
    public NullExpression()
    {
        Type = TypeResolver.ObjectType;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitNullExpression(this);
    }

    public override string ToString() => "null";
}

public abstract class FunctionExpression : Expression
{
    public abstract IMethodSignature GetMethodSignature();
}

public class MethodExpression : FunctionExpression {
    public MethodReference Method { get; set; }

    public MethodExpression(MethodReference method)
    {
        Method = method;
        Type = method.ReturnType;
    }

    public override IMethodSignature GetMethodSignature() => Method;

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitMethodExpression(this);
    }

    public override string ToString() => Method.Name;
}

public class LambdaExpression : FunctionExpression {
    public CallSite Signature { get; set; }
    public Expression Function { get; set; }

    public LambdaExpression(CallSite signature, Expression function)
    {
        Signature = signature;
        Function = function;
        Type = signature.ReturnType;
    }

    public override IMethodSignature GetMethodSignature() => Signature;

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitLambdaExpression(this)) return;
        Function.Accept(visitor);
    }

    public override string ToString() => $"{Function}";
}

public class VirtualFunctionExpression : FunctionExpression {
    public Expression Expression { get; set; }
    public MethodReference Method { get; set; }

    public VirtualFunctionExpression(Expression expression, MethodReference method)
    {
        Expression = expression;
        Method = method;
        Type = method.ReturnType;
    }

    public override IMethodSignature GetMethodSignature() => Method;

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitVirtualFunctionExpression(this);
    }

    public override string ToString() => $"{Expression}.{Method.Name}";
}

public class CallExpression : Expression {
    public FunctionExpression Function { get; set; }
    public List<Expression> Arguments { get; set; }

    public CallExpression(FunctionExpression function, List<Expression> arguments)
    {
        Function = function;
        Arguments = arguments;
        Type = function.Type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitCallExpression(this)) return;
        foreach (var arg in Arguments) {
            arg.Accept(visitor);
        }
    }

    public override string ToString() => $"{Function}({string.Join(", ", Arguments)})";
}

public abstract class Reference : Expression {
    public virtual string Name { get; set; } = "";
    public abstract string GetFullName();
}

public class VariableExpression : Reference
{
    public VariableExpression(string name, TypeReference type) {
        Name = name;
        Type = type;
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
        Type = value.Type;
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
        Type = value.Type;
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
    public SelfExpression(TypeReference type) {
        Type = type;
    }

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
        Type = parameter.ParameterType;
    }

    public override void Accept(ExpressionVisitor visitor) {
        visitor.VisitParameterExpression(this);
    }

    public override string GetFullName() => Parameter.Name;
    public override string ToString() => Parameter.Name;
}

public class ArrayExpression : Expression
{
    public Expression Size { get; set; }

    public ArrayExpression(TypeReference type, Expression size) {
        Size = size;
        Type = type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitArrayExpression(this)) return;
        Size.Accept(visitor);
    }

    public override string ToString() => $"new {Type.Name}[{Size}]";
}

public class LengthExpression : Expression {
    public Reference Value { get; set; }

    public LengthExpression(Reference value) {
        Value = value;
        Type = TypeResolver.Int32Type;
    }

    public override void Accept(ExpressionVisitor visitor) {
        if (!visitor.VisitLengthExpression(this)) return;
        Value.Accept(visitor);
    }

    public override string ToString() => $"{Value}.Length";
}
