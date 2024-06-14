using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public class BuilderResult {
    public List<Statement> Statements { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
}

public class Builder
{

    public List<Statement> Statements { get; set; } = new();
    public Stack<Expression> Expressions { get; set; } = new();
    public Stack<Branch> Branches { get; set; } = new();

    public void GreedyGenerate(List<Instruction> instructions) {
        foreach (var instruction in instructions) {
            HandleInsruction(instruction);
        }
    }

    public void HandleInsruction(Instruction instruction) {
        switch (instruction.OpCode.Code) {
            case Code.Add: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                Expressions.Push(new BinaryExpression() {
                    Left = left,
                    Right = right,
                    Operator = "+"
                });
                break;
            }
            case Code.Ldc_I4: {
                Expressions.Push(new IntLiteral() {
                    Value = (int)instruction.Operand
                });
                break;
            }
            case Code.Ldc_I4_0:
            case Code.Ldc_I4_1:
            case Code.Ldc_I4_2:
            case Code.Ldc_I4_3:
            case Code.Ldc_I4_4:
            case Code.Ldc_I4_5:
            case Code.Ldc_I4_6:
            case Code.Ldc_I4_7:
            case Code.Ldc_I4_8: {
                Expressions.Push(new IntLiteral() {
                    Value = instruction.OpCode.Code - Code.Ldc_I4_0
                });
                break;
            }
            case Code.Ldc_I4_S: {
                Expressions.Push(new IntLiteral() {
                    Value = (sbyte)instruction.Operand
                });
                break;
            }
            case Code.Ldstr: {
                Expressions.Push(new StringLiteral() {
                    Value = (string)instruction.Operand
                });
                break;
            }
            case Code.Stloc: {
                var value = Expressions.Pop();
                Statements.Add(new AssignmentStatement() {
                    Variable = new LocalVariable() {  Offset = (ushort)instruction.Operand },
                    Value = value
                });
                break;
            }
            case Code.Stloc_0:
            case Code.Stloc_1:
            case Code.Stloc_2:
            case Code.Stloc_3: {
                var value = Expressions.Pop();
                Statements.Add(new AssignmentStatement() {
                    Variable = new LocalVariable() { Offset = instruction.OpCode.Code - Code.Stloc_0 },
                    Value = value
                });
                break;
            }
            case Code.Ldloc: {
                Expressions.Push(new LocalVariable() { Offset = (ushort)instruction.Operand });
                break;
            }
            case Code.Ldloc_0:
            case Code.Ldloc_1:
            case Code.Ldloc_2:
            case Code.Ldloc_3: {
                Expressions.Push(new LocalVariable() { Offset = instruction.OpCode.Code - Code.Ldloc_0 });
                break;
            }
            case Code.Call: {
                var method = (MethodReference)instruction.Operand;
                var arguments = new List<Expression>();

                for (var i = 0; i < method.Parameters.Count; i++) {
                    arguments.Add(Expressions.Pop());
                }

                if (method.ReturnType.FullName != "System.Void") {
                    Expressions.Push(new CallExpression() {
                        Method = method.Name,
                        Arguments = arguments
                    });
                } else {
                    Statements.Add(new CallStatement() {
                        Method = method.Name
                    });
                }
                break;
            }
            case Code.Ret: {
                
                Statements.Add(new ReturnStatement() {
                    Value = Expressions.Count > 0 ? Expressions.Pop() : new NullLiteral()
                });
                break;
            }
            case Code.Brfalse:
            case Code.Brfalse_S: {
                var condition = Expressions.Pop();
                Branches.Push(new FalseBranch() {
                    Condition = new FalsyCondition() { Value = condition},
                });
                break;
            }
            case Code.Bne_Un:
            case Code.Bne_Un_S: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                Branches.Push(new TrueBranch() {
                    Condition = new BinaryCondition() {
                        Left = left,
                        Right = right,
                        Operator = "!="
                    }
                });
                break;
            }
            case Code.Ceq: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                Expressions.Push(new BinaryCondition() {
                    Left = left,
                    Right = right,
                    Operator = "=="
                });
                break;
            }
            case Code.Nop:
                break;
            default:
                throw new NotImplementedException($"Unhandled instruction: {instruction.OpCode.Code}");
        }
    }
}
