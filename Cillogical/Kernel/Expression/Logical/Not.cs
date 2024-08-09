namespace Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel;

public class Not : LogicalExpression
{
    public Not(IEvaluable operand, string symbol = "NOT") : base("NOT", symbol, operand)
    {
        if (operands.Length != 1)
        {
            throw new ArgumentException("Unary logical expression must have 1 operand");
        }
    }

    public override object Evaluate(Dictionary<string, object>? context)
    {
        var res = operands[0].Evaluate(context);
        if (res is not bool) {
            throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" in NOT expression, must be boolean value");
        }

        return !(bool)res;
    }


    public override object Simplify(Dictionary<string, object>? context)
    {
        var res = operands[0].Simplify(context);
        if (res is bool) {
            return !(bool)res;
        } else if (res is not IEvaluable) {
            throw new InvalidExpressionException($"invalid simplified operand \"{res}\" ({operands[0]}) in NOT expression, must be boolean value");
        }

        return this;
    }
}