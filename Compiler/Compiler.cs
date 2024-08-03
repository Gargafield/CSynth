﻿using System.Text;
using CSynth.Analysis;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Compiler;

public class Compiler
{
    private ControlTree tree;
    public Stack<List<Statement>> Scopes { get; } = new();
    public List<Statement> Statements => Scopes.Peek();

    public Compiler(ControlTree tree) {
        this.tree = tree;
        Scopes.Push(new());
    }

    public void Compile() {
        CompileStructure(tree.Structure);
    }

    private void CompileStructure(Structure structure) {
        switch (structure) {
            case LoopStructure loop:
                CompileLoop(loop);
                break;
            case BranchStructure branch:
                CompileBranch(branch);
                break;
            case LinearStructure linear:
                CompileLinear(linear);
                break;
            case BlockStructure block:
                CompileBlock(block.Block);
                break;
        }
    }

    private void CompileLoop(LoopStructure loop) {
        Scopes.Push(new());
        foreach (var structure in loop.Children.Take(loop.Children.Count - 1)) {
            CompileStructure(structure);
        }

        var body = Scopes.Pop();
        var block = loop.Children.Last() as BlockStructure;
        var branch = block!.Block as BranchBlock;

        Statements.Add(new DoWhileStatement(
            -1,
            body,
            new VariableExpression(branch!.Variable)
        ));
    }

    private void CompileBranch(BranchStructure structure) {

        var conditions = new List<Tuple<Expression, List<Statement>>>();
        var branch = structure.Block as BranchBlock;

        var counter = 0;
        foreach (var child in structure.Children) {
            Scopes.Push(new());
            CompileStructure(child);
            conditions.Add(new Tuple<Expression, List<Statement>>(
                new BinaryExpression(
                    new VariableExpression(branch!.Variable),
                    new NumberExpression(counter),
                    "=="
                ),
                Scopes.Pop()
            ));

            counter++;
        }

        Statements.Add(new IfStatement(-1, conditions));
    }

    private void CompileLinear(LinearStructure structure) {
        foreach (var child in structure.Children) {
            CompileStructure(child);
        }
    }

    private void CompileBlock(Block block) {
        switch (block) {
            case BasicBlock basic:
                Statements.AddRange(basic.Statements);
                break;
            case BranchBlock branch: {
                // Ignore?
                break;
            }
            case EntryBlock entry:
            case ExitBlock exit:
                break;
            default:
                throw new NotImplementedException();
        }
    }

}
