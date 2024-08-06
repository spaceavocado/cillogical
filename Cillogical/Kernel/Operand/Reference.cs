namespace Cillogical.Kernel.Operand;
using Cillogical.Kernel;
using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    string From(string operand);
    string To(string operand);
}

public interface ISimplifyOptions {
    string[] IgnoredPaths { get; }
    Regex[] IgnoredPathsRx { get; }
}

public class DefaultSerializeOptions : ISerializeOptions
{
    public string From(string operand) {
        if (operand.Length > 1 && operand.StartsWith("$"))
        {
            return operand.Substring(1);
        }

        throw new ArgumentException("invalid operand");
    }
    public string To(string operand) { return $"${operand}"; }
}

public class Reference : IEvaluable
{
    private string address;
    private string path;
    private DataType dataType;
    private ISerializeOptions serializeOptions;
    private ISimplifyOptions? simplifyOptions;
    private object value;

    private const string NESTED_REFERENCE_RX = @"{([^{}]+)}";

    public Reference(string address, ISerializeOptions? serializeOptions = null, ISimplifyOptions? simplifyOptions = null)
    {
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

    public object? Evaluate(Dictionary<string, object>? context = null)
    {
        if (context == null) {
            return null;
        }

        context = ContextUtils.FlattenContext(context);
        var value = ContextLookup(context, path);

        if (value == null)
        {
            return null;
        }

        return dataType switch
        {
            DataType.Number => value,
            DataType.Integer => ToInteger(value),
            DataType.Float => ToFloat(value),
            DataType.Boolean => ToBoolean(value),
            DataType.String => ToString(value),
            _ => value,
        };
    }

    public object Serialize() => serializeOptions.To(
        dataType != DataType.Undefined
            ? $"{path}.({dataType})"
            : path
        );

    public (object?, IEvaluable?) Simplify(Dictionary<string, object>? context = null) => (value, null);

    public override string ToString() => $"{{{address}}}";

    public static string DefaultSerializeTo(string operand) => $"${operand}";

    public static DataType GetDataType(string path) {
        var re = new Regex(@"^.+\.\(([A-Z][a-z]+)\)$");
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
        var re = new Regex(@".\(([A-Z][a-z]+)\)$");
        return re.Replace(path, "");
    }

    public static object? ContextLookup(Dictionary<string, object> context, string path)
    {
        var re = new Regex(NESTED_REFERENCE_RX);

        var match = re.Match(path);
        while (match.Success)
        {
            var val = ContextLookup(context, match.Groups[0].Value);
            if (val == null) {
                return null;
            }

            path = path.Substring(0, match.Index) + val + path.Substring(match.Index + match.Length);
            match = re.Match(path);
        }

        if (context.ContainsKey(path)) {
            return context[path];
        }

        return null;
    }

    public static object ToNumber(object value)
    {
        return 0;
    }

    public static int ToInteger(object value)
    {
        switch (value)
        {
            case int number:
                return number;
            case float number:
                return (int)Math.Ceiling(number);
            case string text:
                return int.Parse(text);
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
                return number * 1f;
            case float number:
                return number;
            case string text:
                return float.Parse(text);
            default:
                throw new InvalidCastException($"invalid conversion from \"{value}\" to float");
        }
    }

    public static string ToString(object value) {
        var text = value.ToString();
        if (text == null)
        {
            throw new InvalidCastException($"invalid conversion from \"{value}\" to string");
        }

        return text;
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
}