using CSynth.AST;
using CSynth.Compiler;
using CSynth.Transformation;
using Mono.Cecil;

namespace CSynth.CLI;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: CSynth <assembly>");
            return;
        }

        var debug = args.Contains("--debug") || args.Contains("-d");

        if (args.Contains("--graph")) {
            var path = args[0];
            var text = File.ReadAllText(path);
            var cfg = CFG.FromEquality(text);
            Restructure.RestructureCFG(cfg);
            Console.WriteLine(cfg.ToMermaid());
            return;
        }

        var context = new TranslationContext() {
            Debug = debug
        };
        
        var filter = args.FirstOrDefault(x => x.StartsWith("--filter=") || x.StartsWith("-f="));
        if (filter != null)
        {
            // Find the type with the given name
            var typeName = filter.Split('=')[1];
            var type = AssemblyDefinition.ReadAssembly(args[0]).MainModule.Types.FirstOrDefault(x => x.FullName == typeName);
            if (type == null)
            {
                Console.WriteLine($"Type {typeName} not found");
                return;
            }

            var moduleContext = new ModuleContext(type.Module, context);
            var typeContext = new TypeContext(type, context);
            var _type = new Compiler.Type(typeContext);
            Console.WriteLine(LuauWriter.Write(_type.Compile(), moduleContext));
        }
        else {
            var path = args[0];
            var assembly = AssemblyDefinition.ReadAssembly(path);

            var moduleContext = new ModuleContext(assembly.MainModule, context);

            var module = new Module(moduleContext);

            Console.WriteLine(LuauWriter.Write(module.Compile(), moduleContext));
        }

    }
}
