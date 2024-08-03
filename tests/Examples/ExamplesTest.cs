using CSynth.Analysis;
using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Test;

public class ExamplesTest
{
    internal static void Example1() {
        for (int i = 0; i < 5; i++) {
            Console.WriteLine("Hello, World!");
        }
    }
    private const string ExpectedExample1 = """
    local loc0, loc1, condition, result, LoopControl

    loc0 = 0
    repeat
        loc1 = loc0 < 5
        condition = loc1
        if condition then
            result = WriteLine("Hello, World!")
            loc0 = loc0 + 1
            LoopControl = true
        else
            LoopControl = false
        end
    until not LoopControl
    return result
    
    """;

    static AssemblyDefinition Assembly = AssemblyDefinition.ReadAssembly("CSynth.Test.dll");
    private MethodDefinition GetMethodDefinition(string methodName) {
        var module = Assembly.MainModule;
        var type = module.GetType("CSynth.Test.ExamplesTest");
        return type.Methods.First(m => m.Name == methodName);
    }

    private string TestMethod(string methodName) {
        var method = GetMethodDefinition(methodName);
        var instructions = method.Body.Instructions;
        var statements = Compiler.Compile(instructions);
        return LuauWriter.Write(statements);
    }

    [Fact]
    public void TestExample1() {
        var result = TestMethod("Example1");
        Assert.Equal(ExpectedExample1, result);
    }

}
