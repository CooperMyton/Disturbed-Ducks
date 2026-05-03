using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One slot in the loadout hotbar.
/// Shows duck name, remaining count, highlights when selected.
/// </summary>
public class LoadoutSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image background;
    [SerializeField] private Button button;

    // Serialized so you can tweak per-slot in the prefab if needed.
    // Default is near-black — readable on yellow, white, and grey backgrounds.
    [SerializeField] private Color countTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);

    public DuckDefinition Definition { get; private set; }

    public void Initialize(DuckDefinition def)
    {
        Definition = def;
        if (nameText  != null) nameText.text  = def.duckName;
        if (countText != null) countText.color = countTextColor;
        button?.onClick.AddListener(() =>
            PlayerDuckInventory.Instance?.SelectType(def));
    }

    public void UpdateDisplay(int remaining, bool selected,
        Color selectedColor, Color unselectedColor, Color emptyColor)
    {
        if (countText != null) countText.text = remaining.ToString();

        if (background != null)
        {
            if (remaining <= 0)
                background.color = emptyColor;
            else
                background.color = selected ? selectedColor : unselectedColor;
        }

        if (button != null)
            button.interactable = remaining > 0;
    }
}