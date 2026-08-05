using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EndingSequence", menuName = "LUX2D/Ending/Sequence")]
public sealed class EndingSequence : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        [SerializeField] private EndingFact[] requiredFacts = Array.Empty<EndingFact>();
        [SerializeField] private EndingFact[] excludedFacts = Array.Empty<EndingFact>();
        [SerializeField] private EndingStep step;

        public EndingStep Step => step;

        public bool IsSatisfiedBy(EndingContext context)
        {
            if (context == null)
                return false;

            foreach (EndingFact fact in requiredFacts)
            {
                if (fact == null || !context.HasFact(fact))
                    return false;
            }

            foreach (EndingFact fact in excludedFacts)
            {
                if (fact != null && context.HasFact(fact))
                    return false;
            }

            return true;
        }
    }

    [SerializeField] private Entry[] entries = Array.Empty<Entry>();
    public IReadOnlyList<Entry> Entries => entries;
}
