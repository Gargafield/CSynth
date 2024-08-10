using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.AST;

public class ILTranslator {
    public List<Statement> Statements = new();
    public List<(int, GotoStatement)> targets = new();
    public List<(Range, Statement)> offsets = new();
    private Stack<Expression> _expressions = new();
    private int _lastOffset = -1;
    private MethodContext _context;

    private ILTranslator(MethodContext context) {
        _context = context;
    }
    
    public static List<Statement> Translate(MethodContext context) {
        var translator = new ILTranslator(context);

        foreach (var instruction in context.Method.Body.Instructions) {
            translator.TranslateInstruction(instruction);
        }

        return translator.Statements;
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
        { "or", Operator.Or },
        { "beq.s", Operator.Equal },
        { "blt.s", Operator.LessThan },
        { "bgt.s", Operator.GreaterThan },
        { "ble.s", Operator.LessThanOrEqual },
        { "bge.s", Operator.GreaterThanOrEqual },
    };

    private void AddStatement(int offset, Statement statement) {
        Statements.Add(statement);
        offsets.Add((new Range(_lastOffset + 1, offset), statement));
        _lastOffset = offset;

        // Look in targets if there is a target less than the current instruction
        foreach (var (target, gotoStatement) in targets.Where(t => t.Item1 <= offset).ToList()) {
            gotoStatement.Target = statement;
            targets.Remove((target, gotoStatement));
        }
    }

    private bool TryGetTarget(int offset, out Statement statement) {
        foreach (var (range, target) in offsets) {
            if (offset >= range.Start.Value && offset <= range.End.Value) {
                statement = target;
                return true;
            }
        }

        statement = null!;
        return false;
    }

    private bool FixBranchTarget(GotoStatement statement, Instruction instruction) {
        var target = (Instruction)instruction.Operand;
        if (TryGetTarget(target.Offset, out var targetStatement)) {
            statement.Target = targetStatement;
            return true;
        }
        else {
            targets.Add((target.Offset, statement));
        }

        return false;
    }

    private void TranslateInstruction(Instruction instruction) {
        
        switch (instruction.OpCode.Code) {
            case Code.Nop:
                break;
            case Code.Ldarg_0:
                if (_context.Method.HasThis) {
                    _expressions.Push(new SelfExpression());
                } else {
                    _expressions.Push(new ParameterExpression(_context.Method.Parameters[0]));
                }
                break;
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
                _expressions.Push(new ParameterExpression(_context.Method.Parameters[(int)instruction.OpCode.Code - (int)Code.Ldarg_1]));
                break;
            case Code.Ldarg_S:
                _expressions.Push(new ParameterExpression(_context.Method.Parameters[(byte)instruction.Operand]));
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
                AddStatement(instruction.Offset, new AssignmentStatement(
                    $"loc{(int)instruction.OpCode.Code - (int)Code.Stloc_0}",
                    _expressions.Pop()
                ));
                break;
            case Code.Stloc_S:
                AddStatement(instruction.Offset, new AssignmentStatement(
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

                if (method.HasThis) {
                    args.Add(_expressions.Pop());
                }

                for (var i = 0; i < method.Parameters.Count; i++) {
                    args.Add(_expressions.Pop());
                }
                args.Reverse();

                if (method.ReturnType.FullName == "System.Void") {
                    AddStatement(instruction.Offset, new CallStatement(
                        new CallExpression(method, args)
                    ));
                    break;
                }

                AddStatement(instruction.Offset, new AssignmentStatement(
                    "result",
                    new CallExpression(method, args)
                ));
                _expressions.Push(new VariableExpression("result"));
                break;
            }
            case Code.Br_S: {
                var target = (Instruction)instruction.Operand;
                var statement = new GotoStatement(null!);
                FixBranchTarget(statement, instruction);
                AddStatement(instruction.Offset, statement);
                break;
            }
            case Code.Beq_S:
            case Code.Bge_S:
            case Code.Bgt_S:
            case Code.Ble_S:
            case Code.Blt_S: {
                var target = (Instruction)instruction.Operand;
                var right = _expressions.Pop();
                var left = _expressions.Pop();
                AddStatement(instruction.Offset - 1, new AssignmentStatement(
                    "condition",
                    new BinaryExpression(left, right, OperatorMap[instruction.OpCode.Name])
                ));

                var statement = new BranchStatement("condition", null!);
                FixBranchTarget(statement, instruction);
                AddStatement(instruction.Offset, statement);

                break;
            }
            case Code.Ret:
                if (_expressions.Count > 0) {
                    AddStatement(instruction.Offset, new ReturnStatement(_expressions.Pop()));
                } else {
                    AddStatement(instruction.Offset, new ReturnStatement(null));
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
            case Code.Brtrue_S:
            case Code.Brfalse_S:{
                var target = (Instruction)instruction.Operand;
                var condition = _expressions.Pop();
                if (instruction.OpCode.Code == Code.Brfalse_S) {
                    condition = new UnaryExpression(condition);
                }
                else {
                    condition = new BinaryExpression(condition, new NumberExpression(0), Operator.NotEqual);
                }

                AddStatement(instruction.Offset - 1, new AssignmentStatement(
                    "condition",
                    condition
                ));

                var statement = new BranchStatement("condition", null!);
                FixBranchTarget(statement, instruction);
                AddStatement(instruction.Offset, statement);
                break;
            }
            case Code.Stfld: {
                var field = (FieldReference)instruction.Operand;
                var value = _expressions.Pop();
                // TODO: Handle values taht are not references
                var obj = _expressions.Pop() as Reference;
                AddStatement(instruction.Offset, new AssignmentStatement(
                    new FieldExpression(field.Name, obj!),
                    value
                ));
                break;
            }
            
            default:
                throw new NotImplementedException(instruction.OpCode.Name);
        }
    }
}
