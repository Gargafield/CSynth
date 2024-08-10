using System.Diagnostics;
using CSynth.AST;
using CSynth.Transformation;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public class Compiler
{
    private ControlTree tree;
    public Stack<List<Statement>> Scopes { get; } = new();
    public List<Statement> Statements => Scopes.Peek();
    public HashSet<string> Locals { get; } = new();

    private VariableVisitor variableVisitor = new();

    public Dictionary<string, System.Type> Variables { get; } = new() {
        { "HeaderExit", typeof(int) },
        { "LoopExit", typeof(bool) }
    };

    private Compiler(ControlTree tree) {
        this.tree = tree;
        Scopes.Push(new());
    }

    public static List<Statement> Compile(MethodContext method) {
        if (method.TranslationContext.Debug) {
            foreach (var instruction in method.Method.Body.Instructions)
                Console.WriteLine(instruction);
        }

        var statements = ILTranslator.Translate(method);        
        var flow = FlowInfo.From(statements);
        Restructure.RestructureCFG(flow.CFG);
        var tree = ControlTree.From(flow.CFG);
        var compiler = new Compiler(tree);
        compiler.Compile();

        if (method.TranslationContext.Debug) {
            Console.WriteLine("Statements:");
            foreach (var statement in compiler.Statements)
                Console.WriteLine(statement);
        }

        return compiler.Statements;
    }

    public static List<Statement> Compile(ControlTree tree) {
        var compiler = new Compiler(tree);
        compiler.Compile();
        return compiler.Statements;
    }

    private void Compile() {
        CompileStructure(tree.Structure);

        if (Locals.Count > 0)
            Statements.Insert(0, new DefineVariablesStatement(Locals.ToList()));
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
            body,
            new VariableExpression(branch!.Variable)
        ));
    }

    private void CompileBranch(BranchStructure structure) {

        var conditions = new List<Tuple<Expression?, List<Statement>>>();
        var branch = structure.Block as BranchBlock;

        // TODO: Support more than 2 branches
        if (structure.Children.Count > 2) {
            throw new NotImplementedException("Only binary branches are supported");
        }
        
        var first = true;
        foreach (var child in structure.Children) {
            var condition = structure.Conditions.First(c => c.Item1 == child).Item2;
            Scopes.Push(new());
            CompileStructure(child);

            Expression? expression = null;
            if (first) {
                var type = Variables.TryGetValue(branch!.Variable, out var t) ? t : typeof(bool);
                expression = new VariableExpression(branch.Variable);
                if (condition == 0) {
                    expression = new UnaryExpression(expression);
                }

                if (type == typeof(int)) {
                    expression = new BinaryExpression(
                        expression,
                        new NumberExpression(condition),
                        Operator.Equal
                    );
                }
            }

            conditions.Add(new Tuple<Expression?, List<Statement>>(
                expression,
                Scopes.Pop()
            ));
            first = false;
        }

        Statements.Add(new IfStatement(conditions));
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
                foreach (var statement in basic.Statements) {
                    ProcessStatement(statement);
                }
                break;
            case BranchBlock: {
                // Ignore?
                break;
            }
            case EntryBlock:
            case ExitBlock:
            case NoopBlock:
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void ProcessStatement(Statement statement) {
        if (statement is AssignmentStatement assignment) {

            variableVisitor.Variables.Clear();
            assignment.Variable.Accept(variableVisitor);

            foreach (var variable in variableVisitor.Variables) {
                Locals.Add(variable.Name);
            }
        }
    }
}
