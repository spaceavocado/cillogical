namespace Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel;

public class Or : LogicalExpression
{
    public Or(IEvaluable[] operands, string symbol = "OR") : base("OR", symbol, operands)
    {
        if (operands.Length < 2) {
            throw new ArgumentException("Non unary logical expression must have at least 2 operands");
        }
    }

    public override object Evaluate(Dictionary<string, object>? context)
    {
        context = ContextUtils.FlattenContext(context);

        foreach (var operand in operands) {
            var res = operand.Evaluate(context);
            if (res is not bool) {
                throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" ({operand}) in OR expression, must be boolean value");
            } else if ((bool)res) {
                return true;
            }
        }

        return false;
    }


    public override object Simplify(Dictionary<string, object>? context)
    {
        context = ContextUtils.FlattenContext(context);
        var simplified = new IEvaluable[] { };

        foreach (var operand in operands)
        {
            var res = operand.Simplify(context);
            if (res is bool) {
                if ((bool)res) {
                    return true;
                }
                continue;
            } else if (res is not IEvaluable) {
                throw new InvalidExpressionException($"invalid simplified operand \"{res}\" ({operand}) in OR expression, must be boolean value");
            }

            Array.Resize(ref simplified, simplified.Length + 1);
            simplified[simplified.Length - 1] = (IEvaluable)res;
        }

        if (simplified.Length == 0) {
            return false;
        }

        if (simplified.Length == 1) {
            return simplified[0];
        }

        return new Or(simplified, symbol);
    }
}