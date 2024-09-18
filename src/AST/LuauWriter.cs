using System.Text;
using Mono.Cecil;

namespace CSynth.AST;

public class LuauWriter {
    private List<Statement> statements;
    private ModuleContext context;

    private Dictionary<string, string> imports = new();

    private StringBuilder builder = new();
    private int indent = 0;
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
        builder.AppendLine($"local {GetTypeName(typeDefinition.Type)} = {{}}");
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

    private string[] badChars = new[] { "<", ">", "|", "`" };

    private string GetTypeName(TypeReference type) {
        var name = type.Name;
        foreach (var c in badChars) {
            name = name.Replace(c, string.Empty);
        }
        return name;
    }

    private string GetMethodName(MethodReference method) {
        if (method.Name == ".ctor" || method.Name == ".cctor") {
            return "new";
        }
        var name = method.Name;
        foreach (var c in badChars) {
            name = name.Replace(c, string.Empty);
        }
        return name;
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

    private string ProcessExpression(Expression expression) {
        switch (expression) {
            case VariableExpression variable:
                return variable.Name;
            case BinaryExpression binary:
                return $"{ProcessExpression(binary.Left)} {OperatorMap[binary.Operator]} {ProcessExpression(binary.Right)}";
            case UnaryExpression unary:
                return $"not {ProcessExpression(unary.Operand)}";
            case NumberExpression number:
                return number.Value.ToString();
            case StringExpression str:
                return $"\"{str.Value}\"";
            case CallExpression call: {
                // TODO: Fix method handle name
                return $"{ImportMethod(call.Method)}({string.Join(", ", call.Arguments.Select(ProcessExpression))})";
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
            default:
                throw new NotImplementedException(expression.GetType().Name);
        }
    }

    private string ImportMethod(MethodReference method) {
        var type = method.DeclaringType;
        if (type.Scope.Name != context.Module.Name && !imports.ContainsKey(type.FullName)) {
            imports[GetTypeName(type)] = $"@{type.FullName.Replace(".", "/")}";
        }

        return $"{GetTypeName(type)}.{GetMethodName(method)}";
    }
}
