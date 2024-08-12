namespace Cillogical.Kernel.Expression.Logical;

public class And : LogicalExpression
{
    public And(IEvaluable[] operands, string symbol = "AND") : base("AND", symbol, operands)
    {
        if (operands.Length < 2) {
            throw new ArgumentException("Non unary logical expression must have at least 2 operands");
        }
    }

    public override object Evaluate(Dictionary<string, object?>? context)
    {
        context = ContextUtils.FlattenContext(context);

        foreach (var operand in operands) {
            var res = operand.Evaluate(context);
            if (res is not bool) {
                throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" ({operand}) in AND expression, must be boolean value");
            } else if (!(bool)res) {
                return false;
            }
        }

        return true;
    }


    public override object Simplify(Dictionary<string, object?>? context)
    {
        context = ContextUtils.FlattenContext(context);
        var simplified = new IEvaluable[] { };

        foreach (var operand in operands)
        {
            var res = operand.Simplify(context);
            if (res is bool) {
                if (!(bool)res) {
                    return false;
                }
                continue;
            }

            Array.Resize(ref simplified, simplified.Length + 1);
            simplified[simplified.Length - 1] = res is IEvaluable ? (IEvaluable)res : operand;
        }

        if (simplified.Length == 0) {
            return true;
        }

        if (simplified.Length == 1) {
            return simplified[0];
        }

        return new And(simplified, symbol);
    }
}