using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IPlayerInventory
{
    [Header("Weapon (slot 0)")]
    [SerializeField] private BaseGun weapon;

    [Header("Tools (slots 1..3)")]
    [SerializeField] private List<BaseToolItem> tools = new List<BaseToolItem>(3);

    [Header("Selection")]
    [SerializeField, Range(0,3)] private int selectedIndex = 0;

    public BaseGun GetSelectedGun => weapon;
    public IReadOnlyList<BaseToolItem> Tools => tools;
    public int SelectedIndex => selectedIndex;
    public bool IsWeaponSelected => selectedIndex == 0;

    public event Action OnInventoryChanged;
    public event Action<int> OnSelectionChanged;

    private void Awake()
    {
        tools.RemoveAll(t => t == null);
        ApplySelectionVisuals(previousIndex: -1);
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public bool TryAddTool(BaseToolItem tool)
    {
        if (tool == null || tools.Count >= 3) return false;
        tools.Add(tool);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveTool(BaseToolItem tool)
    {
        if (tool == null) return false;
        if (!tools.Remove(tool)) return false;
        OnInventoryChanged?.Invoke();
        if (!IsWeaponSelected) SelectIndex(Mathf.Clamp(selectedIndex, 0, Math.Min(3, tools.Count)));
        return true;
    }

    public void SelectIndex(int index)
    {
        int maxIndex = Math.Min(3, tools.Count);
        index = Mathf.Clamp(index, 0, maxIndex);
        if (selectedIndex == index) return;

        int prev = selectedIndex;
        selectedIndex = index;
        ApplySelectionVisuals(prev);
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public void SelectNext()
    {
        int maxIndex = Math.Min(3, tools.Count);
        int prev = selectedIndex;
        selectedIndex = (selectedIndex + 1) % (maxIndex + 1);
        ApplySelectionVisuals(prev);
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public void SelectPrev()
    {
        int maxIndex = Math.Min(3, tools.Count);
        int prev = selectedIndex;
        selectedIndex = (selectedIndex - 1 + (maxIndex + 1)) % (maxIndex + 1);
        ApplySelectionVisuals(prev);
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public BaseToolItem GetSelectedTool()
    {
        if (IsWeaponSelected) return null;
        int idx = selectedIndex - 1;
        return (idx >= 0 && idx < tools.Count) ? tools[idx] : null;
    }

    private void ApplySelectionVisuals(int previousIndex)
    {
        if (previousIndex > 0)
        {
            int prevToolIdx = previousIndex - 1;
            if (prevToolIdx >= 0 && prevToolIdx < tools.Count)
                tools[prevToolIdx]?.OnDeselected();
        }
        if (weapon) weapon.gameObject.SetActive(IsWeaponSelected);
        
        if (!IsWeaponSelected)
        {
            var tool = GetSelectedTool();
            tool?.OnSelected();
            if (weapon) weapon.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f) SelectNext();
        else if (scroll < 0f) SelectPrev();

        for (int i = 1; i <= 4; i++)
        {
            if (Input.GetKeyDown((i).ToString()))
                SelectIndex(i - 1);
        }
    }
}
