using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Operand;

public class CollectionTest
{
    public static IEnumerable<object[]> InvalidItemsTestData()
    {
        yield return new object[] { new IEvaluable[] { } };
    }

    [Theory]
    [MemberData(nameof(InvalidItemsTestData))]
    public void InvalidItems(IEvaluable[] items)
    {
        Assert.Throws<ArgumentException>(() => new Collection(items));
    }

    [Theory]
    [InlineData("==", true)]
    [InlineData("!=", false)]
    [InlineData(null, false)]
    [InlineData(true, false)]
    public void ShouldBeEscaped(object subject, bool expected)
    {
        Assert.Equal(expected, Collection.ShouldBeEscaped(subject, new HashSet<string>() { "==" }));
    }

    [Theory]
    [InlineData("==", '\\', "\\==")]
    [InlineData("==", 'g', "g==")]
    public void EscapeOperator(string subject, char escapeOperator, string expected)
    {
        Assert.Equal(expected, Collection.EscapeOperator(subject, escapeOperator));
    }

    public static IEnumerable<object[]> EvaluateTestData()
    {
        yield return new object[] { new IEvaluable[] { new Value(1) }, new object[] { 1 } };
        yield return new object[] { new IEvaluable[] { new Value("1") }, new object[] { "1" } };
        yield return new object[] { new IEvaluable[] { new Value(true) }, new object[] { true } };
        yield return new object[] { new IEvaluable[] { new Reference("RefA") }, new object[] { "A" } };
        yield return new object[] { new IEvaluable[] { new Value(1), new Reference("RefA") }, new object[] { 1, "A" } };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable[] items, object[] expected)
    {
        var collection = new Collection(items);
        Assert.Equal(expected, collection.Evaluate(new Dictionary<string, object?> { { "RefA", "A" } }));
    }

    public static IEnumerable<object[]> SerializeTestData()
    {
        yield return new object[] { new IEvaluable[] { new Value(1) }, new object[] { 1 } };
        yield return new object[] { new IEvaluable[] { new Value("1") }, new object[] { "1" } };
        yield return new object[] { new IEvaluable[] { new Value(true) }, new object[] { true } };
        yield return new object[] { new IEvaluable[] { new Reference("RefA") }, new object[] { "$RefA" } };
        yield return new object[] { new IEvaluable[] { new Value("=="), new Value(1), new Value(1) }, new object[] { "\\==", 1, 1 } };
        yield return new object[] { new IEvaluable[] { new Value("!="), new Value(1), new Value(1) }, new object[] { "!=", 1, 1 } };
    }

    [Theory]
    [MemberData(nameof(SerializeTestData))]
    public void Serialize(IEvaluable[] items, object[] expected)
    {
        var collection = new Collection(items, escapedOperators: new HashSet<string> { "==" });
        Assert.Equal(expected, collection.Serialize());
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return new object[] {
            new IEvaluable[] { new Reference("RefB") },
            new Collection(new IEvaluable[] { new Reference("RefB") })
        };
        yield return new object[] {
            new IEvaluable[] { new Reference("RefA") },
            new object[] { "A" }
        };
        yield return new object[] {
            new IEvaluable[] { new Value(1), new Reference("RefA") },
            new object[] { 1, "A" }
        };
        yield return new object[] {
            new IEvaluable[] { new Reference("RefA"), new Reference("RefB")  },
            new Collection(new IEvaluable[] { new Reference("RefA"), new Reference("RefB") })
        };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable[] items, object expected)
    {
        var collection = new Collection(items);
        var simplified = collection.Simplify(new Dictionary<string, object?> { { "RefA", "A" } });

        if (simplified is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
    }

    public static IEnumerable<object[]> StringifyTestData()
    {
        yield return new object[] { new IEvaluable[] { new Value(1) }, "[1]" };
        yield return new object[] { new IEvaluable[] { new Value("1") }, "[\"1\"]" };
        yield return new object[] { new IEvaluable[] { new Value(true) }, "[true]" };
        yield return new object[] { new IEvaluable[] { new Reference("RefA") }, "[{RefA}]" };
        yield return new object[] { new IEvaluable[] { new Value(1), new Reference("RefA") }, "[1, {RefA}]" };
    }

    [Theory]
    [MemberData(nameof(StringifyTestData))]
    public void Stringify(IEvaluable[] items, string expected)
    {
        var collection = new Collection(items);
        Assert.Equal(expected, $"{collection}");
    }
}