using System.Collections.Generic;
using UnityEngine;

public static class EndingProgress
{
    public static void Record(EndingFact fact)
    {
        if (fact == null)
            return;

        if (string.IsNullOrEmpty(fact.Id))
        {
            Debug.LogError($"Ending fact '{fact.name}' has no stable ID.", fact);
            return;
        }

        SaveManager.RecordEndingFact(fact.Id);
    }

    public static bool Contains(EndingFact fact)
    {
        return fact != null && SaveManager.HasEndingFact(fact.Id);
    }

    public static HashSet<string> CreateSnapshot()
    {
        return SaveManager.CreateEndingFactSnapshot();
    }
}
