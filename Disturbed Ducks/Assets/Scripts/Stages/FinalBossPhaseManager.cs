using UnityEngine;

public class FinalBossPhaseManager : MonoBehaviour
{
    public static event System.Action<int, int> OnFinalObjectivesChanged;

    [Header("Boss Fight")]
    [SerializeField] private MechaHuskyBoss boss;
    [SerializeField] private BossGenerator[] generators;
    [SerializeField] private FinalHeroRescueObjective heroRescue;

    [Header("Final Duck")]
    [SerializeField] private DuckDefinition heroKingDuckDefinition;
    [SerializeField] private DuckController duckController;
    [SerializeField] private DuckImpact duckImpact;
    [SerializeField] private LauncherController launcherController;

    [Header("UI")]
    [SerializeField] [TextArea(2, 5)]
    private string finalSacrificeTutorialMessage = "Aim the King Duck at the Mecha Husky and launch him into the core.";
    [SerializeField] private FinalStageMessageUI messageUI;

    private bool _partTwoStarted;
    private bool _bossObjectiveComplete;
    private bool _heroObjectiveComplete;

    public bool PartTwoStarted => _partTwoStarted;
    public DuckDefinition HeroKingDuckDefinition => heroKingDuckDefinition;

    private void Start()
    {
        foreach (var generator in generators)
        {
            if (generator != null)
                generator.OnDestroyed += HandleGeneratorDestroyed;
        }

        if (boss != null)
        {
            boss.SetVulnerable(false);
            boss.OnPhaseOneDefeated += HandleBossPhaseOneDefeated;
        }

        if (heroRescue != null)
            heroRescue.OnRescued += HandleHeroRescued;

        PublishObjectiveProgress();
    }

    private void HandleGeneratorDestroyed(BossGenerator generator)
    {
        if (AllGeneratorsDestroyed())
            boss?.SetVulnerable(true);

        PublishObjectiveProgress();
    }

    private void HandleHeroRescued()
    {
        _heroObjectiveComplete = true;
        PublishObjectiveProgress();

        TryShowFinalSacrificePrompt();
    }

    private bool AllGeneratorsDestroyed()
    {
        foreach (var generator in generators)
        {
            if (generator != null && !generator.IsDestroyed)
                return false;
        }

        return true;
    }

    private void HandleBossPhaseOneDefeated()
    {
        _bossObjectiveComplete = true;
        PublishObjectiveProgress();

        TryShowFinalSacrificePrompt();
    }

    private void StartFinalSacrificePhase()
    {
        _partTwoStarted = true;

        if (duckController != null && heroKingDuckDefinition != null)
            duckController.ApplyDefinitionFromType(heroKingDuckDefinition);

        UpgradeManager.Instance?.ApplyCurrentStats();

        launcherController?.ResetToLauncher();
        duckImpact?.Reset();
        FlightUIManager.Instance?.ShowFinalHeroPrompt();
        TutorialManager.Instance?.ShowCustomTutorialText(finalSacrificeTutorialMessage);
    }

    private void PublishObjectiveProgress()
    {
        int total = 0;
        int complete = 0;

        if (generators != null)
        {
            foreach (var generator in generators)
            {
                if (generator == null) continue;

                total++;

                if (generator.IsDestroyed)
                    complete++;
            }
        }

        if (heroRescue != null)
        {
            total++;

            if (_heroObjectiveComplete || heroRescue.IsRescued)
                complete++;
        }

        if (boss != null)
        {
            total++;

            if (_bossObjectiveComplete || boss.PhaseOneDefeated)
                complete++;
        }

        OnFinalObjectivesChanged?.Invoke(total - complete, total);
    }
    private void TryShowFinalSacrificePrompt()
    {
        if (!_bossObjectiveComplete)
            return;

        bool heroRescued = _heroObjectiveComplete || (heroRescue != null && heroRescue.IsRescued);
        if (!heroRescued)
            return;

        messageUI?.Show(
            "The Mecha Husky is down, but the machine is still holding together. The rescued king duck steps forward and tells the other ducks to get away.",
            "Finish This",
            StartFinalSacrificePhase
        );
    }
}