
using System.Diagnostics;
using CSynth.Core;
using ILConformance;
using Mono.Cecil;

namespace CSynth.Conformance;

public static class Runner {

    private static void WriteConformanceTests() {
        var context = new TranslationContext() {
            Debug = false
        };

        var assemblyPath = typeof(Add_I4_Conformance).Assembly.Location;
        var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        var moduleContext = new ModuleContext(assembly.MainModule);

        foreach (var type in assembly.MainModule.Types) {
            if (!type.FullName.EndsWith("Conformance")) {
                continue;
            }

            var typeContext = new TypeContext(type);
            var statements = typeContext.Compile(context);
            
            var function = new MethodExpression(type.Methods.First(m => m.Name == "Main"));
            var expression = new CallExpression(function, new List<Expression>());
            statements.Add(new CallStatement(expression));

            var luau = LuauWriter.Write(statements, moduleContext);
            var path = $"{type}.lua";
            File.WriteAllText(path, luau);
        }
    }

    private static void RunConformanceTests() {
        // use 'lune' to run each lua file in tests/Conformance/bin
        // look for failed in process output

        var files = Directory.GetFiles("./", "*.lua");
        foreach (var file in files) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "lune",
                    Arguments = "run " + file,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0) {
                Assert.Fail($"Conformance test failed: {file}, error: {error}");
            }

            if (output.Contains("failed")) {
                Assert.Fail($"Conformance test failed: {file}, output: {output}");
            }
        }
    }

    [Fact]
    public static void RunConformance() {
        WriteConformanceTests();
        RunConformanceTests();
    }
}