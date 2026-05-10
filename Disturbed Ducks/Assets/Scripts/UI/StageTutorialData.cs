using UnityEngine;

[System.Serializable]
public class TutorialMessage
{
    [TextArea(2, 5)]
    public string message = "";
    [Tooltip("If true, only shows once per session even if the trigger fires again")]
    public bool showOnce = false;
}

[CreateAssetMenu(fileName = "StageTutorial", menuName = "Game/Stage Tutorial Data")]
public class StageTutorialData : ScriptableObject
{
    [Header("Stage Start")]
    public TutorialMessage stageStartMessage;

    [Header("On First Launch")]
    public TutorialMessage onLaunchMessage;

    [Header("On First Crash")]
    [Tooltip("Shown on first crash if the player has bought at least one upgrade")]
    public TutorialMessage onFirstCrashMessage;
    [Tooltip("Shown instead if player has no upgrades at all — use this for the upgrade hint")]
    public TutorialMessage onCrashNoUpgradesMessage;

    [Header("Purchase Hints")]
    public TutorialMessage onAbilityUnlockedMessage;
    public TutorialMessage onDuckPurchasedMessage;
}