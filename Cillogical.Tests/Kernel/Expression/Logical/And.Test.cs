﻿using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class AndTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true), new Value(true) }, true };
        // Falsy
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(false) }, false };
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(true) }, false };
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(false) }, false };
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(1) }, false };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value(true) }, false };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value("bogus") }, false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable[] operands, bool expected)
    {
        var expression = new And(operands);
        Assert.Equal(expected, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        // Truthy
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(true) }, false };
        yield return new object[] { new IEvaluable[] { new Reference("RefA"), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Reference("Missing"), new Value(true) }, new Reference("Missing") };
        yield return new object[] {
            new IEvaluable[] { new Reference("Missing"), new Reference("Missing") },
            new And(new IEvaluable[] { new Reference("Missing"), new Reference("Missing") })
        };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value(true) }, new Value(1) };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable[] operands, object expected)
    {
        var expression = new And(operands);
        var simplified = expression.Simplify(new Dictionary<string, object> { { "RefA", true } });

        if (expected is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
        
    }
}