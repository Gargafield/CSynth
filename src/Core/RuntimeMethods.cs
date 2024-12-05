
using Mono.Cecil;
using Mono.Collections.Generic;

namespace CSynth.Core;

public class RuntimeMethodSignature : IMethodSignature
{
    public bool HasThis { get => false; set => throw new NotImplementedException(); }
    public bool ExplicitThis { get => false; set => throw new NotImplementedException(); }
    public MethodCallingConvention CallingConvention { get => MethodCallingConvention.Default; set => throw new NotImplementedException(); }

    public bool HasParameters => Parameters.Count > 0;

    private Collection<ParameterDefinition> _parameters = new Collection<ParameterDefinition>();
    public Collection<ParameterDefinition> Parameters => _parameters;


    private TypeReference _returnType = null!;
    public TypeReference ReturnType { get => _returnType; set => throw new NotImplementedException(); }

    public MethodReturnType MethodReturnType => throw new NotImplementedException();

    public MetadataToken MetadataToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Name { get; set; } = null!;

    public RuntimeMethodSignature(string name, TypeReference returnType, params ParameterDefinition[] parameters)
    {
        Name = name;
        _returnType = returnType;
        foreach (var parameter in parameters)
        {
            _parameters.Add(parameter);
        }
    }

    public override string ToString() => Name;
}

public static class RuntimeMethods {
    private static ParameterDefinition _int32Parameter = new ParameterDefinition(TypeResolver.Int32Type);

    public static RuntimeMethodSignature AddInt32 = new RuntimeMethodSignature("add_Int32", TypeResolver.Int32Type, _int32Parameter, _int32Parameter);
    public static RuntimeMethodSignature MulInt32 = new RuntimeMethodSignature("mul_Int32", TypeResolver.Int32Type, _int32Parameter, _int32Parameter);

    public static Dictionary<string, RuntimeMethodSignature> Methods = new() {
        { "add_Int32", AddInt32 },
        { "mul_Int32", MulInt32 }
    };
}
