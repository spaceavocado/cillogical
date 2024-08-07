namespace Cillogical.Kernel.Operand;
using Cillogical.Kernel;
using System.Linq;

public class Collection : IEvaluable
{
    private IEvaluable[] items;
    private char escapeCharacter;
    private HashSet<string> escapedOperators;

    public Collection(IEvaluable[] items, char? escapeCharacter = null, HashSet<string>? escapedOperators = null)
    {
        if (items.Length < 1)
        {
            throw new ArgumentException("collection operand must have at least 1 item");
        }

        this.items = items;
        this.escapeCharacter = escapeCharacter ?? '\\';
        this.escapedOperators = escapedOperators ?? new HashSet<string>();
    }

    public object Evaluate(Dictionary<string, object>? context = null) =>
        items.Select((item) => item.Evaluate(context)).ToArray();

    public object Serialize() {
        var head = items[0].Serialize();
        if (ShouldBeEscaped(head, escapedOperators)) {
            head = EscapeOperator((string)head, escapeCharacter);
        }

        return new object[] { head }.Concat(items.Skip(1).Select((item) => item.Serialize())).ToArray();
    }

    public object Simplify(Dictionary<string, object>? context = null) {
        var res = new object?[] { };
        foreach (var item in items) {
            var val = item.Simplify(context);
            if (val is IEvaluable) {
                return this;
            }

            Array.Resize(ref res, res.Length + 1);
            res[res.Length - 1] = val;
        }

        return res;
    }

    public override string ToString() =>
        $"[{String.Join(", ", items.Select((item) => item.ToString()))}]";

    public static bool ShouldBeEscaped(object subject, HashSet<string> escapedOperators)
    {
        if (subject == null) {
            return false;
        }

        return subject is string && escapedOperators.Contains(subject);
    }

    public static string EscapeOperator(string op, char escapeOperator) => $"{escapeOperator}{op}";
}