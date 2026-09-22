using System;
using System.Collections.Generic;
using UnityEngine;

public enum ClueItemCode
{
    None = 0,
    ObservationExperimentLog = 1,
    AnimalResponseExperimentLog = 2,
    WitnessedTheDivision = 3,
    FissionPhenomenonHypothesis = 4,
    ReportForExperiment15 = 5,
    ReportForExperiment13 = 6,
    MentemExperimentReport = 7,
    ServusExperimentReport = 8,
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
}
