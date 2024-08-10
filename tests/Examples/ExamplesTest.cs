﻿using CSynth.Analysis;
using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Test;

public class ExamplesTest
{
    private const string ExpectedExample1 = """
    local Console = require("@System/Console")
    local loc0, condition, LoopControl
    loc0 = 0
    repeat
        condition = loc0 < 5
        if not condition then
            LoopControl = 0
        else
            Console.WriteLine("Hello, World!")
            loc0 = loc0 + 1
            LoopControl = 1
        end
    until LoopControl == 0
    return 
    
    """;

    private const string ExpectedExample2 = """
    local Console = require("@System/Console")
    local loc0, condition, LoopControl
    loc0 = 0
    repeat
        condition = loc0 < 5
        if not condition then
            LoopControl = 0
        else
            Console.WriteLine("Hello, World!")
            condition = loc0 == 2
            if not condition then
                loc0 = loc0 + 1
                LoopControl = 1
            else
                LoopControl = 0
            end
        end
    until LoopControl == 0
    return 

    """;

    static AssemblyDefinition Assembly = AssemblyDefinition.ReadAssembly("IL.dll");
    private MethodDefinition GetMethodDefinition(string methodName) {
        var module = Assembly.MainModule;
        var type = module.GetType("Examples.Examples");
        return type.Methods.First(m => m.Name == methodName);
    }

    private string TestMethod(string methodName) {
        var method = GetMethodDefinition(methodName);
        var context = new TranslationContext() {
            Debug = false
        };

        var statements = Compiler.Compile(new MethodContext(method, context));
        return LuauWriter.Write(statements, new ModuleContext(Assembly.MainModule, context));
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
