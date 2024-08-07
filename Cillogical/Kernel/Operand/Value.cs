namespace Cillogical.Kernel.Operand;
using Cillogical.Kernel;

public class Value : IEvaluable
{
    private object? value;

    public Value(object? value)
    {
        if (value != null && !Primitive.IsPrimitive(value)) {
            throw new ArgumentException($"value {value.GetType()} could be only a primitive type, null, string, number or bool");
        }
        this.value = value;
    }

    public object? Evaluate(Dictionary<string, object>? context = null) => value;

    public object? Serialize() => value;

    public object? Simplify(Dictionary<string, object>? context = null) => value;

    public override string ToString() =>
        this.value switch
        {
            string => $"\"{this.value}\"",
            char => $"\"{this.value}\"",
            _ => $"{this.value}".ToLower()
        };
}