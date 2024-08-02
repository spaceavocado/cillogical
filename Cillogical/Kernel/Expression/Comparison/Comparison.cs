namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;
using System.Linq;

public delegate bool Handler(params object[] operands);

public class ComparisonExpression : Evaluable
{
    private string _operator;
    private string symbol;
    private Handler handler;
    private Evaluable[] operands;

    public ComparisonExpression(string _operator, string symbol, Handler handler, params Evaluable[] operands) {
        this._operator = _operator;
        this.symbol = symbol;
        this.handler = handler;
        this.operands = operands;
    }

    public object Evaluate(Context? context)
    {
        try
        {
            return this.handler(
                (from operand in this.operands select operand.Evaluate(context)).ToArray()
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object Serialize() => new dynamic[] { this.symbol }.Concat(
        this.operands.Select((operand) => operand.Serialize()).Cast<dynamic>()
    ).ToArray();


    public (object?, Evaluable?) Simplify(Context? context)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        var result = $"({this.operands[0]} {this._operator}";
        if (this.operands.Length > 1) { 
            result += $" {String.Join(" ", this.operands.Skip(1))}";
        }
        return $"{result})";
    }
}