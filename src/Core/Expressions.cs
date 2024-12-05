using Mono.Cecil;

namespace CSynth.Core;

public abstract class Expression
{
    public TypeReference Type { get; set; }

    protected Expression(TypeReference type) {
        Type = type;
    }

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

    public BinaryExpression(Expression left, Expression right, Operator op, TypeReference type) : base(type)
    {
        Left = left;
        Right = right;
        Operator = op;
    }

    public override string ToString() =>  $"({Left} {Operator} {Right})";
}

public class UnaryExpression : Expression
{
    public Expression Operand { get; set; }

    public UnaryExpression(Expression operand, TypeReference type) : base(type) {
        Operand = operand;
    }

    public override string ToString() => $"!{Operand}";
}

public class BoolExpression : Expression
{
    public static BoolExpression True => new(true);
    public static BoolExpression False => new(false);
    
    public bool Value { get; set; }

    private BoolExpression(bool value) : base(TypeResolver.BoolType) {
        Value = value;
    }

    public override string ToString() => Value ? "true" : "false";
}

public class NumberExpression : Expression
{
    public object Value { get; set; }

    public NumberExpression(object value, TypeReference type) : base(type) {
        Value = value;
    }

    public override string ToString() => Value.ToString()!;
}

public class StringExpression : Expression
{
    public string Value { get; set; }

    public StringExpression(string value) : base(TypeResolver.StringType) {
        Value = value;
    }

    public override string ToString() => $"\"{Value}\"";
}

public class ByteArrayExpression : Expression {
    private static TypeReference _type = TypeResolver.GetTypeReference<byte[]>();
    public byte[] Value { get; set; }

    public ByteArrayExpression(byte[] value) : base(_type) {
        Value = value;
    }

    public override string ToString() => $"byte[{Value.Length}]";
}

public class NullExpression : Expression
{
    public NullExpression() : base(TypeResolver.ObjectType) {}
    public override string ToString() => "null";
}

public abstract class FunctionExpression : Expression
{
    public abstract IMethodSignature GetMethodSignature();
    protected FunctionExpression(TypeReference type) : base(type) {}
}

public class RuntimeMethodExpression : FunctionExpression {
    public RuntimeMethodSignature Method { get; set; }

    public RuntimeMethodExpression(RuntimeMethodSignature method) : base(method.ReturnType) {
        Method = method;
    }

    public override IMethodSignature GetMethodSignature() => Method;

    public override string ToString() => Method.Name;
}

public class MethodExpression : FunctionExpression {
    public MethodReference Method { get; set; }

    public MethodExpression(MethodReference method) : base(method.ReturnType) {
        Method = method;
    }

    public override IMethodSignature GetMethodSignature() => Method;

    public override string ToString() => Method.Name;
}

public class LambdaExpression : FunctionExpression {
    public CallSite Signature { get; set; }
    public Expression Function { get; set; }

    public LambdaExpression(CallSite signature, Expression function) : base(signature.ReturnType)
    {
        Signature = signature;
        Function = function;
    }

    public override IMethodSignature GetMethodSignature() => Signature;

    public override string ToString() => $"{Function}";
}

public class VirtualFunctionExpression : FunctionExpression {
    public Expression Expression { get; set; }
    public MethodReference Method { get; set; }

    public VirtualFunctionExpression(Expression expression, MethodReference method)
        : base(method.ReturnType)
    {
        Expression = expression;
        Method = method;
    }

    public override IMethodSignature GetMethodSignature() => Method;

    public override string ToString() => $"{Expression}.{Method.Name}";
}

public class CallExpression : Expression {
    public FunctionExpression Function { get; set; }
    public List<Expression> Arguments { get; set; }

    public CallExpression(FunctionExpression function, List<Expression> arguments)
        : base(function.Type)
    {
        Function = function;
        Arguments = arguments;
    }

    public override string ToString() => $"{Function}({string.Join(", ", Arguments)})";
}

public abstract class Reference : Expression {
    public virtual string Name { get; set; } = "";
    public abstract string GetFullName();

    protected Reference(TypeReference type) : base(type) {}
}

public class VariableExpression : Reference
{
    public VariableExpression(string name, TypeReference type) : base(type) {
        Name = name;
    }

    public override string GetFullName() => Name;
    public override string ToString() => Name;
}

public class FieldExpression : Reference
{
    public Reference Value { get; set; }
    public string Field => Name;

    public FieldExpression(string field, Reference value) : base(value.Type) {
        Name = field;
        Value = value;
    }

    public override string GetFullName() => $"{Field}.{Name}";
    public override string ToString() => GetFullName();
}

public class IndexExpression : Reference
{
    public Reference Value { get; set; }
    public Expression Index { get; set; }

    public IndexExpression(Reference value, Expression index) : base(value.Type.GetElementType()) {
        Value = value;
        Index = index;
    }

    public override string GetFullName() => $"{Value}[{Index}]";
    public override string ToString() => GetFullName();
}

public class CreateObjectExpression : Expression
{
    public CreateObjectExpression(TypeReference type) : base(type) {}

    public override string ToString() => $"new {Type.Name}";
}

public class TypeExpression : Reference
{
    public TypeExpression(TypeReference type) : base(type) {}

    public override string GetFullName() => Type.Name;
    public override string ToString() => Type.Name;
}

public class SelfExpression : Reference {
    public SelfExpression(TypeReference type) : base(type) { }

    public override string GetFullName() => "self";
    public override string ToString() => "self";
}

public class ParameterExpression : Reference {
    public ParameterDefinition Parameter { get; set; }

    public ParameterExpression(ParameterDefinition parameter) : base(parameter.ParameterType) {
        Parameter = parameter;
    }

    public override string GetFullName() => Parameter.Name;
    public override string ToString() => Parameter.Name;
}

public class ArrayExpression : Expression
{
    public TypeReference ElementType => Type.GetElementType();
    public Expression Size { get; set; }

    public ArrayExpression(TypeReference type, Expression size)
        : base(TypeResolver.ArrayOf(type))
    {
        Size = size;
    }

    public override string ToString() => $"new {Type.Name}[{Size}]";
}

public class LengthExpression : Expression {
    public Reference Value { get; set; }

    public LengthExpression(Reference value) : base(TypeResolver.Int32Type) {
        Value = value;
    }

    public override string ToString() => $"{Value}.Length";
}
