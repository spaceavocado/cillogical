namespace Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel;

public class Nor : LogicalExpression
{
    private string notSymbol;

    public Nor(IEvaluable[] operands, string symbol = "NOR", string notSymbol = "NOT") : base("NOR", symbol, operands)
    {
        if (operands.Length < 2) {
            throw new ArgumentException("Non unary logical expression must have at least 2 operands");
        }

        this.notSymbol = notSymbol;
    }

    public override object Evaluate(Dictionary<string, object>? context)
    {
        foreach (var operand in operands) {
            var res = operand.Evaluate(context);
            if (res is not bool) {
                throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" ({operand}) in NOR expression, must be boolean value");
            } else if ((bool)res) {
                return false;
            }
        }

        return true;
    }

    public override object Simplify(Dictionary<string, object>? context)
    {
        var simplified = new IEvaluable[] { };
        foreach (var operand in operands)
        {
            var res = operand.Simplify(context);
            if (res is bool) {
                if ((bool)res) {
                    return false;
                }
                continue;
            } else if (res is not IEvaluable) {
                throw new InvalidExpressionException($"invalid simplified operand \"{res}\" ({operand}) in NOR expression, must be boolean value");
            }

            Array.Resize(ref simplified, simplified.Length + 1);
            simplified[simplified.Length - 1] = (IEvaluable)res;
        }

        if (simplified.Length == 0) {
            return true;
        }

        if (simplified.Length == 1) {
            return new Not(simplified[0]);
        }

        return new Nor(simplified, symbol, notSymbol);
    }
}