﻿using CSynth.Analysis;
using CSynth.AST;
using CSynth.Transformation;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Compiler;

public class Scope {
    public List<Statement> Statements { get; } = new();
    public Stack<Expression> Expressions { get; } = new();
}

public class Compiler
{
    private MethodDefinition method;
    private ControlTree tree;
    public Stack<Scope> Scopes { get; } = new();
    public List<Statement> Statements => Scopes.Peek().Statements;
    public Stack<Expression> Expressions => Scopes.Peek().Expressions;
    public HashSet<string> Locals { get; } = new();

    private Compiler(ControlTree tree, MethodDefinition method) {
        this.tree = tree;
        this.method = method;
        Scopes.Push(new());
    }

    public static List<Statement> Compile(MethodContext method) {
        if (method.TranslationContext.Debug) {
            foreach (var instruction in method.Method.Body.Instructions)
                Console.WriteLine(instruction);
        }

        var flow = FlowInfo.From(method.Method.Body.Instructions);

        if (method.TranslationContext.Debug) {
            Console.WriteLine("CFG:");
            Console.WriteLine(flow.CFG.ToDot());
        }

        Restructure.RestructureCFG(flow.CFG);

        if (method.TranslationContext.Debug) {
            Console.WriteLine("CFG (restructured):");
            Console.WriteLine(flow.CFG.ToDot());
        }

        var tree = ControlTree.From(flow.CFG);

        var compiler = new Compiler(tree, method.Method);
        compiler.Compile();

        if (method.TranslationContext.Debug) {
            Console.WriteLine("Statements:");
            foreach (var statement in compiler.Statements)
                Console.WriteLine(statement);
        }

        return compiler.Statements;
    }

    private void Compile() {
        CompileStructure(tree.Structure);

        if (Locals.Count > 0)
            Statements.Insert(0, new DefineVariablesStatement(Locals.ToList()));
    }

