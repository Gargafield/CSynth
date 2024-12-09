using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Core;

public class Scope {
    public List<Statement> Statements { get; } = new();
    public Stack<Expression> Expressions { get; init; } = new();
}

public class Compiler
{
    private MethodDefinition method;
    private ControlTree tree;
    public Stack<Scope> Scopes { get; } = new();
    public List<Statement> Statements => Scopes.Peek().Statements;
    public Stack<Expression> Expressions => Scopes.Peek().Expressions;
    public Dictionary<string, TypeReference> Variables { get; } = new();

    private Compiler(ControlTree tree, MethodDefinition method) {
        this.tree = tree;
        this.method = method;
        Scopes.Push(new());
    }

    public static List<Statement> Compile(MethodDefinition method, TranslationContext translation) {
        var flow = FlowInfo.From(method.Body.Instructions);

        if (translation.Debug) {
            Console.WriteLine("CFG:");
            Console.WriteLine(flow.CFG.ToDot());
        }

        Restructure.RestructureCFG(flow.CFG);

        if (translation.Debug) {
            Console.WriteLine("CFG (restructured):");
            Console.WriteLine(flow.CFG.ToDot());
        }

        var tree = ControlTree.From(flow.CFG);

        var compiler = new Compiler(tree, method);
        compiler.Compile();

        return compiler.Statements;
    }

    private void Compile() {
        CompileStructure(tree.Structure);

        if (Variables.Count > 0)
            Statements.Insert(0, new DefineVariablesStatement(Variables.Keys.ToList()));
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
        foreach (var structure in loop.Children.Take(Math.Max(loop.Children.Count - 1, 1))) {
            CompileStructure(structure);
        }

        var body = Scopes.Pop();
        var block = loop.Children.Last() as Block;

        Expression? condition = null;
        if (block is BranchBlock branch) {
            condition = new VariableExpression(branch!.Variable, Variables[branch!.Variable]);
        }
        else
            condition = body.Expressions.Pop();

        Statements.Add(new DoWhileStatement(
            body.Statements,
            new BoolSyscall(condition)
        ));
    }

    private void CompileBranch(BranchStructure structure) {
        var conditions = new List<Tuple<Expression?, List<Statement>>>();
        var branch = structure.Branch as BranchBlock;

        // TODO: Support more than 2 branches
        if (structure.Conditions.Count > 2) {
            throw new NotImplementedException("Only binary branches are supported");
        }
        
        CompileBlock(structure.Branch);

        var expression = Expressions.Pop();

        var expressions = Expressions;
        List<Scope> scopes = new();

        foreach (var (child, condition) in structure.Conditions) {
            Scopes.Push(new() {
                Expressions = new Stack<Expression>(expressions)
            });
            CompileStructure(child);

            var scope = Scopes.Pop();
            scopes.Add(scope);

            if (expression != null) {
                if (expression.Type.Name == "Int32")
                    expression = new BinaryExpression(expression, new NumberExpression(condition, TypeResolver.Int32Type), Operator.Equal, TypeResolver.BoolType);
            }

            conditions.Add(new Tuple<Expression?, List<Statement>>(
                expression != null ? expression : null,
                scope.Statements
            ));

            expression = null;
        }

        FixExpressionOverflows(scopes);

        Statements.Add(new IfStatement(conditions));
    }

