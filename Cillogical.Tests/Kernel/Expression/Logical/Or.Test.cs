﻿using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class OrTest
{
    public static IEnumerable<object[]> IvliadArgumentTestData()
    {
        yield return new object[] { new IEvaluable[] { new Value(true) } };
    }

    [Theory]
    [MemberData(nameof(IvliadArgumentTestData))]
    public void IvliadArgument(IEvaluable[] operands)
    {
        Assert.Throws<ArgumentException>(() => new Or(operands));
    }

    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true), new Value(false) }, true };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value(true) }, true };
        // Falsy
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(false) }, false };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value("bogus") }, false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable[] operands, bool expected)
    {
        var expression = new Or(operands);
        Assert.Equal(expected, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(false), new Value(true) }, true };
        yield return new object[] { new IEvaluable[] { new Value(true), new Value(false) }, true };
        yield return new object[] { new IEvaluable[] { new Reference("RefA"), new Value(false) }, true };
        yield return new object[] { new IEvaluable[] { new Reference("Missing"), new Value(false) }, new Reference("Missing") };
        yield return new object[] {
            new IEvaluable[] { new Reference("Missing"), new Reference("Missing") },
            new Or(new IEvaluable[] { new Reference("Missing"), new Reference("Missing") })
        };
        yield return new object[] { new IEvaluable[] { new Value(1), new Value(false) }, false };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable[] operands, object expected)
    {
        var expression = new Or(operands);
        var simplified = expression.Simplify(new Dictionary<string, object> { { "RefA", true } });

        if (expected is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
        
    }
}