    private void CompileStructure(object structure) {
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
            case Block block:
                CompileBlock(block);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void CompileLoop(LoopStructure loop) {
        Scopes.Push(new());
        foreach (var structure in loop.Children.Take(loop.Children.Count - 1)) {
            CompileStructure(structure);
        }

        var body = Scopes.Pop();
        var block = loop.Children.Last() as Block;
        var branch = block! as BranchBlock;

        Statements.Add(new DoWhileStatement(
            body.Statements,
            new VariableExpression(branch!.Variable)
        ));
    }

    private void CompileBranch(BranchStructure structure) {
        var conditions = new List<Tuple<Expression?, List<Statement>>>();
        var branch = structure.Branch as BranchBlock;

        // TODO: Support more than 2 branches
        if (structure.Children.Count > 2) {
            throw new NotImplementedException("Only binary branches are supported");
        }
        
        CompileBlock(structure.Branch);

        var expression = Expressions.Pop();
        List<Scope> scopes = new();

        foreach (var child in structure.Children) {
            var condition = structure.Conditions.First(c => c.Item1 == child).Item2;
            Scopes.Push(new());
            CompileStructure(child);

            var scope = Scopes.Pop();
            scopes.Add(scope);

            conditions.Add(new Tuple<Expression?, List<Statement>>(
                expression,
                scope.Statements
            ));

            expression = null;
        }

        FixExpressionOverflows(scopes);

        Statements.Add(new IfStatement(conditions));
    }

    private void FixExpressionOverflows(IEnumerable<Scope> scopes) {
        var expressionOverflow = scopes.Max(s => s.Expressions.Count);
        if (expressionOverflow == 0)
            return;

        for (int i = 0; i < expressionOverflow; i++) {
            var name = $"overflow_{i}";
            Statements.Add(new AssignmentStatement(name, new NullExpression()));
            Expressions.Push(new VariableExpression(name));
        }

        foreach (var scope in scopes) {
            var expressions = scope.Expressions.ToList();
            for (int i = 0; i < expressions.Count; i++) {
                var expression = expressions[i];
                scope.Statements.Add(new AssignmentStatement($"overflow_{i}", expression));
            }
        }
    }

    private void CompileLinear(LinearStructure structure) {
        foreach (var child in structure.Children) {
            CompileStructure(child);
        }
    }

    private void CompileBlock(Block block) {
        switch (block) {
            case BasicBlock basic:
                foreach (var instruction in basic.Instructions) {
                    ProcessInstruction(instruction);
                }
                break;
            case AssignmentBlock assignment:
                foreach (var (name, value) in assignment.Instructions) {
                    Statements.Add(new AssignmentStatement(name, value));
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

    private void ProcessInstruction(Instruction instruction) {

        switch (instruction.OpCode.Code) {
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
            case Code.Ldc_I4_8:
            case Code.Ldc_I4_M1:
            case Code.Ldc_I4_S:
            case Code.Ldc_I4: {
                int value = instruction.GetInt();
                Expressions.Push(new NumberExpression(value));
                break;
            }
            case Code.Ldstr: {
                string value = (string)instruction.Operand;
                Expressions.Push(new StringExpression(value));
                break;
            }
            case Code.Ldloc_0:
            case Code.Ldloc_1:
            case Code.Ldloc_2:
            case Code.Ldloc_3:
            case Code.Ldloc:
            case Code.Ldloc_S: {
                VariableDefinition variable = instruction.GetVariable(method.Body);
                string name = $"local_{variable.Index}";
                Locals.Add(name);
                Expressions.Push(new VariableExpression(name));
                break;
            }
            case Code.Ldarg_0:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
            case Code.Ldarg:
            case Code.Ldarg_S: {
                ParameterDefinition parameter = instruction.GetParameter(method);
                Expressions.Push(new VariableExpression(parameter.Name));
                break;
            }
            case Code.Stloc_0:
            case Code.Stloc_1:
            case Code.Stloc_2:
            case Code.Stloc_3:
            case Code.Stloc:
            case Code.Stloc_S: {
                VariableDefinition variable = instruction.GetVariable(method.Body);
                string name = $"local_{variable.Index}";
                Locals.Add(name);
                var value = Expressions.Pop();
                Statements.Add(new AssignmentStatement(name, value));
                break;
            }
            case Code.Ldloca:
            case Code.Ldloca_S: {
                // TODO: Is this correct? Can we do this?
                VariableDefinition variable = instruction.GetVariable(method.Body);
                string name = $"local_{variable.Index}";
                Expressions.Push(new VariableExpression(name));
                break;
            }
            case Code.Ldlen: {
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new LengthExpression(array));
                break;
            }
            case Code.Ldtoken: {
                // TODO: By no means correct, but whatever
                var token = instruction.GetValue<IMetadataTokenProvider>().MetadataToken;
                
                Expressions.Push(new VariableExpression(token.ToString()));
                break;
            }
            case Code.Ldnull: {
                Expressions.Push(new NullExpression());
                break;
            }
            case Code.Add:
            case Code.Sub:
            case Code.Mul:
            case Code.Div:
            case Code.Rem:
            case Code.And: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                var op = instruction.OpCode.Code switch {
                    Code.Add => Operator.Add,
                    Code.Sub => Operator.Subtract,
                    Code.Mul => Operator.Multiply,
                    Code.Div => Operator.Divide,
                    Code.Rem => Operator.Modulo,
                    _ => throw new NotImplementedException()
                };
                Expressions.Push(new BinaryExpression(left, right, op));
                break;
            }
            case Code.Dup: {
                var value = Expressions.Pop();
                if (value is Reference reference) {
                    Expressions.Push(value);
                    Expressions.Push(reference);
                } else {
                    Reference newReference = GetReference(value);
                    Expressions.Push(newReference);
                    Expressions.Push(newReference);
                }
                break;
            }
            case Code.Conv_I4:
            case Code.Conv_I8: {
                break;
            }
            case Code.Conv_R4:
            case Code.Conv_R8: {
                break;
            }
            case Code.Ceq:
            case Code.Cgt:
            case Code.Clt: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                var op = instruction.OpCode.Code switch {
                    Code.Ceq => Operator.Equal,
                    Code.Cgt => Operator.GreaterThan,
                    Code.Clt => Operator.LessThan,
                    _ => throw new NotImplementedException()
                };
                Expressions.Push(new BinaryExpression(left, right, op));
                break;
            }
            case Code.Br:
            case Code.Br_S: {
                break; // Unconditional branch
            }
            case Code.Brfalse:
            case Code.Brfalse_S: {
                Expression condition = Expressions.Pop();
                Expressions.Push(new UnaryExpression(condition));
                break;
            }
            case Code.Brtrue:
            case Code.Brtrue_S: {
                Expression condition = Expressions.Pop();
                Expressions.Push(new BinaryExpression(condition, new NumberExpression(0), Operator.NotEqual));
                break;
            }
            case Code.Beq:
            case Code.Beq_S:
            case Code.Bge:
            case Code.Bge_S:
            case Code.Bgt:
            case Code.Bgt_S:
            case Code.Ble:
            case Code.Ble_S:
            case Code.Blt:
            case Code.Blt_S: {
                Expression right = Expressions.Pop();
                Expression left = Expressions.Pop();

                Operator op = instruction.OpCode.Code switch {
                    Code.Beq => Operator.Equal,
                    Code.Beq_S => Operator.Equal,
                    Code.Bge => Operator.GreaterThanOrEqual,
                    Code.Bge_S => Operator.GreaterThanOrEqual,
                    Code.Bgt => Operator.GreaterThan,
                    Code.Bgt_S => Operator.GreaterThan,
                    Code.Ble => Operator.LessThanOrEqual,
                    Code.Ble_S => Operator.LessThanOrEqual,
                    Code.Blt => Operator.LessThan,
                    Code.Blt_S => Operator.LessThan,
                    _ => throw new NotImplementedException()
                };
                Expressions.Push(new BinaryExpression(left, right, op));
                break;
            }
            case Code.Ldelem_I4:
            case Code.Ldelem_I8: {
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new IndexExpression(array, index));
                break;
            }
            case Code.Stelem_I4:
            case Code.Stelem_I8: {
                var value = Expressions.Pop();
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Statements.Add(new ArrayAssignmentStatement(array, index, value));
                break;
            }
            case Code.Ret: {
                if (method.ReturnType.FullName != "System.Void")
                    Statements.Add(new ReturnStatement(Expressions.Pop()));
                break;
            }
            case Code.Callvirt:
            case Code.Call: {
                var method = instruction.GetValue<MethodReference>();
                var arguments = new List<Expression>();
                for (int i = 0; i < method.Parameters.Count; i++) {
                    arguments.Add(Expressions.Pop());
                }
                
                if (method.HasThis) {
                    var reference = GetReference(Expressions.Pop());
                    arguments.Add(reference);
                }

                arguments.Reverse();
                
                CallExpression call = new(method, arguments);

                if (method.ReturnType.FullName != "System.Void") {
                    string name = $"result_{instruction.Offset}";
                    Statements.Add(new AssignmentStatement(name, call));
                    Expressions.Push(new VariableExpression(name));
                } else {
                    Statements.Add(new CallStatement(call));
                }
                break;
            }
            case Code.Newarr: {
                var type = instruction.GetValue<TypeReference>();
                var size = Expressions.Pop();
                Expressions.Push(new ArrayExpression(type, size));
                break;
            }
            case Code.Newobj: {
                var ctor = instruction.GetValue<MethodReference>();
                var arguments = new List<Expression>();
                for (int i = 0; i < method.Parameters.Count; i++) {
                    arguments.Add(Expressions.Pop());
                }

                arguments.Reverse();

                Expressions.Push(new CallExpression(ctor, arguments));
                break;
            }
            case Code.Unbox_Any:
            case Code.Box: {
                // TODO: Do nothing?
                break;
            }
            case Code.Constrained: {
                // TODO: Do nothing?
                break;
            }
            case Code.Endfinally:
            case Code.Leave_S: {
                // TODO: Implement exception handling
                break;
            }
            default:
                throw new NotImplementedException(instruction.OpCode.ToString());
        }
    }

    private Reference GetReference(Expression expression) {
        if (expression is Reference reference) {
            return reference;
        }

        var name = $"temp_{Statements.Count}";
        Statements.Add(new AssignmentStatement(name, expression));
        return new VariableExpression(name);
    }
}
