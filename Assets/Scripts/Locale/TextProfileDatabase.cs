using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TextProfileDatabase",
    menuName = "LUX/Localization/Text Profile Database"
)]
public class TextProfileDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string key;
        public string profile;
    }

    public List<Entry> entries = new();

    private Dictionary<string, string> lookup;

    public string GetProfile(string key)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, string>();

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.key))
                    continue;

                lookup[entry.key] = entry.profile;
            }
        }

        return lookup.TryGetValue(key, out var profile) ? profile : null;
    }

    public void ReplaceEntries(List<Entry> newEntries)
    {
        entries = newEntries;
        lookup = null;
    }
}
