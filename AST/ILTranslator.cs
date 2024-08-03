using System.Collections.ObjectModel;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.AST;

public class ILTranslator {
    private List<Statement> _statements = new();
    private Stack<Expression> _expressions = new();

    
    public static List<Statement> Translate(IEnumerable<Instruction> instructions) {
        var translator = new ILTranslator();

        foreach (var instruction in instructions) {
            translator.TranslateInstruction(instruction);
        }

        return translator._statements;
    }

    private Dictionary<string, Operator> OperatorMap = new() {
        { "add", Operator.Add },
        { "sub", Operator.Subtract },
        { "mul", Operator.Multiply },
        { "div", Operator.Divide },
        { "rem", Operator.Modulo },
        { "ceq", Operator.Equal },
        { "cgt", Operator.GreaterThan },
        { "clt", Operator.LessThan },
        { "cgt.un", Operator.GreaterThanOrEqual },
        { "clt.un", Operator.LessThanOrEqual },
        { "and", Operator.And },
        { "or", Operator.Or }
    };

    private void TranslateInstruction(Instruction instruction) {
        switch (instruction.OpCode.Code) {
            case Code.Nop:
                break;
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
                _expressions.Push(new VariableExpression($"arg{(int)instruction.OpCode.Code - (int)Code.Ldarg_0}"));
                break;
            case Code.Ldarg_S:
                _expressions.Push(new VariableExpression($"arg{(byte)instruction.Operand}"));
                break;
            case Code.Ldloc_0:
            case Code.Ldloc_1:
            case Code.Ldloc_2:
            case Code.Ldloc_3:
                _expressions.Push(new VariableExpression($"loc{(int)instruction.OpCode.Code - (int)Code.Ldloc_0}"));
                break;
            case Code.Ldloc_S:
                _expressions.Push(new VariableExpression($"loc{(byte)instruction.Operand}"));
                break;
            case Code.Stloc_0:
            case Code.Stloc_1:
            case Code.Stloc_2:
            case Code.Stloc_3:
                _statements.Add(new AssignmentStatement(
                    instruction.Offset,
                    $"loc{(int)instruction.OpCode.Code - (int)Code.Stloc_0}",
                    _expressions.Pop()
                ));
                break;
            case Code.Stloc_S:
                _statements.Add(new AssignmentStatement(
                    instruction.Offset,
                    $"loc{(byte)instruction.Operand}",
                    _expressions.Pop()
                ));
                break;
            case Code.Ldc_I4:
                _expressions.Push(new NumberExpression((int)instruction.Operand));
                break;
            case Code.Ldc_I4_S:
                _expressions.Push(new NumberExpression((sbyte)instruction.Operand));
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
                _expressions.Push(new NumberExpression((int)instruction.OpCode.Code - (int)Code.Ldc_I4_0));
                break;
            case Code.Add:
            case Code.Sub:
            case Code.Mul:
            case Code.Div:
            case Code.Rem: {
                var right = _expressions.Pop();
                var left = _expressions.Pop();
                _expressions.Push(new BinaryExpression(left, right, OperatorMap[instruction.OpCode.Name]));
                break;
            }
            case Code.Ldstr:
                _expressions.Push(new StringExpression((string)instruction.Operand));
                break;
            case Code.Call: {
                var method = (MethodReference)instruction.Operand;
                var args = new List<Expression>();
                for (var i = 0; i < method.Parameters.Count; i++) {
                    args.Add(_expressions.Pop());
                }
                args.Reverse();
                _statements.Add(new AssignmentStatement(
                    instruction.Offset,
                    "result", new CallExpression(method, args)));
                _expressions.Push(new VariableExpression("result"));
                break;
            }
            case Code.Br_S: {
                var target = (Instruction)instruction.Operand;
                _statements.Add(new GotoStatement(instruction.Offset, target.Offset));
                break;
            }
            case Code.Blt_S: {
                var target = (Instruction)instruction.Operand;
                var right = _expressions.Pop();
                var left = _expressions.Pop();
                _statements.Add(new AssignmentStatement(instruction.Offset - 1, "condition", new BinaryExpression(left, right, Operator.LessThan)));
                _statements.Add(new BranchStatement(instruction.Offset, "condition", target.Offset));
                break;
            }
            case Code.Ret:
                if (_expressions.Count > 0) {
                    _statements.Add(new ReturnStatement(instruction.Offset, _expressions.Pop()));
                } else {
                    _statements.Add(new ReturnStatement(instruction.Offset, null));
                }
                break;
            case Code.Clt:
            case Code.Cgt:
            case Code.Ceq: {
                var right = _expressions.Pop();
                var left = _expressions.Pop();
                _expressions.Push(new BinaryExpression(left, right, OperatorMap[instruction.OpCode.Name]));
                break;
            }
            case Code.Brtrue_S: {
                var target = (Instruction)instruction.Operand;
                var condition = _expressions.Pop();
                _statements.Add(new AssignmentStatement(instruction.Offset - 1, "condition", condition));
                _statements.Add(new BranchStatement(instruction.Offset, "condition", target.Offset));
                break;
            }
            default:
                throw new NotImplementedException(instruction.OpCode.Name);
        }
    }
}
