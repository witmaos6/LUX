using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EndingFactRecorder : MonoBehaviour
{
    [Serializable]
    private sealed class Mapping
    {
        public GameEvent gameEvent = null;
        public EndingFact endingFact = null;
    }

    [SerializeField] private Mapping[] mappings = Array.Empty<Mapping>();
    private readonly List<(GameEvent gameEvent, Action handler)> subscriptions = new();

    private void OnEnable()
    {
        foreach (Mapping mapping in mappings)
        {
            if (mapping == null || mapping.gameEvent == null || mapping.endingFact == null)
                continue;

            EndingFact fact = mapping.endingFact;
            Action handler = () => EndingProgress.Record(fact);
            GameEventManager.Subscribe(mapping.gameEvent, handler);
            subscriptions.Add((mapping.gameEvent, handler));
        }
    }

    private void OnDisable()
    {
        foreach ((GameEvent gameEvent, Action handler) in subscriptions)
            GameEventManager.Unsubscribe(gameEvent, handler);

        subscriptions.Clear();
    }
}
