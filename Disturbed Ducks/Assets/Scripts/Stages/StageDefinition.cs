using UnityEngine;

/// <summary>
/// Defines a single stage. Create one asset per stage.
/// Place enemies and collectibles in the scene and tag them.
/// </summary>
[CreateAssetMenu(fileName = "NewStage", menuName = "Game/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    [Header("Identity")]
    public string stageId   = "stage_1";
    public string stageName = "Stage 1";

    [Header("Objectives")]
    [Tooltip("Tag on TargetEnemy objectives in the scene (e.g. 'Beaver')")]
    public string objectiveTag = "Beaver";

    [Tooltip("Tag on Collectible objectives in the scene (e.g. 'Blueprint'). Leave empty if none.")]
    public string collectibleTag = "";

    [Header("Currency Rewards")]
    [Tooltip("Bonus currency awarded the first time this stage is cleared")]
    public int firstClearBonus = 100;

    [Header("Stage Flow")]
    [Tooltip("Next stage asset — leave null if this is the final stage")]
    public StageDefinition nextStage;
}