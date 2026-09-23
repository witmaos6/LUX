using System;
using System.Collections.Generic;
using UnityEngine;

public enum ClueItemCode
{
    None = 0,
    InitialExperimentLog = 1,
    WitnessedTheDivision = 2,
    FissionPhenomenonHypothesis = 3,
    ReportForExperiment15 = 4,
    ReportForExperiment13 = 5,
    MentemExperimentReport = 6,
    ServusExperimentReport = 7,
}

[CreateAssetMenu(
    fileName = "ClueInventoryItemDatabase",
    menuName = "LUX2D/Clue Inventory Item Database")]
public sealed class ClueInventoryItemDatabase : ScriptableObject
{
    [Serializable]
    public class ClueItemDefinition
    {
        public ClueItemCode clueItemCode;
        public string displayName;
        public GameObject uiPrefab;
    }

    [SerializeField] private List<ClueItemDefinition> items = new();

    public string GetDisplayName(ClueItemCode clueItemCode)
    {
        ClueItemDefinition clueItemDefinition = items.Find(definition => definition.clueItemCode == clueItemCode);
        if(clueItemDefinition != null && !string.IsNullOrWhiteSpace(clueItemDefinition.displayName))
        {
            return clueItemDefinition.displayName;
        }
        return clueItemDefinition != null ? clueItemDefinition.displayName : clueItemDefinition.clueItemCode.ToString();
    }

    public GameObject GetUI(ClueItemCode clueItemCode)
    {
        ClueItemDefinition clueItemDefinition = items.Find(definition => definition.clueItemCode == clueItemCode);

        return clueItemDefinition != null ? clueItemDefinition.uiPrefab : null;
    }
}
