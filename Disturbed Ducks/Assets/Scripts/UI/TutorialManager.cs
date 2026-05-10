using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float           displayDuration = 6f;
    [SerializeField] private StageTutorialData[] stageTutorials;

    private StageTutorialData    _currentData;
    private HashSet<string>      _shownThisSession = new HashSet<string>();
    private Coroutine            _hideCoroutine;
    private bool                 _hasLaunched;
    private bool                 _hasCrashed;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        StageManager.OnStageInitialized      += HandleStageInitialized;
        UpgradeManager.OnAbilityFirstUnlocked += HandleAbilityUnlocked;
        PlayerDuckInventory.OnDuckPurchased   += HandleDuckPurchased;
    }

    private void OnDisable()
    {
        StageManager.OnStageInitialized      -= HandleStageInitialized;
        UpgradeManager.OnAbilityFirstUnlocked -= HandleAbilityUnlocked;
        PlayerDuckInventory.OnDuckPurchased   -= HandleDuckPurchased;
    }

    // -------------------------------------------------------------------------

    private void HandleStageInitialized(int stageIndex)
    {
        _currentData  = stageIndex < stageTutorials.Length ? stageTutorials[stageIndex] : null;
        _hasLaunched  = false;
        _hasCrashed   = false;

        ShowMessage(_currentData?.stageStartMessage);
    }

    public void OnLaunched()
    {
        if (_hasLaunched) return;
        _hasLaunched = true;
        ShowMessage(_currentData?.onLaunchMessage);
    }

    public void OnCrashed()
    {
        if (_hasCrashed) return;
        _hasCrashed = true;

        bool noUpgrades = UpgradeManager.Instance != null
            && UpgradeManager.Instance.SpeedLevel   == 0
            && UpgradeManager.Instance.ManeurLevel  == 0
            && UpgradeManager.Instance.AbilityLevel == 0;

        if (noUpgrades && _currentData?.onCrashNoUpgradesMessage != null
            && !string.IsNullOrEmpty(_currentData.onCrashNoUpgradesMessage.message))
            ShowMessage(_currentData.onCrashNoUpgradesMessage);
        else
            ShowMessage(_currentData?.onFirstCrashMessage);
    }

    private void HandleAbilityUnlocked()  => ShowMessage(_currentData?.onAbilityUnlockedMessage);
    private void HandleDuckPurchased()    => ShowMessage(_currentData?.onDuckPurchasedMessage);

    // -------------------------------------------------------------------------

    private void ShowMessage(TutorialMessage msg)
    {
        if (msg == null || string.IsNullOrEmpty(msg.message)) return;

        if (msg.showOnce)
        {
            if (_shownThisSession.Contains(msg.message)) return;
            _shownThisSession.Add(msg.message);
        }

        if (tutorialText != null) tutorialText.text = msg.message;

        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay(displayDuration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tutorialText != null) tutorialText.text = "";
    }
}