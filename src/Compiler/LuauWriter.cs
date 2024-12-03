using System.Text;
using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Compiler;

public class LuauWriter {
    public const string RuntimePath = "@lib/Runtime";

    private List<Statement> statements;
    private ModuleContext context;

    private Dictionary<string, string> imports = new();

    private StringBuilder builder = new();
    private int indent = 0;
    private bool useRuntime = false;
    private string IndentString => new string(' ', indent * 4);


    private LuauWriter(List<Statement> statements, ModuleContext context) {
        this.statements = statements;
        this.context = context;
    }

    public static string Write(List<Statement> statements, ModuleContext context) {
        var writer = new LuauWriter(statements, context);
        return writer.Write();
    }


    public string Write() {
        foreach (var statement in statements) {
            ProcessStatement(statement);
        }

        foreach (var (name, path) in imports) {
            builder.Insert(0, $"local {name} = require(\"{path}\"){Environment.NewLine}");
        }

        if (useRuntime)
            builder.Insert(0, $"local rt = require(\"{RuntimePath}\"){Environment.NewLine}");

        return builder.ToString();
    }

    private void ProcessStatement(Statement statement) {
        switch (statement) {
            case DoWhileStatement doWhile:
                ProcessDoWhile(doWhile);
                break;
            case AssignmentStatement assignment:
                builder.AppendLine($"{IndentString}{ProcessExpression(assignment.Variable)} = {ProcessExpression(assignment.Expression)}");
                break;
            case IfStatement ifStatement:
                ProcessIf(ifStatement);
                break;
            case ReturnStatement returnStatement: {
                var expr = returnStatement.Expression == null ? "" : ProcessExpression(returnStatement.Expression);
                builder.AppendLine($"{IndentString}return {expr}");
                break;
            }
            case DefineVariablesStatement defineVariables: {
                var declaration = string.Join(", ", defineVariables.Variables);
                builder.AppendLine($"{IndentString}local {declaration}");
                break;
            }
            case CallStatement callStatement:
                builder.AppendLine($"{IndentString}{ProcessExpression(callStatement.Expression)}");
                break;
            case ModuleDefinitionStatement moduleDefinition:
                DefineModule(moduleDefinition);
                break;
            case TypeDefinitionStatement typeDefinition:
                DefineType(typeDefinition);
                break;
            case MethodDefinitionStatement methodDefinition:
                DefineMethod(methodDefinition);
                break;
            case ThrowStatement:
                builder.AppendLine($"{IndentString}error(\"Not implemented\")");
                break;
            case ArrayAssignmentStatement arrayAssignment:
                builder.AppendLine($"{IndentString}{ProcessExpression(arrayAssignment.Variable)}[{ProcessExpression(arrayAssignment.Index)}] = {ProcessExpression(arrayAssignment.Expression)}");
                break;
            default:
                throw new NotImplementedException(statement.GetType().Name);
        }
    }

    private void ProcessDoWhile(DoWhileStatement doWhile) {
        builder.AppendLine($"{IndentString}repeat");
        indent++;
        foreach (var statement in doWhile.Body) {
            ProcessStatement(statement);
        }
        indent--;
        builder.AppendLine($"{IndentString}until {doWhile.Condition} == 0");
    }

    private void ProcessIf(IfStatement ifStatement) {
        var first = true;
        foreach (var (expression, body) in ifStatement.Conditions) {
            var keyword = first ? "if" : (expression == null ? "else" : "elseif");
            first = false;

            builder.Append($"{IndentString}{keyword}");
            if (expression != null) {
                builder.Append($" {ProcessExpression(expression)} then");
            }
            builder.AppendLine();
            
            indent++;
            foreach (var statement in body) {
                ProcessStatement(statement);
            }
            indent--;
        }
        builder.AppendLine($"{IndentString}end");
    }

    private void DefineModule(ModuleDefinitionStatement moduleDefinition) {
        builder.AppendLine($"local {moduleDefinition.Module.Name} = {{}}");
    }

    private void DefineType(TypeDefinitionStatement typeDefinition) {
        builder.AppendLine($"{GetTypeName(typeDefinition.Type)} = {{}}");
    }

    private void DefineMethod(MethodDefinitionStatement methodDefinition) {
        var method = methodDefinition.Method;
        builder.Append($"{IndentString}function {GetTypeName(method.DeclaringType)}.{GetMethodName(method)}");

        var methods = method.Parameters.Select(p => p.Name).ToList();
        if (method.HasThis) {
            methods.Insert(0, "this");
        }

        builder.Append($"({string.Join(", ", methods)})");
        builder.AppendLine();
        indent++;
        foreach (var statement in methodDefinition.Body)
            ProcessStatement(statement);
        indent--;
        builder.AppendLine($"{IndentString}end");
    }

    private string[] badChars = new[] { "<", ">", "|", "`", "!" };

    private string ReplaceBadChars(string name) {
        foreach (var c in badChars) {
            name = name.Replace(c, string.Empty);
        }
        return name;
    }

    private string GetTypeName(TypeReference type) {
        if (type.IsArray) {
            return "Array_" + GetTypeName(type.GetElementType());
        
        }

        var name = ReplaceBadChars(type.Name);
        while (type.DeclaringType != null) {
            type = type.DeclaringType;
            name = $"{ReplaceBadChars(type.Name)}.{name}";
        }
        
        return name;
    }

