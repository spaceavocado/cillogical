using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel;

public class EvalualeTest
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(1.1d, true)]
    [InlineData(1.1f, true)]
    [InlineData("val", true)]
    [InlineData('c', true)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(null, false)]
    [InlineData(new int[] { 1 }, false)]
    public void IsPrimitive(object input, bool expected)
    {
        Assert.Equal(expected, Primitive.IsPrimitive(input));
    }

    public static IEnumerable<object?[]> FlattenContextData()
    {
        yield return new object?[] { null, null };

        yield return new object[] {
            new FlattenContext<string, object> { { "a", 1 } },
            new FlattenContext<string, object> { { "a", 1 } }
        };

        yield return new object[] {
            new Dictionary<string, object?> { { "a", 1 } },
            new Dictionary<string, object?> { { "a", 1 } }
        };

        yield return new object[] {
            new Dictionary<string, object?> {
                { "a", 1 },
                { "b", new Dictionary<string, object?> {
                    { "c", 5 },
                    { "d", true }
                }},
                { "c", null },
            },
            new Dictionary<string, object?> { { "a", 1 }, { "b.c", 5 }, { "b.d", true }, { "c", null } }
        };

        yield return new object[] {
            new Dictionary<string, object?> {
                { "a", 1 },
                { "b", new object[] { 1, "val", true }
            }},
            new Dictionary<string, object?> { { "a", 1 }, { "b[0]", 1 }, { "b[1]", "val" }, { "b[2]", true } }
        };

        yield return new object[] {
            new Dictionary<string, object?> {
                { "a", 1 },
                { "b", new object[] {
                    1.1,
                    new Dictionary<string, object?>
                    {
                        { "c", false },
                        { "d", 1.2f }
                    },
                    'c' 
                }}
            },
            new Dictionary<string, object?> { { "a", 1 }, { "b[0]", 1.1 }, { "b[1].c", false }, { "b[1].d", 1.2f }, { "b[2]", 'c' } }
        };
    }

    [Theory]
    [MemberData(nameof(FlattenContextData))]
    public void FlattenContext(Dictionary<string, object?>? input, Dictionary<string, object?>? expected)
    {
        Assert.Equal(expected, ContextUtils.FlattenContext(input));
    }
}