
using CSynth.AST;

namespace CSynth.Analysis;

public class VariableVisitor : ExpressionVisitor
{
    public List<VariableExpression> Variables { get; } = new();

    public VariableVisitor() { }

    public override bool VisitVariableExpression(VariableExpression expression) {
        Variables.Add(expression);
        return true;
    }
}