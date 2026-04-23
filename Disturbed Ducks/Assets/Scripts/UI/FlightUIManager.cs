using UnityEngine;
using TMPro;

/// <summary>
/// Manages in-flight UI prompts.
/// Uses a singleton so other scripts can notify it without a direct reference.
/// </summary>
public class FlightUIManager : MonoBehaviour
{
    public static FlightUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        // Simple singleton — only one UI manager should exist
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowLaunchPrompt();
    }

    // -------------------------------------------------------------------------

    public void ShowLaunchPrompt()
    {
        SetPrompt("Use WASD / Arrow Keys to launch");
    }

    public void OnLaunched()
    {
        SetPrompt(""); // clear prompt while flying
    }

    public void OnCrashed()
    {
        SetPrompt("Press R to respawn");
    }

    // -------------------------------------------------------------------------

    private void SetPrompt(string message)
    {
        if (promptText == null) return;
        promptText.text = message;
    }
}