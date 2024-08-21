namespace Cillogical.Kernel.Expression.Comparison;
public class Overlap : ComparisonExpression
{
    public Overlap(IEvaluable left, IEvaluable right, string symbol = "OVERLAP") :
        base("<overlaps>", symbol, (object?[] operands) =>
        {
            var left = operands[0] is IEnumerable<object>
                ? (IEnumerable<object>)operands[0]
                : [operands[0]];

            var right = operands[1] is IEnumerable<object>
                ? (IEnumerable<object>)operands[1]
                : [operands[1]];

            foreach (var i in left) {
                foreach (var j in right) {
                    if (Equals(i, j)) {
                        return true;
                    }
                }
            }

            return false;
        }, left, right) { }
}