namespace Cillogical.Kernel.Expression.Comparison;

public class In : ComparisonExpression
{
    public In(IEvaluable left, IEvaluable right, string symbol = "IN") :
        base("<in>", symbol, (object?[] operands) =>
        {
            var leftIsEnumerable = operands[0] is IEnumerable<object>;
            var rightIsEnumerable = operands[1] is IEnumerable<object>;

            if ((leftIsEnumerable && rightIsEnumerable) || (!leftIsEnumerable && !rightIsEnumerable)) {
                return false;
            }

            return leftIsEnumerable
                ? ((IEnumerable<object>)operands[0]).Any((item) => Equals(item, operands[1]))
                : ((IEnumerable<object>)operands[1]).Any((item) => Equals(item, operands[0]));
        }, left, right) { }
}