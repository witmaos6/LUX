using System.Collections.Generic;

public sealed class EndingContext
{
    private readonly HashSet<string> factIds;

    public EndingContext(IEnumerable<string> recordedFactIds)
    {
        factIds = recordedFactIds != null
            ? new HashSet<string>(recordedFactIds)
            : new HashSet<string>();
    }

    public bool HasFact(EndingFact fact)
    {
        return fact != null && !string.IsNullOrEmpty(fact.Id) && factIds.Contains(fact.Id);
    }
}
