using System.Linq.Expressions;
using System.Text;
using CSynth.Analysis;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Compiler;

public class Compiler
{
    private CFG cfg { get; set; }
    private HashSet<Block> visited { get; set; } = new();
    private Stack<Stack<Block>> stacks { get; set; } = new();
    private Stack<Block> stack => stacks.Peek();
    private Stack<List<Statement>> scopes { get; set; } = new();
    private List<Statement> output => scopes.Peek();
    private Stack<Expression> expressions { get; set; } = new();
    private int indent { get; set; }

    public Compiler(CFG cfg)
    {
        this.cfg = cfg;
        stacks.Push(new Stack<Block>());
        scopes.Push(new List<Statement>());
        this.indent = 0;
    }

    public List<Statement> Compile()
    {
        output.Clear();
        stack.Push(cfg.Blocks.First());

        Compile(cfg.Blocks.ToHashSet());

        return output;
    }

    private void Compile(HashSet<Block> blocks)
    {
        while (stack.Any())
        {
            var block = stack.Pop();

            if (visited.Contains(block))
                continue;

            visited.Add(block);

            CompileBlock(block);

            foreach (var successor in block.Successors)
                if (blocks.Contains(successor))
                    stack.Push(successor);
        }
    }

    private void CompileBlock(Block block)
    {
        var loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Header == block);
        if (loop != null)
            StartLoopRegion(loop, block);
        
        switch (block) {
            case NoopBlock:
            case EntryBlock:
            case ExitBlock:
                break;
            case BasicBlock basic:
                CompileBasicBlock(basic);
                break;
            case AssignmentBlock assignment:
                CompileAssignmentBlock(assignment);
                break;
            case BranchBlock branchBlock:
                CompileBranchBlock(branchBlock);
                break;
        }

        loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Control == block);
        if (loop != null)
            EndLoopRegion(loop, block);

        var branch = cfg.Regions.OfType<BranchRegion>().FirstOrDefault(r => r.Header == block);
        if (branch != null)
            StartBranchRegion(branch, block);
    }

    private void CompileBasicBlock(BasicBlock block)
    {
        foreach (var instruction in block.Instructions)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Nop:
                    break;
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8: {
                    var num = instruction.OpCode.Code - Code.Ldc_I4_0;
                    expressions.Push(new NumberExpression(num));
                    break;
                }
                case Code.Ldc_I4_S:
                    expressions.Push(new NumberExpression((sbyte)instruction.Operand));
                    break;
                case Code.Add: {
                    var right = expressions.Pop();
                    var left = expressions.Pop();
                    expressions.Push(new BinaryExpression(left, right, "+"));
                    break;
                }
                case Code.Clt: {
                    var right = expressions.Pop();
                    var left = expressions.Pop();
                    expressions.Push(new BinaryExpression(left, right, "<"));
                    break;
                }
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3: {
                    var num = instruction.OpCode.Code - Code.Stloc_0;
                    output.Add(new InstructionStatement($"v{num} = {expressions.Pop()}"));
                    break;
                }
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3: {
                    var num = instruction.OpCode.Code - Code.Ldloc_0;
                    expressions.Push(new VariableExpression($"v{num}"));
                    break;
                }
                case Code.Br_S:
                    break;
                case Code.Call:
                    var func = instruction.Operand as MethodReference;
                    var call = new StringBuilder();
                    call.Append($"{func!.FullName}(");
                    for (var i = 0; i < func.Parameters.Count; i++)
                    {
                        call.Append(expressions.Pop());
                        if (i < func.Parameters.Count - 1)
                            call.Append(", ");
                    }
                    call.Append(")");
                    output.Add(new InstructionStatement(call.ToString()));

                    break;
                case Code.Brtrue_S:
                    var expr = expressions.Pop();
                    output.Add(new ExpressionStatement(
                        expr
                    ));
                    break;
                case Code.Ret:
                    output.Add(new InstructionStatement("return"));
                    break;
            }
        }
    }

    private void CompileAssignmentBlock(AssignmentBlock block)
    {
        foreach (var assignment in block.Assignments)
        {
            var expr = new InstructionStatement($"{assignment.Key} = {assignment.Value}");
            output.Add(expr);
        }
    }

    private void CompileBranchBlock(BranchBlock block)
    {
        var expr = new ExpressionStatement(new VariableExpression($"{block.Variable}"));
        output.Add(expr);
    }

    private void EndLoopRegion(LoopRegion loop, Block block)
    {
        var stats = scopes.Pop();
        var exprStat = stats.Last() as ExpressionStatement;
        stats.RemoveAt(stats.Count - 1);
        var expr = new BinaryExpression(exprStat!.Expression, new NumberExpression(0), "==");
        output.Add(new LoopStatement(stats, expr));
    }

    private void StartLoopRegion(LoopRegion loop, Block block) {
        scopes.Push(new List<Statement>());
    }

    private void StartBranchRegion(BranchRegion branch, Block block) {
        var conditions = new List<Tuple<Expression, List<Statement>>>();
        var exprStat = output.Last() as ExpressionStatement;
        output.RemoveAt(output.Count - 1);

        if (block is BasicBlock basic) {
            // Two targets, two regions

            conditions.Add(new Tuple<Expression, List<Statement>>(exprStat!.Expression, DoBranchRegion(branch.Regions[0])));
            conditions.Add(new Tuple<Expression, List<Statement>>(BoolExpression.True, DoBranchRegion(branch.Regions[1])));
        }
        else if (block is BranchBlock branchBlock) {
            // Multiple targets, multiple regions
            foreach (var region in branch.Regions) {
                var index = (double)branch.Regions.IndexOf(region);
                var expr = new BinaryExpression(exprStat!.Expression, new NumberExpression(index), "==");
                conditions.Add(new Tuple<Expression, List<Statement>>(expr, DoBranchRegion(region)));
            }
        }

        output.Add(new IfStatement(conditions));
        stack.Push(branch.Exit);
    }

    private List<Statement> DoBranchRegion(BranchRegion.Region region) {
        var stats = new List<Statement>();
        scopes.Push(stats);
        var stack = new Stack<Block>();
        stack.Push(region.Header);
        stacks.Push(stack);
        Compile(region.Blocks);
        stacks.Pop();
        scopes.Pop();
        return stats;
    }

}
