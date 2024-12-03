
using Mono.Cecil;
using Mono.Collections.Generic;

namespace CSynth.AST;

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
}

public static class RuntimeMethods {
    public static RuntimeMethodSignature AddInt32 = new RuntimeMethodSignature("add_int32", TypeResolver.Int32Type, new ParameterDefinition(TypeResolver.Int32Type), new ParameterDefinition(TypeResolver.Int32Type));
    
    public static Dictionary<string, RuntimeMethodSignature> Methods = new() {
        { "add_int32", AddInt32 }
    };
}
