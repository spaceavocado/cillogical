namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Overlap : ComparisonExpression
{
    public Overlap(IEvaluable left, IEvaluable right, string symbol = "OVERLAP") :
        base("<overlaps>", symbol, (object?[] operands) =>
        {
            var leftIsEnumerable = operands[0] is IEnumerable<object>;
            var rightIsEnumerable = operands[1] is IEnumerable<object>;

            if (!leftIsEnumerable || !rightIsEnumerable) {
                return false;
            }

            foreach (var i in (IEnumerable<object>)operands[0]) {
                foreach (var j in (IEnumerable<object>)operands[1]) {
                    if (object.Equals(i, j)) {
                        return true;
                    }
                }
            }

            return false;
        }, left, right) { }
}