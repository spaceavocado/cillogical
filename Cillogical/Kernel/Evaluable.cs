namespace Cillogical.Kernel;

public interface Evaluable {
    object Evaluate(Context? context);
    object Serialize();
    (object?, Evaluable?) Simplify(Context? context);
    string ToString();
}

public static class Primitive
{
    public static bool IsPrimitive(object value) => value switch
    {
        string => true,
        char => true,
        int => true,
        decimal => true,
        float => true,
        double => true,
        bool => true,
        _ => false
    };
}