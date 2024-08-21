# (c#)illogical

A micro conditional engine used to parse the logical and comparison expressions, evaluate an expression in data context, and provide access to a text form of the given expression.

> Revision: Aug 21, 2024.

Other implementations:
- [TS/JS](https://github.com/spaceavocado/illogical)
- [GO](https://github.com/spaceavocado/goillogical)
- [Python](https://github.com/spaceavocado/pyillogical)

## About

This project has been developed to provide C// implementation of [spaceavocado/illogical](https://github.com/spaceavocado/illogical).

## Getting Started
You can install the **(c#)illogical** from [nuget](https://www.nuget.org/).

```sh
dotnet add package Cillogical --version 1.0.1
```

**Table of Content**

---

- [(c#)illogical](#cillogical)
  - [About](#about)
  - [Getting Started](#getting-started)
- [Basic Usage](#basic-usage)
  - [Evaluate](#evaluate)
  - [Statement](#statement)
  - [Parse](#parse)
  - [IEvaluable](#ievaluable)
    - [Simplify](#simplify)
    - [Serialize](#serialize)
- [Working with Expressions](#working-with-expressions)
  - [Evaluation Data Context](#evaluation-data-context)
    - [Accessing Array Element:](#accessing-array-element)
    - [Accessing Array Element via Reference:](#accessing-array-element-via-reference)
    - [Nested Referencing](#nested-referencing)
    - [Composite Reference Key](#composite-reference-key)
    - [Data Type Casting](#data-type-casting)
  - [Operand Types](#operand-types)
    - [Value](#value)
    - [Reference](#reference)
    - [Collection](#collection)
  - [Comparison Expressions](#comparison-expressions)
    - [Equal](#equal)
    - [Not Equal](#not-equal)
    - [Greater Than](#greater-than)
    - [Greater Than or Equal](#greater-than-or-equal)
    - [Less Than](#less-than)
    - [Less Than or Equal](#less-than-or-equal)
    - [In](#in)
    - [Not In](#not-in)
    - [Prefix](#prefix)
    - [Suffix](#suffix)
    - [Overlap](#overlap)
    - [None](#none)
    - [Present](#present)
  - [Logical Expressions](#logical-expressions)
    - [And](#and)
    - [Or](#or)
    - [Nor](#nor)
    - [Xor](#xor)
    - [Not](#not)
- [Engine Options](#engine-options)
  - [Reference Serialize Options](#reference-serialize-options)
    - [From](#from)
    - [To](#to)
  - [Collection Serialize Options](#collection-serialize-options)
    - [Escape Character](#escape-character)
  - [Simplify Options](#simplify-options)
    - [Ignored Paths](#ignored-paths)
    - [Ignored Paths RegEx](#ignored-paths-regex)
  - [Operator Mapping](#operator-mapping)
- [Contributing](#contributing)
- [License](#license)

---


# Basic Usage

```c#
using Cillogical;

// Create a new instance of the engine
var illogical = new Illogical();

// Evaluate an expression
illogical.Evaluate(new object[] { "==", 1, 1 }, null);
```

> For advanced usage, please [Engine Options](#engine-options).

## Evaluate

Evaluate comparison or logical expression:

`illogical.Evaluate(`[Comparison Expression](#comparison-expressions) or [Logical Expression](#logical-expressions), [Evaluation Data Context](#evaluation-data-context)`)` => `bool`

**Example**

```c#
var context = new Dictionary<string, object?>
{
  { "name", "peter" }
};

// Comparison expression
illogical.Evaluate(new object[]{"==", 5, 5}, context);
illogical.Evaluate(new object[]{"==", "circle", "circle"}, context);
illogical.Evaluate(new object[]{"==", true, true }, context);
illogical.Evaluate(new object[]{"==", "$name", "peter"}, context);
illogical.Evaluate(new object[]{"NULL", "$RefA"}, context);

// Logical expression
illogical.Evaluate(new object[] {
  "AND",
  new object[] { "==", 5, 5 },
  new object[] { "==", 10, 10 }
}, context);

illogical.Evaluate(new object[] {
  "AND",
  new object[] { "==", "circle", "circle" },
  new object[] { "==", 10, 10 }
}, context);

illogical.Evaluate(new object[] {
  "OR",
  new object[] { "==", "$name", "peter" },
  new object[] { "==", 5, 10 }
}, context);
```

## Statement

Get expression string representation:

`illogical.Statement(`[Comparison Expression](#comparison-expressions) or [Logical Expression](#logical-expressions)`)` => `str`

**Example**

```c#
// Comparison expression

illogical.Statement(new object[] { "==", 5, 5 }); // (5 == 5)
illogical.Statement(new object[] { "==", "circle", "circle" }); // ("circle" == "circle")
illogical.Statement(new object[] { "==", true, true }); // (True == True)
illogical.Statement(new object[] { "==", "$name", "peter" }); // ({name} == "peter")
illogical.Statement(new object[] { "NONE", "$RefA" }); // ({RefA} <is none>)

// Logical expression

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
```

## Parse

Parse the expression into a **IEvaluable** object, i.e. it returns the parsed self-evaluable condition expression.

`illogical.parse(`[Comparison Expression](#comparison-expressions) or [Logical Expression](#logical-expressions)`)` => `IEvaluable`

## IEvaluable

- `evaluable.Evaluate(context)` please see [Evaluation Data Context](#evaluation-data-context).
- `evaluable.Simplify(context)` please see [Simplify](#simplify).
- `evaluable.Serialize()` please see [Serialize](#serialize).
- `$"{evaluable}" | evaluable.ToString()` please see [Statement](#statement).

**Example**

```c#
var evaluable = illogical.Parse(new object[] { "==", "$name", "peter" });

evaluable.Evaluate(new Dictionary<string, object?> { { "name", "peter" } }); // true

Console.WriteLine(evaluable); // ({name} == "peter")
```

### Simplify

Simplifies an expression with a given context. This is useful when you already have some of
the properties of context and wants to try to evaluate the expression.

**Example**

```c#
var evaluable = illogical.Parse(new object[] {
  "AND",
  new object[] { "==", "$a", 10 },
  new object[] { "==", "$b", 20 }
});

evaluable.Simplify(new Dictionary<string, object?> { { "a", 10 } }); // ({b} == 20)
evaluable.Simplify(new Dictionary<string, object?> { { "a", 20 } }); // false
```

Values not found in the context will cause the parent operand not to be evaluated and returned
as part of the simplified expression.

In some situations we might want to evaluate the expression even if referred value is not
present. You can provide a list of keys that will be strictly evaluated even if they are not
present in the context.

**Example**

```c#
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

var illogical = new Illogical(simplifyOptions: new SimplifyOptions(
    new string[] { "ignored" }, new Regex[] { new Regex(@"^ignored") }
));

var evaluable = illogical.Parse(new object[] {
    "AND",
    new object[] { "==", "$a", 10 },
    new object[] { "==", "$ignored", 20 }
});

evaluable.Simplify(new Dictionary<string, object?> { { "a", 10 } }); // false
// $ignored" will be evaluated to null.
```

Alternatively we might want to do the opposite and strictly evaluate the expression for all referred
values not present in the context except for a specified list of optional keys.

**Example**

```c#
var illogical = new Illogical(simplifyOptions: new SimplifyOptions(
    new string[] { "ignored" }, new Regex[] { }
));

var evaluable = illogical.Parse(new object[] {
    "OR",
    new object[] { "==", "$a", 10 },
    new object[] { "==", "$b", 20 },
    new object[] { "==", "$c", 20 }
});

evaluable.Simplify(new Dictionary<string, object?> { { "c", 10 } }); // ({a} == 10)
// except for "$b" everything not in context will be evaluated to null.
```

### Serialize

Serializes an expression into the raw expression form, reverse the parse operation.

**Example**

```c#
evaluable = illogical.parse(new object[] {
  "AND",
  new object[] { "==", "$a", 10 },
  new object[] { "==", 10, 20}
});

evaluable.Serialize()
// new object[] { "AND", new object[] { "==", "$a", 10 }, new object[] { "==", 10, 20 } }
```

# Working with Expressions

## Evaluation Data Context

The evaluation data context is used to provide the expression with variable references, i.e. this allows for the dynamic expressions. The data context is object with properties used as the references keys, and its values as reference values.

> Valid reference values: Dictionary, string, char, int, float, decimal, double, array of (bool, string, char, int, float).

To reference the nested reference, please use "." delimiter, e.g.:
`$address.city`

### Accessing Array Element:

`$options[1]`

### Accessing Array Element via Reference:

`$options[{index}]`

- The **index** reference is resolved within the data context as an array index.

### Nested Referencing

`$address.{segment}`

- The **segment** reference is resolved within the data context as a property key.

### Composite Reference Key

`$shape{shapeType}`

- The **shapeType** reference is resolved within the data context, and inserted into the outer reference key.
- E.g. **shapeType** is resolved as "**B**" and would compose the **$shapeB** outer reference.
- This resolution could be n-nested.

### Data Type Casting

`$payment.amount.(Type)`

Cast the given data context into the desired data type before being used as an operand in the evaluation.

> Note: If the conversion is invalid, then a warning message is being logged.

Supported data type conversions:

- .(String): cast a given reference to String.
- .(Number): cast a given reference to Number.
- .(Integer): cast a given reference to Integer.
- .(Float): cast a given reference to Float.
- .(Boolean): cast a given reference to Boolean.

**Example**

```c#
// Data context
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
```

## Operand Types

The [Comparison Expression](#comparison-expression) expect operands to be one of the below:

### Value

Simple value types: string, char, int, float, decimal, double, bool, null.

**Example**

```c#
var val1 = 5;
var var2 = "cirle";
var var3 = true;

illogical.Parse(new object[] {
    "AND",
    new object[] { "==", val1, var2 },
    new object[] { "==", var3, var3 }
});
```

### Reference

The reference operand value is resolved from the [Evaluation Data Context](#evaluation-data-context), where the the operands name is used as key in the context.

The reference operand must be prefixed with `$` symbol, e.g.: `$name`. This might be customized via [Reference Predicate Parser Option](#reference-predicate).

**Example**

| Expression                    | Data Context      |
| ----------------------------- | ----------------- |
| `["==", "$age", 21]`          | `{age: 21}`       |
| `["==", "circle", "$shape"] ` | `{shape: "circle"}` |
| `["==", "$visible", true]`    | `{visible: true}` |

### Collection

The operand could be an array mixed from [Value](#value) and [Reference](#reference).

**Example**

| Expression                               | Data Context                        |
| ---------------------------------------- | ----------------------------------- |
| `["IN", [1, 2], 1]`                      | `null`                                |
| `["IN", "circle", ["$shapeA", "$shapeB"] ` | `{shapeA: "circle", shapeB: "box"}` |
| `["IN", ["$number", 5], 5]`                | `{number: 3}`                       |

## Comparison Expressions

### Equal

Expression format: `["==", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string, char, int, float, decimal, double, bool, null.

```json
["==", 5, 5]
```

```c#
illogical.Evaluate(new object[] { "==", 5, 5 }, context); // true
```

### Not Equal

Expression format: `["!=", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string, char, int, float, decimal, double, bool, null.

```json
["!=", "circle", "square"]
```

```c#
illogical.Evaluate(new object[] { "!=", "circle", "square" }, context); // true
```

### Greater Than

Expression format: `[">", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: int, float.

```json
[">", 10, 5]
```

```c#
illogical.Evaluate(new object[] { ">", 10, 5 }, context); // true
```

### Greater Than or Equal

Expression format: `[">=", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: int, float.

```json
[">=", 5, 5]
```

```c#
illogical.Evaluate(new object[] { ">=", 5, 5 }, context); // true
```

### Less Than

Expression format: `["<", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: int, float.

```json
["<", 5, 10]
```

```c#
illogical.Evaluate(new object[] { "<", 5, 10 }, context); // true
```

### Less Than or Equal

Expression format: `["<=", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: int, float.

```json
["<=", 5, 5]
```

```c#
illogical.Evaluate(new object[] { "<=", 5, 5 }, context); // true
```

### In

Expression format: `["IN", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string, char, int, float, decimal, double, bool, null and an array of (string, char, int, float, decimal, double, bool, null).

```json
["IN", 5, [1, 2, 3, 4, 5]]
["IN", ["circle", "square", "triangle"], "square"]
```

```c#
illogical.Evaluate(new object[] {
    "IN", 5, new object[] { 1, 2, 3, 4, 5 }
}, context); // true

illogical.Evaluate(new object[] {
    "IN", new object[] { "circle", "square", "triangle" }, "square" },
context); // true
```

### Not In

Expression format: `["NOT IN", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string, char, int, float, decimal, double, bool, null and array of (string, char, int, float, decimal, double, bool, null).

```json
["IN", 10, [1, 2, 3, 4, 5]]
["IN", ["circle", "square", "triangle"], "oval"]
```

```c#
illogical.Evaluate(new object[] {
    "NOT IN", 10, new object[] { 1, 2, 3, 4, 5 }
}, context); // true

illogical.Evaluate(new object[] {
    "NOT IN", new object[] { "circle", "square", "triangle" }, "oval" },
context); // true
```

### Prefix

Expression format: `["PREFIX", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string.

- Left operand is the PREFIX term.
- Right operand is the tested word.

```json
["PREFIX", "hemi", "hemisphere"]
```

```c#
illogical.Evaluate(new object[] { "PREFIX", "hemi", "hemisphere" }, context) // true
illogical.Evaluate(new object[] { "PREFIX", "hemi", "sphere" }, context) // false
```

### Suffix

Expression format: `["SUFFIX", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: string.

- Left operand is the tested word.
- Right operand is the SUFFIX term.

```json
["SUFFIX", "establishment", "ment"]
```

```c#
illogical.Evaluate(new object[] { "SUFFIX", "establishment", "ment" }, context) // true
illogical.Evaluate(new object[] { "SUFFIX", "establish", "ment" }, context) // false
```

### Overlap

Expression format: `["OVERLAP", `[Left Operand](#operand-types), [Right Operand](#operand-types)`]`.

> Valid operand types: list; set; tuple of (string, char, int, float, decimal, double, bool, null).

```json
["OVERLAP", [1, 2], [1, 2, 3, 4, 5]]
["OVERLAP", ["circle", "square", "triangle"], ["square"]]
```

```c#
illogical.Evaluate(new object[] {
    "OVERLAP", new object[] { 1, 2, 6 }, new object[] { 1, 2, 3, 4, 5 }
}, context); // true

illogical.Evaluate(new object[] {
    "OVERLAP", new object[] {"circle", "square", "triangle" }, new object[] {"square", "oval" }
}, context); // true
```

### None

Expression format: `["NONE", `[Reference Operand](#reference)`]`.

```json
["NONE", "$RefA"]
```

```c#
illogical.Evaluate(new object[] { "NONE", "RefA" }, null); // true
illogical.Evaluate(new object[] { "NONE", "RefA" }, new Dictionary<string, object?> { { "RefA", 10 } }); // false
```

### Present

Evaluates as FALSE when the operand is UNDEFINED or NULL.

Expression format: `["PRESENT", `[Reference Operand](#reference)`]`.

```json
["PRESENT", "$RefA"]
```

```c#
illogical.Evaluate(new object[] { "PRESENT", "RefA" }, null); // false
illogical.Evaluate(new object[] { "PRESENT", "RefA" }, new Dictionary<string, object?> { { "RefA", 10 } }); // true
illogical.Evaluate(new object[] { "PRESENT", "RefA" }, new Dictionary<string, object?> { { "RefA", false } }); // true
illogical.Evaluate(new object[] { "PRESENT", "RefA" }, new Dictionary<string, object?> { { "RefA", "val" } }); // true
```

## Logical Expressions

### And

The logical AND operator returns the bool value TRUE if both operands are TRUE and returns FALSE otherwise.

Expression format: `["AND", Left Operand 1, Right Operand 2, ... , Right Operand N]`.

> Valid operand types: [Comparison Expression](#comparison-expressions) or [Nested Logical Expression](#logical-expressions).

```json
["AND", ["==", 5, 5], ["==", 10, 10]]
```

```c#
illogical.Evaluate(new object[] {
    "AND",
    new object[] { "==", 5, 5 },
    new object[] { "==", 10, 10 }
}, context); // true
```

### Or

The logical OR operator returns the bool value TRUE if either or both operands is TRUE and returns FALSE otherwise.

Expression format: `["OR", Left Operand 1, Right Operand 2, ... , Right Operand N]`.

> Valid operand types: [Comparison Expression](#comparison-expressions) or [Nested Logical Expression](#logical-expressions).

```json
["OR", ["==", 5, 5], ["==", 10, 5]]
```

```c#
illogical.Evaluate(new object[] {
    "OR",
    new object[] { "==", 5, 5 },
    new object[] { "==", 10, 5 }
}, context); // true
```

### Nor

The logical NOR operator returns the bool value TRUE if both operands are FALSE and returns FALSE otherwise.

Expression format: `["NOR", Left Operand 1, Right Operand 2, ... , Right Operand N]`

> Valid operand types: [Comparison Expression](#comparison-expressions) or [Nested Logical Expression](#logical-expressions).

```json
["NOR", ["==", 5, 1], ["==", 10, 5]]
```

```c#
illogical.Evaluate(new object[] {
    "NOR",
    new object[] { "==", 5, 1 },
    new object[] { "==", 10, 5 }
}, context); // true
```

### Xor

The logical NOR operator returns the bool value TRUE if both operands are FALSE and returns FALSE otherwise.

Expression format: `["XOR", Left Operand 1, Right Operand 2, ... , Right Operand N]`

> Valid operand types: [Comparison Expression](#comparison-expressions) or [Nested Logical Expression](#logical-expressions).

```json
["XOR", ["==", 5, 5], ["==", 10, 5]]
```

```c#
illogical.Evaluate(new object[] {
    "XOR",
    new object[] { "==", 5, 5 },
    new object[] { "==", 10, 5 }
}, context); // true
```

```json
["XOR", ["==", 5, 5], ["==", 10, 10]]
```

```c#
illogical.Evaluate(new object[] {
    "XOR",
    new object[] { "==", 5, 5 },
    new object[] { "==", 10, 10 }
}, context); // false
```

### Not

The logical NOT operator returns the bool value TRUE if the operand is FALSE, TRUE otherwise.

Expression format: `["NOT", Operand]`

> Valid operand types: [Comparison Expression](#comparison-expressions) or [Nested Logical Expression](#logical-expressions).

```json
["NOT", ["==", 5, 5]]
```

```c#
illogical.Evaluate(new object[] { "NOT", new object[] { "==", 5, 5 } }, context); // true
```

# Engine Options

## Reference Serialize Options

**Usage**

```c#
class SerializeOptions : ISerializeOptions
{
    public string? From(string operand) =>
        operand.Length > 2 && operand.StartsWith("__")
            ? operand.Substring(2)
            : null;

    public string To(string operand) => $"__{operand}";
}

var illogical = new Illogical(serializeOptions: new SerializeOptions());
```

### From

A function used to determine if the operand is a reference type, if so, return a raw value operand value

**Return value:**

- `string` = reference type
- `null` = value type

**Default reference predicate:**

> The `$` symbol at the begging of the operand is used to predicate the reference type., E.g. `$State`, `$Country`.

### To

A function used to transform the operand into the reference annotation stripped form. I.e. remove any annotation used to detect the reference type. E.g. "$Reference" => "Reference".

> **Default reference transform:**
> It removes the `$` symbol at the begging of the operand name.

## Collection Serialize Options

**Usage**

```c#
var illogical = new Illogical(escapeCharacter: '*');
```

### Escape Character

Charter used to escape fist value within a collection, if the value contains operator value.

**Example**
- `["==", 1, 1]` // interpreted as EQ expression
- `["\==", 1, 1]` // interpreted as a collection

> **Default escape character:**
> `\`

## Simplify Options

Options applied while an expression is being simplified.

**Usage**

```c#
var illogical = new Illogical(simplifyOptions: new SimplifyOptions(
    new string[] { "ignored" }, new Regex[] { new Regex(@"^ignored") }
));
```

### Ignored Paths

Reference paths which should be ignored while simplification is applied. Must be an exact match.

### Ignored Paths RegEx

Reference paths which should be ignored while simplification is applied. Matching regular expression patterns.

## Operator Mapping

Mapping of the operators. The key is unique operator key, and the value is the key used to represent the given operator in the raw expression.

**Usage**

```c#
var operatorMapping = new Dictionary<Operator, string>(Parser.DEFAULT_OPERATOR_MAPPING);
operatorMapping[Operator.EQ] = "IS";

var illogical = new Illogical(operatorMapping: operatorMapping);
```

**Default operator mapping:**

```c#
var DEFAULT_OPERATOR_MAPPING = new Dictionary<Operator, string> {
    // Logical
    { Operator.AND, "AND" },
    { Operator.OR, "OR" },
    { Operator.NOR, "NOR" },
    { Operator.XOR, "XOR" },
    { Operator.NOT, "NOT" },
    // Comparison
    { Operator.EQ, "==" },
    { Operator.NE, "!=" },
    { Operator.GT, ">" },
    { Operator.GE, ">=" },
    { Operator.LT, "<" },
    { Operator.LE, "<=" },
    { Operator.NONE, "NONE" },
    { Operator.PRESENT, "PRESENT" },
    { Operator.IN, "IN" },
    { Operator.NOTIN, "NOT IN" },
    { Operator.OVERLAP, "OVERLAP" },
    { Operator.PREFIX, "PREFIX" },
    { Operator.SUFFIX, "SUFFIX" },
};
```

---

# Contributing

See [contributing.md](https://github.com/spaceavocado/pyillogical/blob/master/contributing.md).

# License

Illogical is released under the MIT license. See [license.md](https://github.com/spaceavocado/pyillogical/blob/master/license.md).