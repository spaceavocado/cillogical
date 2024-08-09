namespace Cillogical.Kernel;

public interface IEvaluable {
    object? Evaluate(Dictionary<string, object>? context);
    object? Serialize();
    object? Simplify(Dictionary<string, object>? context);
    string ToString();
}

public class FlattenContext<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {} 

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

public static class ContextUtils
{
    public static Dictionary<string, object>? FlattenContext(Dictionary<string, object>? context) {
        if (context == null) {
            return null;
        }
        if (context is FlattenContext<string, object>) {
            return context;
        }

        var res = new FlattenContext<string, object>();

        Action<object, string>? lookup = null;
        lookup = (object value, string path) =>
        {
            switch (value)
            {
                case int:
                case double:
                case decimal:
                case float:
                case string:
                case char:
                case bool:
                    res[path] = value;
                    break;
                case Dictionary<string, object> dict:
                    foreach (var entry in dict)
                    {
                        lookup?.Invoke(entry.Value, JoinPath(path, entry.Key));
                    }
                    break;
                case object[] array:
                    for (var i = 0; i < array.Length; i++)
                    {
                        lookup?.Invoke(array[i], $"{path}[{i}]");
                    }
                    break;
                default:
                    return;
            }
        };

        lookup(context, "");
        return res;
    }

    public static string JoinPath(string a, string b) => a.Length == 0 ? b : $"{a}.{b}";
}