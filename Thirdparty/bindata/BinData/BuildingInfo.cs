using System.Linq.Expressions;

namespace BinData;

internal record BuildingInfo(List<Expression> Expressions, List<ParameterExpression> Variables, Type Type, Expression Value, ParameterExpression Stream)
{
    public bool IsTopLevel { get; init; } = true;

    private Dictionary<Type, Stack<ParameterExpression>>? variablesCache;

    public void Add(Expression expression)
    {
        Expressions.Add(expression);
    }

    public ParameterExpression GetVariable(Type type)
    {
        if (variablesCache is not null && variablesCache.TryGetValue(type, out Stack<ParameterExpression>? cache))
        {
            if (cache.TryPop(out ParameterExpression? cachedVariable))
                return cachedVariable;
        }

        ParameterExpression variable = Expression.Variable(type);
        Variables.Add(variable);
        return variable;
    }

    public void DiscardVariable(ParameterExpression variable)
    {
        variablesCache ??= new();
        if (!variablesCache.TryGetValue(variable.Type, out Stack<ParameterExpression>? cache))
        {
            cache = new Stack<ParameterExpression>();
            variablesCache[variable.Type] = cache;
        }
        cache.Push(variable);
    }
}
