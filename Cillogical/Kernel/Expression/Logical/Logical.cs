namespace Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel;
using System.Linq;

public abstract class LogicalExpression : IEvaluable
{
    private string op;
    protected string symbol;
    protected IEvaluable[] operands;

    public LogicalExpression(string op, string symbol, params IEvaluable[] operands)
    {
        this.op = op;
        this.symbol = symbol;
        this.operands = operands;
    }

    public abstract object Evaluate(Dictionary<string, object>? context);

    public object Serialize() =>
        new object[] { symbol }.Concat(operands.Select((operand) => operand.Serialize()));


    public abstract object Simplify(Dictionary<string, object>? context);

    public override string ToString()
    {
        if (operands.Length == 1) {
            return $"({op} {operands[0]})";
        }

        return $"({String.Join($" {op} ", operands.Select((operand) => operand.ToString()))})";
    }
}