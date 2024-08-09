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
        foreach (var operand in operands) {
            var res = operand.Evaluate(context);
            if (res is bool && (bool)res) {
                return true;
            }
        }

        return false;
    }


    public override object Simplify(Dictionary<string, object>? context)
    {
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
                continue;
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