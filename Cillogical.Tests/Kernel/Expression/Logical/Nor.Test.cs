﻿using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class NorTest
{
    public static IEnumerable<object[]> IvliadArgumentTestData()
    {
        yield return [new IEvaluable[] { new Value(true) }];
    }

    [Theory]
    [MemberData(nameof(IvliadArgumentTestData))]
    public void IvliadArgument(IEvaluable[] operands)
    {
        Assert.Throws<ArgumentException>(() => new Nor(operands));
    }

    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new IEvaluable[] { new Value(false), new Value(false) }, true];
        yield return [new IEvaluable[] { new Value(false), new Value(false), new Value(false) }, true];
        // Falsy
        yield return [new IEvaluable[] { new Value(true), new Value(true) }, false];
        yield return [new IEvaluable[] { new Value(false), new Value(true) }, false];
        yield return [new IEvaluable[] { new Value(true), new Value(true) }, false];
        yield return [new IEvaluable[] { new Value(true), new Value(true), new Value(false) }, false];
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable[] operands, bool expected)
    {
        var expression = new Nor(operands);
        Assert.Equal(expected, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> EvaluateInvalidOperandTestData()
    {
        yield return [new IEvaluable[] { new Value(1), new Value(true) }];
        yield return [new IEvaluable[] { new Value(false), new Value(1) }];
        yield return [new IEvaluable[] { new Value(1), new Value("bogus") }];
    }

    [Theory]
    [MemberData(nameof(EvaluateInvalidOperandTestData))]
    public void EvaluateInvalidOperand(IEvaluable[] operands)
    {
        var expression = new Nor(operands);
        Assert.Throws<InvalidExpressionException>(() => expression.Evaluate(null));
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return [new IEvaluable[] { new Value(false), new Value(false) }, true];
        yield return [new IEvaluable[] { new Value(false), new Value(true) }, false];
        yield return [new IEvaluable[] { new Value(true), new Value(false) }, false];
        yield return [new IEvaluable[] { new Value(true), new Value(true) }, false];
        yield return [new IEvaluable[] { new Reference("RefA"), new Value(false) }, false];
        yield return [new IEvaluable[] { new Reference("Missing"), new Value(false) }, new Not(new Reference("Missing"))];
        yield return [
            new IEvaluable[] { new Reference("Missing"), new Reference("Missing") },
            new Nor([new Reference("Missing"), new Reference("Missing")])
        ];
        yield return [new IEvaluable[] { new Value(false), new Reference("invalid") }, new Not(new Reference("invalid"))];
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable[] operands, object expected)
    {
        var expression = new Nor(operands);
        var simplified = expression.Simplify(new Dictionary<string, object?> { { "RefA", true }, { "invalid", 1 } });

        if (expected is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
        
    }
}