namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;
using System.Linq;

public delegate bool Comparison(params object?[] operands);

public abstract class ComparisonExpression : IEvaluable
{
    private string op;
    private string symbol;
    private Comparison comparison;
    private IEvaluable[] operands;

    public ComparisonExpression(string op, string symbol, Comparison comparison, params IEvaluable[] operands) {
        this.op = op;
        this.symbol = symbol;
        this.comparison = comparison;
        this.operands = operands;
    }

    public object Evaluate(Dictionary<string, object>? context)
    {
        try {
            return comparison((from operand in operands select operand.Evaluate(context)).ToArray());
        }
        catch (Exception) {
            return false;
        }
    }

    public object Serialize() =>
        new object[] { symbol }.Concat(operands.Select((operand) => operand.Serialize()));


    public object Simplify(Dictionary<string, object>? context)
    {
        var res = new object?[] { };
        foreach (var operand in operands) {
            var val = operand.Simplify(context);
            if (val is IEvaluable) {
                return this;
            }

            Array.Resize(ref res, res.Length + 1);
            res[res.Length - 1] = val;
        }

        return comparison(res);
    }

    public override string ToString()
    {
        var result = $"({operands[0]} {op}";
        if (operands.Length > 1) { 
            result += $" {String.Join(" ", operands.Skip(1))}";
        }
        return $"{result})";
    }

    public static bool IsNumber(object? subject) =>
        subject is not null && subject switch {
            int => true,
            float => true,
            decimal => true,
            double => true,
            _ => false
        };

    public static bool IsText(object? subject) =>
        subject is not null && subject switch
        {
            string => true,
            char => true,
            _ => false
        };
}

// in
// not in
// overlap