namespace Cillogical.Kernel.Expression.Logical;

public class Not : LogicalExpression
{
    public Not(IEvaluable operand, string symbol = "NOT") : base("NOT", symbol, operand) { }

    public override object Evaluate(Dictionary<string, object?>? context)
    {
        context = ContextUtils.FlattenContext(context);

        var res = operands[0].Evaluate(context);
        if (res is not bool) {
            throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" in NOT expression, must be boolean value");
        }

        return !(bool)res;
    }


    public override object Simplify(Dictionary<string, object?>? context)
    {
        context = ContextUtils.FlattenContext(context);
        var res = operands[0].Simplify(context);

        if (res is bool) {
            return !(bool)res;
        }

        return this;
    }
}