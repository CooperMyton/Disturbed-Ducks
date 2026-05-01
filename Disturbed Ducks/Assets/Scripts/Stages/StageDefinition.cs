using UnityEngine;

/// <summary>
/// Defines a single stage. Create one asset per stage.
/// Place enemies in the scene and tag them — no direct references needed.
/// </summary>
[CreateAssetMenu(fileName = "NewStage", menuName = "Game/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    [Header("Identity")]
    public string stageId = "stage_1";
    public string stageName = "Stage 1";

    [Header("Objectives")]
    [Tooltip("Tag used on objective enemies in the scene (e.g. 'Beaver')")]
    public string objectiveTag = "Beaver";

    [Header("Currency Rewards")]
    [Tooltip("Bonus currency awarded the first time this stage is cleared")]
    public int firstClearBonus = 100;

    [Header("Stage Flow")]
    [Tooltip("ID of the next stage asset to load after this one is cleared")]
    public StageDefinition nextStage;
}