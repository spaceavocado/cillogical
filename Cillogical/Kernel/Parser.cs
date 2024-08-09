using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using System;

namespace Cillogical.Kernel;


public class UnexpectedExpressionInputException : Exception
{
    public UnexpectedExpressionInputException(string message) : base(message) {}
}

public class UnexpectedExpressionException : Exception
{
    public UnexpectedExpressionException(string message) : base(message) { }
}

public class UnexpectedOperandException : Exception
{
    public UnexpectedOperandException(string message) : base(message) { }
}

public enum Operator {
    AND,
    OR,
    NOR,
    XOR,
    NOT,
    EQ,
    NE,
    GT,
    GE,
    LT,
    LE,
    IN,
    NOTIN,
    OVERLAP,
    PREFIX,
    SUFFIX,
    NULL,
    PRESENT,
}

public class Parser {
    public static Dictionary<Operator, string> DEFAULT_OPERATOR_MAPPING = new Dictionary<Operator, string> {
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
        { Operator.NULL, "NULL" },
        { Operator.PRESENT, "PRESENT" },
        { Operator.IN, "IN" },
        { Operator.NOTIN, "NOT IN" },
        { Operator.OVERLAP, "OVERLAP" },
        { Operator.PREFIX, "PREFIX" },
        { Operator.SUFFIX, "SUFFIX" },
    };

    public static char DEFAULT_ESCAPE_CHARACTER = '\\';

    private Dictionary<Operator, string> operatorMapping;
    private Dictionary<Operator, Func<IEvaluable[], IEvaluable>> operatorHandlerMapping;
    private ISerializeOptions serializeOptions;
    private ISimplifyOptions? simplifyOptions;
    private char escapeCharacter;
    private HashSet<string> escapedOperators;

    public Parser(
        Dictionary<Operator, string>? operatorMapping = null,
        ISerializeOptions? serializeOptions = null,
        ISimplifyOptions? simplifyOptions = null,
        char? escapeCharacter = null
    ) {
        this.serializeOptions = this.serializeOptions ?? new DefaultSerializeOptions();
        this.simplifyOptions = simplifyOptions;
        this.escapeCharacter = escapeCharacter ?? '\\';
        this.operatorMapping = operatorMapping ?? DEFAULT_OPERATOR_MAPPING;

        Func<Operator, string> operatorSymbol = (Operator op) => this.operatorMapping[op] ?? DEFAULT_OPERATOR_MAPPING[op];

        this.operatorHandlerMapping = new Dictionary<Operator, Func<IEvaluable[], IEvaluable>> {
            // Logical
            { Operator.AND, MultiaryHandler((operands) => new And(operands, operatorSymbol(Operator.AND))) },
            { Operator.OR, MultiaryHandler((operands) => new Or(operands, operatorSymbol(Operator.OR))) },
            { Operator.NOR, MultiaryHandler((operands) => new Nor(operands, operatorSymbol(Operator.NOR), operatorSymbol(Operator.NOT))) },
            { Operator.XOR, MultiaryHandler((operands) => new Xor(operands, operatorSymbol(Operator.XOR), operatorSymbol(Operator.NOT), operatorSymbol(Operator.NOR))) },
            { Operator.NOT, UnaryHandler((operand) => new Not(operand, operatorSymbol(Operator.NOT))) },
            //// Comparison
            { Operator.EQ, BinaryHandler((left, right) => new Eq(left, right, operatorSymbol(Operator.EQ))) },
            { Operator.NE, BinaryHandler((left, right) => new Ne(left, right, operatorSymbol(Operator.NE))) },
            { Operator.GT, BinaryHandler((left, right) => new Gt(left, right, operatorSymbol(Operator.GT))) },
            { Operator.GE, BinaryHandler((left, right) => new Ge(left, right, operatorSymbol(Operator.GE))) },
            { Operator.LT, BinaryHandler((left, right) => new Lt(left, right, operatorSymbol(Operator.LT))) },
            { Operator.LE, BinaryHandler((left, right) => new Le(left, right, operatorSymbol(Operator.LE))) },
            { Operator.NULL, UnaryHandler((operand) => new Null(operand, operatorSymbol(Operator.NULL))) },
            { Operator.PRESENT, UnaryHandler((operand) => new Present(operand, operatorSymbol(Operator.PRESENT))) },
            { Operator.IN, BinaryHandler((left, right) => new In(left, right, operatorSymbol(Operator.IN))) },
            { Operator.NOTIN, BinaryHandler((left, right) => new NotIn(left, right, operatorSymbol(Operator.NOTIN))) },
            { Operator.OVERLAP, BinaryHandler((left, right) => new Overlap(left, right, operatorSymbol(Operator.OVERLAP))) },
            { Operator.PREFIX, BinaryHandler((left, right) => new Prefix(left, right, operatorSymbol(Operator.PREFIX))) },
            { Operator.SUFFIX, BinaryHandler((left, right) => new Suffix(left, right, operatorSymbol(Operator.SUFFIX))) },
        };
        this.escapedOperators = new HashSet<string>(this.operatorMapping.Values);
}

