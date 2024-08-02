namespace Cillogical.Kernel.Operand;
using Cillogical.Kernel;

public class Value : Evaluable
{
    private object value;

    public Value(object value)
    {
        if (!Primitive.IsPrimitive(value)) {
            throw new ArgumentException($"value {value.GetType()} could be only primitive type, string, number or bool");
        }
        this.value = value;
    }

    public object Evaluate(Context? context = null) => value;

    public object Serialize() => value;

    public (object?, Evaluable?) Simplify(Context? context = null) => (value, null);

    public override string ToString() =>
        this.value switch
        {
            string => $"\"{this.value}\"",
            char => $"\"{this.value}\"",
            _ => $"{this.value}".ToLower()
        };
}