using Mono.Cecil;

namespace CSynth.Analysis;

public class AssemblyAnalyzer
{
    private AssemblyDefinition _assembly;

    internal AssemblyAnalyzer(AssemblyDefinition assembly) {
        _assembly = assembly;
    }

    public static AssemblyAnalyzer Create(string path)
    {
        var assembly = AssemblyDefinition.ReadAssembly(path);
        return new AssemblyAnalyzer(assembly);
    }

    public List<string> GetMethods()
    {
        var methods = _assembly.Modules.SelectMany(m => m.Types).SelectMany(t => t.Methods);
        return methods.Select(m => m.FullName).ToList();
    }
}
