using CSynth.Core;
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
            Console.WriteLine("Before: ");
            Console.WriteLine(cfg.ToMermaid());

            Restructure.RestructureCFG(cfg);
            
            Console.WriteLine("After: ");
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

            var moduleContext = new ModuleContext(type.Module);
            var typeContext = new TypeContext(type);
            Console.WriteLine(LuauWriter.Write(typeContext.Compile(context), moduleContext));
        }
        else {
            var path = args[0];
            var assembly = AssemblyDefinition.ReadAssembly(path);

            var moduleContext = new ModuleContext(assembly.MainModule);
            Console.WriteLine(LuauWriter.Write(moduleContext.Compile(context), moduleContext));
        }

    }
}
