using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class LogicalExpressionMock : LogicalExpression
{
    public LogicalExpressionMock(IEvaluable[] operands, string symbol) : base(symbol, symbol, operands) { }
    public override object Evaluate(Dictionary<string, object?>? context)
    {
        throw new NotImplementedException();
    }
    public override object Simplify(Dictionary<string, object?>? context)
    {
        throw new NotImplementedException();
    }
}

public class LogicalTest
{
    public static IEnumerable<object[]> SerializeTestData()
    {
        yield return ["->", new IEvaluable[] { new Value(1), new Value(2) }, new object[] { "->", 1, 2 }];
        yield return ["X", new IEvaluable[] { new Value(1) }, new object[] { "X", 1 }];
    }

    [Theory]
    [MemberData(nameof(SerializeTestData))]
    public void Serialize(string op, IEvaluable[] operands, object expected)
    {
        var expression = new LogicalExpressionMock(operands, op);
        Assert.Equal(expected, expression.Serialize());
    }

    public static IEnumerable<object[]> StringifyTestData()
    {
        yield return ["->", new IEvaluable[] { new Value(1), new Value("2") }, "(1 -> \"2\")"];
        yield return ["->", new IEvaluable[] { new Value(1), new Value("2"), new Value(1) }, "(1 -> \"2\" -> 1)"];
        yield return ["X", new IEvaluable[] { new Value(1) }, "(X 1)"];
    }

    [Theory]
    [MemberData(nameof(StringifyTestData))]
    public void Stringify(string op, IEvaluable[] operands, object expected)
    {
        var expression = new LogicalExpressionMock(operands, op);
        Assert.Equal(expected, $"{expression}");
    }
}