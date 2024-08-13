using Cillogical;
using Cillogical.Kernel;
using Cillogical.Kernel.Operand;
using System.Text.RegularExpressions;

public class SimplifyOptions : ISimplifyOptions
{
    public string[]? IgnoredPaths { get; }

    public Regex[]? IgnoredPathsRx { get; }

    public SimplifyOptions(string[]? ignoredPaths, Regex[]? ignoredPathsRx)
    {
        IgnoredPaths = ignoredPaths;
        IgnoredPathsRx = ignoredPathsRx;
    }
}

class SerializeOptions : ISerializeOptions
{
    public string? From(string operand) =>
        operand.Length > 2 && operand.StartsWith("__")
            ? operand.Substring(2)
            : null;

    public string To(string operand) => $"__{operand}";
}

class Program
{
    static void BasicUsage()
    {
        var illogical = new Illogical();
        var context = new Dictionary<string, object?>
        {
            { "name", "peter" }
        };

        // 1. Evaluate expression
        illogical.Evaluate(new object[] { "==", 1, 1 }, null); // Context is optional

        // Comparison expression
        illogical.Evaluate(new object[] { "==", 5, 5 }, context);
        illogical.Evaluate(new object[] { "==", "circle", "circle" }, context);
        illogical.Evaluate(new object[] { "==", true, true }, context);
        illogical.Evaluate(new object[] { "==", "$name", "peter" }, context);
        illogical.Evaluate(new object[] { "NIL", "$RefA" }, context);

        // Logical expression
        illogical.Evaluate(new object[] { "AND", new object[] { "==", 5, 5 }, new object[] { "==", 10, 10 } }, context);
        illogical.Evaluate(new object[] { "AND", new object[] { "==", "circle", "circle" }, new object[] { "==", 10, 10 } }, context);
        illogical.Evaluate(new object[] { "OR", new object[] { "==", "$name", "peter" }, new object[] { "==", 5, 10 } }, context);

        // 2. Get expression statement

        illogical.Statement(new object[] { "==", 5, 5 }); // (5 == 5)
        illogical.Statement(new object[] { "==", "circle", "circle" }); // ("circle" == "circle")
        illogical.Statement(new object[] { "==", true, true }); // (True == True)
        illogical.Statement(new object[] { "==", "$name", "peter" }); // ({name} == "peter")
        illogical.Statement(new object[] { "NONE", "$RefA" }); // ({RefA} <is none>)

        illogical.Statement(new object[] {
            "AND",
            new object[] { "==", 5, 5 },
            new object[] { "==", 10, 10 }
        }); // ((5 == 5) AND (10 == 10))

        illogical.Statement(new object[] {
            "AND",
            new object[] { "==", "circle", "circle" },
            new object[] { "==", 10, 10 }
        }); // (("circle" == "circle") AND (10 == 10))

        illogical.Statement(new object[] {
            "OR",
            new object[] { "==", "$name", "peter" },
            new object[] { "==", 5, 10 }
        }); // (({name} == "peter") OR (5 == 10))
    }

    static void WorkingWithIEvaluable()
    {
        var illogical = new Illogical();
        IEvaluable evaluable;

        // 1. Parse expression into IEvaluable
        evaluable = illogical.Parse(new object[] { "==", "$name", "peter" });

        // 2. Evaluate
        evaluable.Evaluate(new Dictionary<string, object?> { { "name", "peter" } }); // True

        // 1. Parse expression into IEvaluable
        evaluable = illogical.Parse(new object[] {
            "AND",
            new object[] { "==", "$a", 10 },
            new object[] { "==", "$b", 20 }
        });

        // 2. Simplify expression into IEvaluable
        evaluable.Simplify(new Dictionary<string, object?> { { "a", 10 } }); // ({b} == 20)
        evaluable.Simplify(new Dictionary<string, object?> { { "a", 20 } }); // false
    }

    static void SimplifingOptions()
    {
        var illogical = new Illogical(simplifyOptions: new SimplifyOptions(
            new string[] { "ignored" }, new Regex[] { new Regex(@"^ignored") }
        ));

        var evaluable = illogical.Parse(new object[] {
            "AND",
            new object[] { "==", "$a", 10 },
            new object[] { "==", "$ignored", 20 }
        });

        evaluable.Simplify(new Dictionary<string, object?> { { "a", 10 } }); // false
        // $ignored" will be evaluated to None.
    }

    static void SerializeOptions()
    {
        var illogical = new Illogical(serializeOptions: new SerializeOptions());
        illogical.Statement("__reference"); // {__reference}, parsed as a reference
        illogical.Statement("reference"); // "__reference", parsed as a value
    }

    static void WorkingWithContext()
    {
        var illogical = new Illogical();
        var context = new Dictionary<string, object?> {
            { "name",    "peter" },
            { "country", "canada" },
            { "age",     21 },
            { "options", new object[]{1, 2, 3 } },
            { "address", new Dictionary<string, object?> {
                { "city", "Toronto" },
                { "country", "Canada" },
            } },
            { "index",     2 },
            { "segment",   "city" },
            { "shapeA",    "box" },
            { "shapeB",    "circle" },
            { "shapeType", "B" },
        };

        // Evaluate an expression in the given data context

        illogical.Evaluate(new object[] { ">", "$age", 20 }, context); // true
        illogical.Evaluate(new object[] { "==", "$address.city", "Toronto" }, context); // true

        // Accessing Array Element
        illogical.Evaluate(new object[] { "==", "$options[1]", 2 }, context); // true

        // Accessing Array Element via Reference
        illogical.Evaluate(new object[] { "==", "$options[{index}]", 3 }, context); // true

        // Nested Referencing
        illogical.Evaluate(new object[] { "==", "$address.{segment}", "Toronto" }, context); // true

        // Composite Reference Key
        illogical.Evaluate(new object[] { "==", "$shape{shapeType}", "circle" }, context); // true

        // Data Type Casting
        illogical.Evaluate(new object[] { "==", "$age.(String)", "21" }, context); // true
    }

    static void EscapeCharacter()
    {
        var illogical = new Illogical(escapeCharacter: '*');
        var expression = new object[] { "*AND", 1, 1 };

        var evaluable = illogical.Parse(expression);
        // new Collection(new IEvaluable[] { new Value("AND"), new Value(1), new Value(1) })

        evaluable = illogical.Parse(new object[] {
            "AND",
            new object[] { "==", "$a", 10 },
            new object[] { "==", "$ignored", 20 }
        });

        evaluable.Simplify(new Dictionary<string, object?> { { "a", 10 } }); // false
        // $ignored" will be evaluated to null.
    }

    static void Main(string[] args)
    {
        BasicUsage();
        WorkingWithIEvaluable();
        SerializeOptions();
        SimplifingOptions();
        EscapeCharacter();
    }
}