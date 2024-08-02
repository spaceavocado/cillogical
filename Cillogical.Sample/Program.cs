using Cillogical;
using Cillogical.Kernel;
using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;

class Program
{
    static void Main(string[] args)
    {
        var eq = new ComparisonExpression("X", "X", (a) => true, new Value("hello"), new Value(2));
        Dictionary<string, Object> a = new Dictionary<string, Object>();
        Console.WriteLine(eq.ToString());

        Console.WriteLine(new Value(5).Evaluate(a).Equals(5));
        Console.WriteLine(new Value(5).Evaluate(a).GetType());
    }
}