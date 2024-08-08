using System.Text;
using Mono.Cecil;

namespace CSynth.AST;

public class LuauWriter {
    private List<Statement> statements;

    private Dictionary<string, string> imports = new();

    private StringBuilder builder = new();
    private int indent = 0;
    private string IndentString => new string(' ', indent * 4);

    private LuauWriter(List<Statement> statements) {
        this.statements = statements;
    }

    public static string Write(List<Statement> statements) {
        var writer = new LuauWriter(statements);
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
                builder.AppendLine($"{IndentString}{assignment.Variable} = {ProcessExpression(assignment.Expression)}");
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
            default:
                throw new NotImplementedException();
        }
    }

    private string ImportMethod(MethodReference method) {
        var type = method.DeclaringType;
        if (!imports.ContainsKey(type.FullName)) {
            imports[type.Name] = $"@{type.FullName.Replace(".", "/")}";
        }

        return $"{type.Name}.{method.Name}";
    }
}
