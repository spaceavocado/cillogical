using Cillogical.Kernel;
using Cillogical.Kernel.Operand;

namespace Cillogical;

public class Illogical
{
    private Parser parser;
    public Illogical(
        Dictionary<Operator, string>? operatorMapping = null,
        ISerializeOptions? serializeOptions = null,
        ISimplifyOptions? simplifyOptions = null,
        char? escapeCharacter = null
    ) {
        parser = new Parser(operatorMapping, serializeOptions, simplifyOptions, escapeCharacter);
    }

    public IEvaluable Parse(object[] expression) => parser.Parse(expression);
    public object? Evaluable(object[] expression, Dictionary<string, object>? context)
        => parser.Parse(expression).Evaluate(ContextUtils.FlattenContext(context));
    public object? Simplify(object[] expression, Dictionary<string, object>? context)
        => parser.Parse(expression).Simplify(ContextUtils.FlattenContext(context));
    public string Statement(object[] expression) => $"{parser.Parse(expression)}";
}
