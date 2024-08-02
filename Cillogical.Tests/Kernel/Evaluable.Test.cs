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
        Assert.Equal(Primitive.IsPrimitive(input), expected);
    }
}