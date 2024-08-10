
namespace CSynth.AST;

public abstract class ExpressionVisitor {
    public virtual bool VisitBinaryExpression(BinaryExpression expression) => true;
    public virtual bool VisitUnaryExpression(UnaryExpression expression) => true;
    public virtual bool VisitBoolExpression(BoolExpression expression) => true;
    public virtual bool VisitNumberExpression(NumberExpression expression) => true;
    public virtual bool VisitStringExpression(StringExpression expression) => true;
    public virtual bool VisitCallExpression(CallExpression expression) => true;
    public virtual bool VisitVariableExpression(VariableExpression expression) => true;
    public virtual bool VisitFieldExpression(FieldExpression expression) => true;
    public virtual bool VisitSelfExpression(SelfExpression expression) => true;
    public virtual bool VisitParameterExpression(ParameterExpression expression) => true;
}