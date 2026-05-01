using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LoadoutUI : MonoBehaviour
{
    public static LoadoutUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject loadoutPanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Appearance")]
    [SerializeField] private Color selectedColor   = new Color(1f, 0.85f, 0f);
    [SerializeField] private Color unselectedColor = Color.white;
    [SerializeField] private Color emptyColor      = new Color(1f, 1f, 1f, 0.3f);

    private List<LoadoutSlot> _slots = new List<LoadoutSlot>();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    private void Start()
    {
        if (PlayerDuckInventory.Instance != null)
        {
            PlayerDuckInventory.Instance.OnInventoryChanged    += Refresh;
            PlayerDuckInventory.Instance.OnSelectedTypeChanged += _ => Refresh();
        }

        BuildSlots();
    }

    // -------------------------------------------------------------------------

    public void Show()
    {
        if (loadoutPanel == null) return;
        loadoutPanel.SetActive(true);

        if (_slots.Count == 0)
            BuildSlots();
        else
            Refresh();
    }

    public void Hide()
    {
        if (loadoutPanel == null) return;
        loadoutPanel.SetActive(false);
    }

    public void RebuildAndShow()
    {
        _isBuilt = false;
        if (slotContainer != null)
            foreach (Transform child in slotContainer)
                DestroyImmediate(child.gameObject); // ← immediate, not deferred

        _slots.Clear();
        BuildSlots();

        if (loadoutPanel != null)
            loadoutPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    private bool _isBuilt = false;
    private void BuildSlots()
    {
        if (_isBuilt) return;
        _isBuilt = true;
        if (PlayerDuckInventory.Instance == null)
        {
            Debug.LogError("LoadoutUI BuildSlots: PlayerDuckInventory null");
            return;
        }
        if (slotPrefab == null)
        {
            Debug.LogError("LoadoutUI BuildSlots: slotPrefab null");
            return;
        }
        if (slotContainer == null)
        {
            Debug.LogError("LoadoutUI BuildSlots: slotContainer null");
            return;
        }

        foreach (var def in PlayerDuckInventory.Instance.GetOwnedTypes())
        {
            var slotObj = Instantiate(slotPrefab, slotContainer);
            var slot    = slotObj.GetComponent<LoadoutSlot>();
            if (slot == null) continue;
            slot.Initialize(def);
            _slots.Add(slot);
        }

        Refresh();
    }

    private void Refresh()
    {
        if (PlayerDuckInventory.Instance == null) return;
        var selected = PlayerDuckInventory.Instance.SelectedType;

        foreach (var slot in _slots)
        {
            if (slot == null) continue;
            int  remaining  = PlayerDuckInventory.Instance.GetRemaining(slot.Definition);
            bool isSelected = slot.Definition == selected;
            slot.UpdateDisplay(remaining, isSelected,
                selectedColor, unselectedColor, emptyColor);
        }
    }
    private void OnDestroy()
    {
        if (PlayerDuckInventory.Instance != null)
        {
            PlayerDuckInventory.Instance.OnInventoryChanged    -= Refresh;
            PlayerDuckInventory.Instance.OnSelectedTypeChanged -= _ => Refresh();
        }
    }
}