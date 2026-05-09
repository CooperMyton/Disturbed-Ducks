using UnityEngine;

[CreateAssetMenu(fileName = "NewStage", menuName = "Game/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    [Header("Identity")]
    public string stageId   = "stage_1";
    public string stageName = "Stage 1";

    [Header("Objectives")]
    [Tooltip("Tags on TargetEnemy objectives — add as many as you need")]
    public string[] objectiveTags = { "Beaver" };

    [Tooltip("Tags on Collectible objectives — add as many as you need, leave empty if none")]
    public string[] collectibleTags = {};

    [Header("Currency Rewards")]
    public int firstClearBonus = 100;

    [Header("Stage Flow")]
    public StageDefinition nextStage;
}