using System.Text.RegularExpressions;

namespace Cillogical.Kernel.Operand;

public enum DataType
{
    Undefined,
    Unsupported,
    Number,
    Integer,
    Float,
    String,
    Boolean
}

public interface ISerializeOptions
{
    string? From(string operand);
    string To(string operand);
}

public interface ISimplifyOptions {
    string[]? IgnoredPaths { get; }
    Regex[]? IgnoredPathsRx { get; }
}

public class DefaultSerializeOptions : ISerializeOptions
{
    public string? From(string operand) =>
        operand.Length > 1 && operand.StartsWith("$")
            ? operand.Substring(1)
            : null;

    public string To(string operand) => $"${operand}";
}

public class Reference : IEvaluable
{
    private string address;
    private string path;
    private DataType dataType;
    private ISerializeOptions serializeOptions;
    private ISimplifyOptions? simplifyOptions;

    private const string NESTED_REFERENCE_RX = @"{([^{}]+)}";
    private const string DATA_TYPE_RX = @"^.+\.\(([A-Z][a-z]+)\)$";
    private const string DATA_TYPE_TRIM_RX = @".\(([A-Z][a-z]+)\)$";
    private const string FLOAT_TRIM_RX = @"\.\d+";
    private const string FLOAT_RX = @"^\d+\.\d+$";
    private const string INT_RX = @"^0$|^[1-9]\d*$";

    public Reference(
        string address,
        ISerializeOptions? serializeOptions = null,
        ISimplifyOptions? simplifyOptions = null
    ) {
        var dataType = GetDataType(address);
        if (dataType == DataType.Unsupported) {
            throw new ArgumentException($"unsupported type casting, {address}");
        }

        this.address = address;
        this.path = TrimDataType(address);
        this.dataType = dataType;
        this.serializeOptions = serializeOptions ?? new DefaultSerializeOptions();
        this.simplifyOptions = simplifyOptions;
    }

    public object? Evaluate(Dictionary<string, object?>? context = null)
    {
        if (context == null) {
            return null;
        }

        context = ContextUtils.FlattenContext(context);
        var (_, _, value) = Evaluate(context, path, dataType);

        return value;
    }

    public object Serialize() => serializeOptions.To(
        dataType != DataType.Undefined
            ? $"{path}.({dataType})"
            : path
        );

    public object? Simplify(Dictionary<string, object?>? context = null) {
        if (context == null) {
            return this;
        }

        context = ContextUtils.FlattenContext(context);
        var (found, resolvedPath, value) = Evaluate(context, path, dataType);

        if (found && !IsIgnoredPath(path, simplifyOptions?.IgnoredPaths, simplifyOptions?.IgnoredPathsRx)) {
            return value;
        }

        return this;
    }

    public static DataType GetDataType(string path) {
        var re = new Regex(DATA_TYPE_RX);
        var matches = re.Matches(path);

        if (matches.Count > 0) {
            switch (matches[0].Groups[1].Value) {
                case "Number":
                    return DataType.Number;
                case "Integer":
                    return DataType.Integer;
                case "Float":
                    return DataType.Float;
                case "String":
                    return DataType.String;
                case "Boolean":
                    return DataType.Boolean;
                default:
                    return DataType.Unsupported;
            }
        }

        return DataType.Undefined;
    }

    public static string TrimDataType(string path) {
        var re = new Regex(DATA_TYPE_TRIM_RX);
        return re.Replace(path, "");
    }

    public static (bool, string, object?) Evaluate(Dictionary<string, object?>? context, string path, DataType dataType)
    {
        context = ContextUtils.FlattenContext(context);
        var (found, resolvedPath, value) = ContextLookup(context, path);

        value = value is not null ? dataType switch
        {
            DataType.Number => ToNumber(value),
            DataType.Integer => ToInteger(value),
            DataType.Float => ToFloat(value),
            DataType.Boolean => ToBoolean(value),
            DataType.String => ToString(value),
            _ => value,
        } : null;

        return (found, resolvedPath, value);
    }

    public static (bool, string, object?) ContextLookup(Dictionary<string, object?>? flattenContext, string path)
    {
        if (flattenContext is null) {
            return (false, path, null);
        }

        var re = new Regex(NESTED_REFERENCE_RX);

        var match = re.Match(path);
        while (match.Success)
        {
            var (found, _, val) = ContextLookup(flattenContext, match.Groups[1].Value);
            if (!found) {
                return (false, path, null);
            }

            path = path.Substring(0, match.Index) + val + path.Substring(match.Index + match.Length);
            match = re.Match(path);
        }

        if (flattenContext.ContainsKey(path)) {
            return (true, path, flattenContext[path]);
        }

        return (false, path, null);
    }

    public static bool IsIgnoredPath(string path, string[]? ignoredPaths = null, Regex[]? ignoredPathsRx = null)
    {
        if (ignoredPaths != null && ignoredPaths.Any((needle) => needle == path)) {
            return true;
        }

        if (ignoredPathsRx != null && ignoredPathsRx.Any((pattern) => pattern.IsMatch(path))) {
            return true;
        }

        return false;
    }

    public static object ToNumber(object value)
    {
        switch (value)
        {
            case int:
            case float:
            case double:
                return value;
            case bool bit:
                return bit ? 1 : 0;
            case string text:
                if (new Regex(FLOAT_RX).IsMatch(text))
                {
                    return float.Parse(text);
                }
                if (new Regex(INT_RX).IsMatch(text))
                {
                    return int.Parse(text);
                }
                throw new InvalidCastException($"invalid conversion from \"{value}\" text to number");
            default:
                throw new InvalidCastException($"invalid conversion from \"{value}\" to number");
        }
    }

    public static int ToInteger(object value)
    {
        switch (value)
        {
            case int number:
                return number;
            case float number:
                return (int)Math.Floor(number);
            case double number:
                return (int)Math.Floor(number);
            case string text:
                if (new Regex(FLOAT_RX).IsMatch(text))
                {
                    return int.Parse(new Regex(FLOAT_TRIM_RX).Replace(text, ""));
                }
                if (new Regex(INT_RX).IsMatch(text))
                {
                    return int.Parse(text);
                }
                throw new InvalidCastException($"invalid conversion from \"{value}\" text to int");
            case bool bit:
                return bit ? 1 : 0;
            default:
                throw new InvalidCastException($"invalid conversion from \"{value}\" to int");
        }
    }

    public static float ToFloat(object value)
    {
        switch (value)
        {
            case int number:
                return number;
            case float number:
                return number;
            case double number:
                return (float)number;
            case string text:
                try {
                    return float.Parse(text);
                } catch {
                    throw new InvalidCastException($"invalid conversion from \"{value}\" text to float");
                }
            default:
                throw new InvalidCastException($"invalid conversion from \"{value}\" to float");
        }
    }

    public static string ToString(object value) {
        var text = value.ToString() ?? string.Empty;
        return value is bool ? text.ToLower() : text; 
    }

    public static bool ToBoolean(object value)
    {
        switch (value)
        {
            case int number:
                if (number == 1) return true;
                if (number == 0) return false;
                throw new InvalidCastException($"invalid conversion from \"{number}\" number to boolean");
            case string text:
                text = text.Trim().ToLower();
                if (text == "true" || text == "1") return true;
                if (text == "false" || text == "0") return false;
                throw new InvalidCastException($"invalid conversion from \"{text}\" string to boolean");
            case bool bit:
                return bit;
            default:
                throw new InvalidCastException($"invalid conversion from \"{value}\" to boolean");
        }
    }

    public override string ToString() => $"{{{address}}}";
}