
using Mono.Cecil;

namespace CSynth.AST;

public abstract class Syscall : Expression {
    public abstract string Name { get; }
    public TypeReference Return => Type;
    public abstract IEnumerable<Expression> Arguments { get; }

    public virtual string GetFullName() => Name;
    public virtual Expression? Emit() => null;

    protected Syscall(TypeReference returnType) : base(returnType) { }

    public Expression GenerateRuntimeCall() {
        return new CallExpression(new RuntimeMethodExpression(RuntimeMethods.Methods[GetFullName()]), Arguments.ToList());
    }
}

public class AddSyscall : Syscall {
    public override string Name => "add";
    public override IEnumerable<Expression> Arguments => [Left, Right];

    public Expression Left { get; }
    public Expression Right { get; }

    public AddSyscall(Expression left, Expression right) : base(left.Type) {
        Left = left;
        Right = right;
    }

    public override string ToString() => $"{Left} + {Right}";
    public override Expression? Emit() {
        if (Type.Name == "Int32") {
            var add = new BinaryExpression(Left, Right, Operator.Add, Type);
            return new BinaryExpression(add, new NumberExpression(0, TypeResolver.Int32Type), Operator.BitwiseOr, Type);
        }

        return null;
    }
    public override string GetFullName()
    {
        return $"{Name}_{TypeResolver.GetName(Type)}";
    }
}

public class SubSyscall : Syscall {
    public override string Name => "sub";
    public override IEnumerable<Expression> Arguments => [Left, Right];

    public Expression Left { get; }
    public Expression Right { get; }

    public SubSyscall(Expression left, Expression right) : base(left.Type) {
        Left = left;
        Right = right;
    }

    public override string ToString() => $"{Left} - {Right}";
    public override Expression? Emit() {
        if (Type.Name == "Int32") {
            var add = new BinaryExpression(Left, Right, Operator.Subtract, Type);
            return new BinaryExpression(add, new NumberExpression(0, TypeResolver.Int32Type), Operator.BitwiseOr, Type);
        }

        return null;
    }
    public override string GetFullName()
    {
        return $"{Name}_{TypeResolver.GetName(Type)}";
    }
}

public class EqSyscall : Syscall {
    public override string Name => "eq";
    public override IEnumerable<Expression> Arguments => [Left, Right];

    public Expression Left { get; }
    public Expression Right { get; }

    public EqSyscall(Expression left, Expression right) : base(TypeResolver.BoolType) {
        Left = left;
        Right = right;
    }

    public override string ToString() => $"{Left} == {Right}";
    public override Expression? Emit() {
        if (Left.Type.Name == "Int32") {
            return new BinaryExpression(Left, Right, Operator.Equal, TypeResolver.BoolType);
        }

        return null;
    }
    public override string GetFullName()
    {
        return $"{Name}_{TypeResolver.GetName(Type)}";
    }
}

public class BoolSyscall : Syscall {
    public override string Name => "bool";
    public override IEnumerable<Expression> Arguments => [Expression];

    public Expression Expression { get; }

    public BoolSyscall(Expression expression) : base(TypeResolver.BoolType) {
        Expression = expression;
        if (expression.Type == null)
            throw new Exception("Expression type is null");
    }

    public override string ToString() => $"!!{Expression}";
    public override Expression? Emit() {
        
        switch (Expression.Type.Name) {
            case "Int32":
                return new BinaryExpression(Expression, new NumberExpression(0, TypeResolver.Int32Type), Operator.NotEqual, TypeResolver.BoolType);
            case "Boolean":
                return Expression;
        }

        return null;
    }
    public override string GetFullName() {
        return $"{Name}_{TypeResolver.GetName(Type)}";
    }
}