    private void FixExpressionOverflows(IEnumerable<Scope> scopes) {
        var expressionOverflow = scopes.MaxBy(s => s.Expressions.Count)?.Expressions;
        var overflowCount = (expressionOverflow?.Count ?? 0) - Expressions.Count; 
        if (expressionOverflow == null || overflowCount <= 0)
            return;

        for (int i = 0; i < overflowCount; i++) {
            var name = $"overflow_{i}";
            var element = expressionOverflow.ElementAt(expressionOverflow.Count - i - 1);
            Statements.Add(new AssignmentStatement(name, new NullExpression()));
            Expressions.Push(new VariableExpression(name, element.Type));
            Variables[name] = element.Type;
        }

        foreach (var scope in scopes) {
            var expressions = scope.Expressions.ToList();
            for (int i = 0; i < Math.Min(expressions.Count, overflowCount); i++) {
                var expression = expressions.ElementAt(i);
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
                    Variables[name] = value.Type;
                }
                break;
            case BranchBlock branch: {
                Expressions.Push(new VariableExpression(branch.Variable, Variables[branch.Variable]));
                break;
            }
            case ExitBlock:
                var expression = Expressions.Count > 0 ? Expressions.Pop() : new NullExpression();
                Statements.Add(new ReturnStatement(expression));
                break;
            case EntryBlock:
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
                Expressions.Push(new NumberExpression(value, TypeResolver.Int32Type));
                break;
            }
            case Code.Ldc_I8: {
                long value = instruction.GetValue<long>();
                Expressions.Push(new NumberExpression(value, TypeResolver.Int64Type));
                break;
            }
            case Code.Ldc_R4: {
                float value = instruction.GetValue<float>();
                Expressions.Push(new NumberExpression(value, TypeResolver.FloatType));
                break;
            }
            case Code.Ldc_R8: {
                double value = instruction.GetValue<double>();
                Expressions.Push(new NumberExpression(value, TypeResolver.DoubleType));
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
                Expressions.Push(new VariableExpression(name, variable.VariableType));
                Variables[name] = variable.VariableType;
                break;
            }
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
            case Code.Ldarg:
            case Code.Ldarg_S: {
                ParameterDefinition parameter = instruction.GetParameter(method);
                Expressions.Push(new VariableExpression(parameter.Name, parameter.ParameterType));
                break;
            }
            case Code.Starg:
            case Code.Starg_S : {
                // TODO: Do this right (support out parameters)
                ParameterDefinition parameter = instruction.GetParameter(method);
                string name = parameter.Name;
                var value = Expressions.Pop();
                Statements.Add(new AssignmentStatement(name, value));
                break;
            }
            case Code.Ldarga:
            case Code.Ldarga_S: { // TODO: This is wrong
                ParameterDefinition parameter = instruction.GetParameter(method);
                Expressions.Push(new VariableExpression(parameter.Name, parameter.ParameterType));
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
                var value = Expressions.Pop();
                Variables[name] = variable.VariableType;
                Statements.Add(new AssignmentStatement(name, value));
                break;
            }
            case Code.Ldloca:
            case Code.Ldloca_S: {
                // TODO: Is this correct? Can we do this?
                VariableDefinition variable = instruction.GetVariable(method.Body);
                string name = $"local_{variable.Index}";
                Expressions.Push(new VariableExpression(name, variable.VariableType));
                Variables[name] = variable.VariableType;
                break;
            }
            case Code.Ldsflda: {
                FieldReference field = instruction.GetValue<FieldReference>();
                Expressions.Push(new IndexExpression(new TypeExpression(field.DeclaringType), new NumberExpression(field.Resolve().RVA, TypeResolver.Int32Type)));
                break;
            }
            case Code.Ldsfld: {
                FieldReference field = instruction.GetValue<FieldReference>();
                Expressions.Push(new IndexExpression(new TypeExpression(field.DeclaringType), new NumberExpression(field.Resolve().RVA, TypeResolver.Int32Type)));
                break;
            }
            case Code.Stsfld: {
                FieldReference field = instruction.GetValue<FieldReference>();
                var value = Expressions.Pop();
                Statements.Add(new AssignmentStatement(new IndexExpression(new TypeExpression(field.DeclaringType), new NumberExpression(field.Resolve().RVA, TypeResolver.Int32Type)), value));
                break;
            }
            case Code.Ldflda: {
                FieldReference field = instruction.GetValue<FieldReference>();
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(new FieldExpression(field.Name, reference));
                break;
            }
            case Code.Ldfld: {
                FieldReference field = instruction.GetValue<FieldReference>();
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(new FieldExpression(field.Name, reference));
                break;
            }
            case Code.Stfld: {
                FieldReference field = instruction.GetValue<FieldReference>();
                var value = Expressions.Pop();
                var reference = GetReference(Expressions.Pop());
                Statements.Add(new AssignmentStatement(new FieldExpression(field.Name, reference), value));
                break;
            }
            case Code.Ldlen: {
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new LengthExpression(array));
                break;
            }
            case Code.Ldtoken: {
                // TODO: By no means correct, but whatever
                var token = instruction.GetValue<IMetadataTokenProvider>();
                
                switch (token) {
                    case FieldDefinition field:
                        Expressions.Push(new IndexExpression(new TypeExpression(field.DeclaringType), new NumberExpression(field.RVA, TypeResolver.Int32Type)));
                        break;
                    case TypeDefinition type:
                        Expressions.Push(new TypeExpression(type));
                        break;
                    case TypeReference typeReference:
                        Expressions.Push(new TypeExpression(typeReference));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                break;
            }
            case Code.Ldftn: {
                var method = instruction.GetValue<MethodReference>();
                Expressions.Push(new MethodExpression(method));
                break;
            }
            case Code.Ldvirtftn: {
                var method = instruction.GetValue<MethodReference>();
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(new VirtualFunctionExpression(reference, method));
                break;
            }
            case Code.Ldnull: {
                Expressions.Push(new NullExpression());
                break;
            }
            case Code.Localloc: {
                var type = new TypeReference("System", "Byte", method.Module, method.Module);
                Expressions.Push(new ArrayExpression(type, Expressions.Pop()));
                break;
            }
            case Code.Add:
            case Code.Add_Ovf:
            case Code.Add_Ovf_Un:
            case Code.Sub:
            case Code.Sub_Ovf:
            case Code.Sub_Ovf_Un:
            case Code.Mul:
            case Code.Mul_Ovf:
            case Code.Mul_Ovf_Un:
            case Code.Div:
            case Code.Div_Un:
            case Code.Rem:
            case Code.Rem_Un:
            case Code.And:
            case Code.Or:
            case Code.Xor:
            case Code.Shr:
            case Code.Shr_Un:
            case Code.Shl: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                var op = instruction.OpCode.Code switch {
                    Code.Add => Operator.Add,
                    Code.Add_Ovf => Operator.Add,
                    Code.Add_Ovf_Un => Operator.Add,
                    Code.Sub => Operator.Subtract,
                    Code.Sub_Ovf => Operator.Subtract,
                    Code.Sub_Ovf_Un => Operator.Subtract,
                    Code.Mul => Operator.Multiply,
                    Code.Mul_Ovf => Operator.Multiply,
                    Code.Mul_Ovf_Un => Operator.Multiply,
                    Code.Div => Operator.Divide,
                    Code.Div_Un => Operator.Divide,
                    Code.Rem => Operator.Modulo,
                    Code.Rem_Un => Operator.Modulo,
                    Code.And => Operator.BitwiseAnd,
                    Code.Or => Operator.BitwiseOr,
                    Code.Xor => Operator.BitwiseXor,
                    Code.Shr => Operator.RightShift,
                    Code.Shr_Un => Operator.RightShift,
                    Code.Shl => Operator.LeftShift,
                    _ => throw new NotImplementedException()
                };

                // TODO: Generate specaialized expressions for types
                // if (left.Type != right.Type) {
                //     throw new NotImplementedException("Type mismatch");
                // }

                Expression expression = op switch {
                    Operator.Add => new AddSyscall(left, right),
                    Operator.Subtract => new SubSyscall(left, right),
                    Operator.Multiply => new MulSyscall(left, right),
                    _ => new BinaryExpression(left, right, op, left.Type)
                };

                Expressions.Push(expression);
                break;
            }
            case Code.Not: {
                var value = Expressions.Pop();
                Expressions.Push(new UnaryExpression(value, value.Type));
                break;
            }
            case Code.Neg: {
                var value = Expressions.Pop();
                Expressions.Push(new NegSyscall(value));
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
            case Code.Pop: {
                Expressions.Pop();
                break;
            }
            case Code.Conv_U:
            case Code.Conv_U1:
            case Code.Conv_U2:
            case Code.Conv_U4:
            case Code.Conv_U8:
            case Code.Conv_I:
            case Code.Conv_I1:
            case Code.Conv_I2:
            case Code.Conv_I4:
            case Code.Conv_I8: 
            case Code.Conv_R_Un:
            case Code.Conv_R4:
            case Code.Conv_R8:
            case Code.Conv_Ovf_U:
            case Code.Conv_Ovf_U_Un:
            case Code.Conv_Ovf_U1:
            case Code.Conv_Ovf_U1_Un:
            case Code.Conv_Ovf_U2:
            case Code.Conv_Ovf_U2_Un:
            case Code.Conv_Ovf_U4:
            case Code.Conv_Ovf_U4_Un:
            case Code.Conv_Ovf_U8:
            case Code.Conv_Ovf_U8_Un:
            case Code.Conv_Ovf_I:
            case Code.Conv_Ovf_I_Un:
            case Code.Conv_Ovf_I1:
            case Code.Conv_Ovf_I1_Un:
            case Code.Conv_Ovf_I2:
            case Code.Conv_Ovf_I2_Un:
            case Code.Conv_Ovf_I4:
            case Code.Conv_Ovf_I4_Un:
            case Code.Conv_Ovf_I8:
            case Code.Conv_Ovf_I8_Un: {
                // TODO: Conversion
                break;
            }
            case Code.Ceq:
            case Code.Cgt:
            case Code.Cgt_Un:
            case Code.Clt:
            case Code.Clt_Un: {
                var right = Expressions.Pop();
                var left = Expressions.Pop();
                var op = instruction.OpCode.Code switch {
                    Code.Ceq => Operator.Equal,
                    Code.Cgt => Operator.GreaterThan,
                    Code.Cgt_Un => Operator.GreaterThan,
                    Code.Clt => Operator.LessThan,
                    Code.Clt_Un => Operator.LessThan,
                    _ => throw new NotImplementedException()
                };

                Expression expression = Operator.Equal switch {
                    Operator.Equal => new EqSyscall(left, right),
                    _ => new BinaryExpression(left, right, op, TypeResolver.BoolType)
                };

                Expressions.Push(expression);
                break;
            }
            case Code.Br:
            case Code.Br_S: {
                break; // Unconditional branch
            }
            case Code.Brfalse:
            case Code.Brfalse_S: {
                Expression condition = Expressions.Pop();
                Expressions.Push(new UnaryExpression(new BoolSyscall(condition), TypeResolver.BoolType));
                break;
            }
            case Code.Brtrue:
            case Code.Brtrue_S: {
                Expression condition = Expressions.Pop();
                Expressions.Push(new BoolSyscall(condition));
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
            case Code.Blt_S:
            case Code.Bge_Un:
            case Code.Bge_Un_S:
            case Code.Bne_Un:
            case Code.Bne_Un_S:
            case Code.Bgt_Un:
            case Code.Bgt_Un_S:
            case Code.Ble_Un:
            case Code.Ble_Un_S:
            case Code.Blt_Un:
            case Code.Blt_Un_S: {
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
                    Code.Bge_Un => Operator.GreaterThanOrEqual,
                    Code.Bge_Un_S => Operator.GreaterThanOrEqual,
                    Code.Bne_Un => Operator.NotEqual,
                    Code.Bne_Un_S => Operator.NotEqual,
                    Code.Bgt_Un => Operator.GreaterThan,
                    Code.Bgt_Un_S => Operator.GreaterThan,
                    Code.Ble_Un => Operator.LessThanOrEqual,
                    Code.Ble_Un_S => Operator.LessThanOrEqual,
                    Code.Blt_Un => Operator.LessThan,
                    Code.Blt_Un_S => Operator.LessThan,
                    _ => throw new NotImplementedException()
                };

                Expression expression = op switch {
                    Operator.Equal => new EqSyscall(left, right),
                    _ => new BinaryExpression(left, right, op, TypeResolver.BoolType)
                };

                Expressions.Push(expression);
                break;
            }
            case Code.Isinst: {
                // TODO: Isinst (needs typetags on values)
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(reference);
                break;
            }
            case Code.Ldelem_Any: {
                var type = instruction.GetValue<TypeReference>();
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new IndexExpression(array, index));
                break;
            }
            case Code.Stelem_Any: {
                var value = Expressions.Pop();
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Statements.Add(new ArrayAssignmentStatement(array, index, value));
                break;
            }
            case Code.Ldelem_R4:
            case Code.Ldelem_R8:
            case Code.Ldelem_U1:
            case Code.Ldelem_U2:
            case Code.Ldelem_U4:
            case Code.Ldelem_I:
            case Code.Ldelem_I1:
            case Code.Ldelem_I2:
            case Code.Ldelem_I4:
            case Code.Ldelem_I8:
            case Code.Ldelem_Ref: {
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new IndexExpression(array, index));
                break;
            }
            case Code.Stelem_R4:
            case Code.Stelem_R8:
            case Code.Stelem_I:
            case Code.Stelem_I1:
            case Code.Stelem_I2:
            case Code.Stelem_I4:
            case Code.Stelem_I8:
            case Code.Stelem_Ref: {
                var value = Expressions.Pop();
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Statements.Add(new ArrayAssignmentStatement(array, index, value));
                break;
            }
            case Code.Ldelema: {
                // TODO: Implement Ldelema
                var index = Expressions.Pop();
                var array = GetReference(Expressions.Pop());
                Expressions.Push(new IndexExpression(array, index));
                break;
            }
            case Code.Ldind_I:
            case Code.Ldind_U1:
            case Code.Ldind_U2:
            case Code.Ldind_U4:
            case Code.Ldind_I1:
            case Code.Ldind_I2:
            case Code.Ldind_I4:
            case Code.Ldind_I8:
            case Code.Ldind_R4:
            case Code.Ldind_R8:
            case Code.Ldind_Ref: {
                // TODO: Address stuff is completely wrong
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(reference);
                break;
            }
            case Code.Stind_I:
            case Code.Stind_I1:
            case Code.Stind_I2:
            case Code.Stind_I4:
            case Code.Stind_I8:
            case Code.Stind_R4:
            case Code.Stind_R8:
            case Code.Stind_Ref: {
                var value = Expressions.Pop();
                var reference = GetReference(Expressions.Pop());
                Statements.Add(new AssignmentStatement(reference, value));
                break;
            }
            case Code.Ret: {
                if (method.ReturnType.FullName != "System.Void" && Expressions.Count > 0)
                    Statements.Add(new ReturnStatement(Expressions.Pop()));
                else
                    Statements.Add(new ReturnStatement(new NullExpression()));
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
                
                CallExpression call = new(new MethodExpression(method), arguments);

                if (method.ReturnType.FullName != "System.Void") {
                    string name = $"result_{instruction.Offset}";
                    Statements.Add(new AssignmentStatement(name, call));
                    Expressions.Push(new VariableExpression(name, method.ReturnType));
                    Variables.Add(name, method.ReturnType);
                } else {
                    Statements.Add(new CallStatement(call));
                }
                break;
            }
            case Code.Calli: {
                var signature = instruction.GetValue<CallSite>();
                var arguments = new List<Expression>();
                for (int i = 0; i < signature.Parameters.Count; i++) {
                    arguments.Add(Expressions.Pop());
                }

                arguments.Reverse();

                var function = Expressions.Pop();
                var lambda = new LambdaExpression(signature, function);
                Expressions.Push(new CallExpression(lambda, arguments));
                break;
            }
            case Code.Newarr: {
                var type = instruction.GetValue<TypeReference>();
                var size = Expressions.Pop();
                Expressions.Push(new ArrayExpression(type, size));
                break;
            }
            case Code.Initobj: {
                var type = instruction.GetValue<TypeReference>();
                var reference = GetReference(Expressions.Pop());
                Statements.Add(new AssignmentStatement(reference, new NullExpression()));
                break;
            }
            case Code.Newobj: {
                var ctor = instruction.GetValue<MethodReference>();
                var arguments = new List<Expression>();
                for (int i = 0; i < ctor.Parameters.Count; i++) {
                    arguments.Add(Expressions.Pop());
                }

                arguments.Reverse();

                var function = new MethodExpression(ctor);
                Expressions.Push(new CallExpression(function, arguments));
                break;
            }
            case Code.Ldobj: {
                // TODO: Ldobj
                var type = instruction.GetValue<TypeReference>();
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(reference);
                break;
            }
            case Code.Stobj: {
                // TODO: Stobj
                var type = instruction.GetValue<TypeReference>();
                var value = Expressions.Pop();
                var reference = GetReference(Expressions.Pop());
                Statements.Add(new AssignmentStatement(reference, value));
                break;
            }
            case Code.Castclass: {
                // TODO: Castclass
                var type = instruction.GetValue<TypeReference>();
                var reference = GetReference(Expressions.Pop());
                Expressions.Push(reference);
                break;
            }
            case Code.Throw: {
                var exception = Expressions.Pop();
                Statements.Add(new ThrowStatement(exception));
                break;
            }
            case Code.Rethrow: {
                // TODO: Rethrow
                break;
            }
            case Code.Sizeof: {
                var type = instruction.GetValue<TypeReference>();
                Expressions.Push(new NumberExpression(type.GetSize(), TypeResolver.Int32Type));
                break;
            }
            case Code.Refanytype: {
                // TODO: Refanytype
                break;
            }
            case Code.Unbox_Any:
            case Code.Unbox:
            case Code.Box: {
                // TODO: Do nothing?
                break;
            }
            case Code.Volatile:
            case Code.Constrained:
            case Code.Readonly: {
                // TODO: Do nothing?
                break;
            }
            case Code.Endfinally:
            case Code.Leave:
            case Code.Leave_S: {
                // TODO: Implement exception handling
                break;
            }
            case Code.Switch: {
                // TODO: Implement switch
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
        Variables.Add(name, expression.Type);
        return new VariableExpression(name, expression.Type);
    }
}
