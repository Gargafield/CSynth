using CSynth.Analysis;
using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Test;

public class ExamplesTest
{
    private const string ExpectedExample1 = """
    local loc0, condition, result, LoopControl

    loc0 = 0
    repeat
        condition = loc0 < 5
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

    private const string ExpectedExample2 = """
    local loc0, condition, result, LoopControl

    loc0 = 0
    repeat
        condition = loc0 < 5
        if condition then
            result = WriteLine("Hello, World!")
            condition = loc0 == 2
            if condition then
                LoopControl = false
            else
                loc0 = loc0 + 1
                LoopControl = true
            end
        else
            LoopControl = false
        end
    until not LoopControl
    return result

    """;

    static AssemblyDefinition Assembly = AssemblyDefinition.ReadAssembly("IL.dll");
    private MethodDefinition GetMethodDefinition(string methodName) {
        var module = Assembly.MainModule;
        var type = module.GetType("Examples.Examples");
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

    [Fact]
    public void TestExample2() {
        var result = TestMethod("Example2");
        Assert.Equal(ExpectedExample2, result);
    }

}