    private string GetMethodName(MethodReference method) {
        string baseName = null!;
        
        if (method.Name == ".ctor" || method.Name == ".cctor") {
            baseName = "new";
        }
        else {
            baseName = ReplaceBadChars(method.Name);
        }

        var methodDef = method.Resolve();   
        if (methodDef == null)
            return baseName;

        return baseName + '_' + methodDef.RVA;
    }

    private string EscapeString(string str) {
        return str.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private string ProcessExpression(Expression expression) {
        switch (expression) {
            case Syscall syscall:
                return ProcessExpression(syscall.Emit() ?? syscall.GenerateRuntimeCall());
            case VariableExpression variable:
                return variable.Name;
            case BinaryExpression binary:
                return ProcessBinary(binary);
            case UnaryExpression unary:
                return $"not ({ProcessExpression(unary.Operand)})";
            case NumberExpression number:
                return FormatNumber(number);
            case StringExpression str:
                return $"\"{EscapeString(str.Value)}\"";
            case ByteArrayExpression byteArray:
                return "buffer.fromstring(\"" + string.Join("", byteArray.Value.Select(b => $"\\x{b:X2}")) + "\")";
            case NullExpression _:
                return "nil";
            case RuntimeMethodExpression runtimeMethod:
                useRuntime = true;
                return $"rt.{runtimeMethod.Method}";
            case MethodExpression method:
                return ImportMethod(method.Method);
            case LambdaExpression lambda:
                return ProcessExpression(lambda.Function);
            case VirtualFunctionExpression virtualFunction:
                return $"{ProcessExpression(virtualFunction.Expression)}.{GetMethodName(virtualFunction.Method)}";
            case CallExpression call: {
                // TODO: Fix method handle name
                var signature = call.Function.GetMethodSignature();
                return $"{ProcessExpression(call.Function)}({string.Join(", ", call.Arguments.Select(ProcessExpression))}) -- {signature}";
            }
            case BoolExpression boolean:
                return boolean.Value ? "true" : "false";
            case SelfExpression self:
                return "self";
            case FieldExpression field:
                return $"{ProcessExpression(field.Value)}.{field.Name}";
            case IndexExpression index:
                return $"{ProcessExpression(index.Value)}[{ProcessExpression(index.Index)}]";
            case ParameterExpression parameter:
                return $"arg{parameter.Parameter.Sequence}";
            case TypeExpression type:
                return GetTypeName(type.Type);
            case CreateObjectExpression createObject:
                return "{}";
            case ArrayExpression array:
                return $"table.create({ProcessExpression(array.Size)})";
            case LengthExpression length:
                return $"#({ProcessExpression(length.Value)}) + 1";
            default:
                throw new NotImplementedException(expression.GetType().Name);
        }
    }

    private Dictionary<Operator, string> OperatorMap = new() {
        { Operator.Add, "+" },
        { Operator.Subtract, "-" },
        { Operator.Multiply, "*" },
        { Operator.Divide, "/" },
        { Operator.Modulo, "%" },
        { Operator.Equal, "==" },
        { Operator.NotEqual, "~=" },
        { Operator.GreaterThan, ">" },
        { Operator.LessThan, "<" },
        { Operator.GreaterThanOrEqual, ">=" },
        { Operator.LessThanOrEqual, "<=" },
        { Operator.And, "and" },
        { Operator.Or, "or" }
    };

    private Dictionary<Operator, string> BitwiseMap = new() {
        { Operator.BitwiseNot, "bnot" },
        { Operator.BitwiseAnd, "band" },
        { Operator.BitwiseOr, "bor" },
        { Operator.BitwiseXor, "bxor" },
        { Operator.RightShift, "rshift" },
        { Operator.LeftShift, "lshift" }
    };

    private string ProcessBinary(BinaryExpression binary) {
        if (BitwiseMap.ContainsKey(binary.Operator)) {
            var method = BitwiseMap[binary.Operator];
            return $"bit32.{method}({ProcessExpression(binary.Left)}, {ProcessExpression(binary.Right)})";
        }

        var op = OperatorMap[binary.Operator];
        return $"{ProcessExpression(binary.Left)} {op} {ProcessExpression(binary.Right)}";
    }

    private string FormatNumber(NumberExpression number) {
        switch (number.Value) {
            case int:
            case long:
                return $"0x{number.Value:X}";
            case float:
            case double:
                return number.Value.ToString()!;
            default:
                throw new NotImplementedException();
        }
    }

    private string GetImportPath(TypeReference type) {
        var path = string.IsNullOrEmpty(type.Namespace) ? type.Name : $"{type.Namespace}.{type.Name}";

        if (type.IsNested)
            path = $"{GetImportPath(type.DeclaringType)}/{path}";

        return ReplaceBadChars(path.Replace(".", "/"));
    }

    private string ImportMethod(MethodReference method) {
        var type = method.DeclaringType;

        TypeReference importType = type;
        while (importType.DeclaringType != null)
            importType = importType.DeclaringType;

        if (importType.Scope.Name != context.Module.Name && !imports.ContainsKey(importType.FullName)) {
            var path = GetImportPath(importType);
            imports[GetTypeName(importType)] = $"@{path}";
        }

        return $"{GetTypeName(type)}.{GetMethodName(method)}";
    }
}
