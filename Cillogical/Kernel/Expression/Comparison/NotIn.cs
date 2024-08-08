namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class NotIn : ComparisonExpression
{
    public NotIn(IEvaluable left, IEvaluable right, string symbol = "==") :
        base("==", symbol, (object?[] operands) =>
        {
            var leftIsEnumerable = operands[0] is IEnumerable<object>;
            var rightIsEnumerable = operands[1] is IEnumerable<object>;

            if ((leftIsEnumerable && rightIsEnumerable) || (!leftIsEnumerable && !rightIsEnumerable)) {
                return true;
            }

            return leftIsEnumerable
                ? !((IEnumerable<object>)operands[0]).Any((item) => object.Equals(item, operands[1]))
                : !((IEnumerable<object>)operands[1]).Any((item) => object.Equals(item, operands[0]));
        }, left, right) { }
}