    public IEvaluable Parse(object? input)
    {
        if (input == null) {
            throw new UnexpectedExpressionInputException("input cannot be null");
        };

        if (input is not IEnumerable<object>) {
            return CreateOperand(input);
        }

        var expression = ((IEnumerable<object>)input).ToArray();

        if (expression.Count() < 2) {
            return CreateOperand(expression);
        }

        if (expression[0] is string && IsEspaced((string)expression[0])) {
            return CreateOperand(new object[] { ((string)expression[0]).Substring(1) }.Concat(expression.Skip(1)));
        }

        try {
            return CreateExpression(expression);
        }
        catch (UnexpectedExpressionException) {
            return CreateOperand(expression);
        }
    }

    private Func<IEvaluable[], IEvaluable> UnaryHandler(Func<IEvaluable, IEvaluable> handler) =>
        (IEvaluable[] operands) => handler(operands[0]);

    private Func<IEvaluable[], IEvaluable> BinaryHandler(Func<IEvaluable, IEvaluable, IEvaluable> handler) =>
        (IEvaluable[] operands) => handler(operands[0], operands[1]);

    private Func<IEvaluable[], IEvaluable> MultiaryHandler(Func<IEvaluable[], IEvaluable> handler) =>
        (IEvaluable[] operands) => handler(operands);

    private bool IsEspaced(string value) =>
        value.StartsWith(escapeCharacter);

    private string? ToReferenceAddress(object? reference) =>
        reference is string ? serializeOptions.From((string)reference) : null;

    private IEvaluable CreateOperand(object? value)
    {
        if (value is IEnumerable<object>) {
            if (((IEnumerable<object>)value).Count() == 0) {
                throw new UnexpectedOperandException("collection operand must have items");
            }

            return new Collection(
                (from operand in (IEnumerable<object>)value select Parse(operand)).ToArray(),
                escapeCharacter,
                escapedOperators
            );
        }

        var address = ToReferenceAddress(value);
        if (address != null) {
            return new Reference(address, serializeOptions, simplifyOptions);
        }

        if (value != null && !Primitive.IsPrimitive(value)) {
            throw new UnexpectedOperandException("value operand must be a primitive value, number, text, bool and/or null");
        }

        return new Value(value);
    }

    private IEvaluable CreateExpression(object[] expression)
    {
        var op = expression[0];
        var operands = expression.Skip(1).ToArray();

        if (op is not string) {
            throw new UnexpectedExpressionException($"expression must have a valid operator, got {op}");
        }

        Operator opKey;
        try {
            opKey = (Operator)Enum.Parse(typeof(Operator), (string)op);
        } catch {
            throw new UnexpectedExpressionException($"expression got unsupported opereator, got {op}");
        }

        if (!operatorHandlerMapping.ContainsKey(opKey)) {
            throw new UnexpectedExpressionException($"missing expression handler for {op}");
        }

        return operatorHandlerMapping[opKey]((from operand in operands select Parse(operand)).ToArray());
    }
}