using System.Text;

namespace CSynth.AST;

public class LuauWriter {
    private List<Statement> statements;

    private HashSet<string> variables = new();
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
        
        var declaration = string.Join(", ", variables);
        builder.Insert(0, $"{IndentString}local {declaration}{Environment.NewLine}{Environment.NewLine}");
        return builder.ToString();
    }

    private void ProcessStatement(Statement statement) {
        switch (statement) {
            case DoWhileStatement doWhile:
                ProcessDoWhile(doWhile);
                break;
            case AssignmentStatement assignment:
                ProcessAssignment(assignment);
                break;
            case IfStatement ifStatement:
                ProcessIf(ifStatement);
                break;
            case ReturnStatement returnStatement: {
                var expr = returnStatement.Expression == null ? "" : ProcessExpression(returnStatement.Expression);
                builder.AppendLine($"{IndentString}return {expr}");
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
        builder.AppendLine($"{IndentString}until not {doWhile.Condition}");
    }

    private void ProcessAssignment(AssignmentStatement assignment) {
        variables.Add(assignment.Variable);
        builder.AppendLine($"{IndentString}{assignment.Variable} = {ProcessExpression(assignment.Expression)}");
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
                variables.Add(variable.Name);
                return variable.Name;
            case BinaryExpression binary:
                return $"{ProcessExpression(binary.Left)} {OperatorMap[binary.Operator]} {ProcessExpression(binary.Right)}";
            case UnaryExpression unary:
                return $"not {ProcessExpression(unary.Operand)}";
            case NumberExpression number:
                return number.Value.ToString();
            case StringExpression str:
                return $"\"{str.Value}\"";
            case CallExpression call:
                // TODO: Fix method handle name
                return $"{call.Method.Name}({string.Join(", ", call.Arguments.Select(ProcessExpression))})";
            case BoolExpression boolean:
                return boolean.Value ? "true" : "false";
            default:
                throw new NotImplementedException();
        }
    }
}
