
using Mono.Cecil;

public static class TypeResolver {
    private static AssemblyDefinition _assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);

    public static TypeReference BoolType => _assembly.MainModule.ImportReference(typeof(bool));
    public static TypeReference Int32Type => _assembly.MainModule.ImportReference(typeof(int));
    public static TypeReference Int64Type => _assembly.MainModule.ImportReference(typeof(long));
    public static TypeReference FloatType => _assembly.MainModule.ImportReference(typeof(float));
    public static TypeReference DoubleType => _assembly.MainModule.ImportReference(typeof(double));
    public static TypeReference StringType => _assembly.MainModule.ImportReference(typeof(string));
    public static TypeReference VoidType => _assembly.MainModule.ImportReference(typeof(void));
    public static TypeReference ObjectType => _assembly.MainModule.ImportReference(typeof(object));

    public static TypeReference GetTypeReference<T>() {
        return _assembly.MainModule.ImportReference(typeof(T));
    }

    public static string GetName(TypeReference type) {
        // TODO: Better naming
        return type.Name;
    }

    public static TypeReference ArrayOf(TypeReference type) {
        return new ArrayType(type);
    }
}