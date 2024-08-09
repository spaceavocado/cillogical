namespace Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel;

public class Xor : LogicalExpression
{
    private string notSymbol;
    private string norSymbol;

    public Xor(IEvaluable[] operands, string symbol = "XOR", string notSymbol = "NOT", string norSymbol = "NOR") : base("XOR", symbol, operands)
    {
        if (operands.Length < 2) {
            throw new ArgumentException("Non unary logical expression must have at least 2 operands");
        }

        this.notSymbol = notSymbol;
        this.norSymbol = norSymbol;
    }

    public override object Evaluate(Dictionary<string, object>? context)
    {
        bool? xor = null;
        foreach (var operand in operands) {
            var res = operand.Evaluate(context);
            if (res is not bool) {
                throw new InvalidExpressionException($"invalid evaluated operand \"{res}\" ({operand}) in XOR expression, must be boolean value");
            }

            if (xor is null) {
                xor = (bool)res;
                continue;
            }

            if ((bool)xor && (bool)res) {
                return false;
            }

            xor = (bool)res ? true : xor;
        }

        return xor ?? false;
    }


    public override object Simplify(Dictionary<string, object>? context)
    {
        var truthy = 0;
        var simplified = new IEvaluable[] { };

        foreach (var operand in operands)
        {
            var res = operand.Simplify(context);
            if (res is bool) {
                if ((bool)res) {
                    truthy += 1;
                }
                if (truthy > 1) {
                    return false;
                }
                continue;
            } else if (res is not IEvaluable) {
                throw new InvalidExpressionException($"invalid simplified operand \"{res}\" ({operand}) in XOR expression, must be boolean value");
            }

            Array.Resize(ref simplified, simplified.Length + 1);
            simplified[simplified.Length - 1] = (IEvaluable)res;
        }

        if (simplified.Length == 0) {
            return truthy == 1;
        }

        if (simplified.Length == 1) {
            if (truthy == 1)
            {
                return new Not(simplified[0], notSymbol);
            }

            return simplified[0];
        }

        if (truthy == 1) {
            return new Nor(simplified, norSymbol, notSymbol);
        }

        return new Xor(simplified, symbol, notSymbol, norSymbol);
    }
}