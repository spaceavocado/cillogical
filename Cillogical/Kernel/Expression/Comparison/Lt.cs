﻿namespace Cillogical.Kernel.Expression.Comparison;

public class Lt : ComparisonExpression
{
    public Lt(IEvaluable left, IEvaluable right, string symbol = "<") :
        base(
            "<",
            symbol,
            (object?[] operands) => IsNumber(operands[0]) && IsNumber(operands[1])
                ? (dynamic)operands[0] < (dynamic)operands[1]
                : false,
            left,
            right
        ) { }
}