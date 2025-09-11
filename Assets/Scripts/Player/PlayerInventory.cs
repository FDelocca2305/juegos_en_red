using System;
using System.Collections.Generic;
using Photon.Pun;
using Player;
using UnityEngine;

public class PlayerInventory : MonoBehaviourPunCallbacks, IPlayerInventory
{
    [Header("Weapon (slot 0)")]
    [SerializeField] private BaseGun weapon;

    [Header("Tools (slots 1..3)")]
    [SerializeField] private List<BaseToolItem> tools = new List<BaseToolItem>(3);

    [Header("Selection")]
    [SerializeField, Range(0, 3)] private int selectedIndex = 0;

    public BaseGun GetSelectedGun => weapon;
    public IReadOnlyList<BaseToolItem> Tools => tools;
    public int SelectedIndex => selectedIndex;
    public bool IsWeaponSelected => selectedIndex == 0 && weapon != null;

    public event Action OnInventoryChanged;
    public event Action<int> OnSelectionChanged;

    public override void OnEnable()
    {
        if (photonView.IsMine)
            ServiceLocator.Register<IPlayerInventory>(this);
    }

    public override void OnDisable()
    {
        if (photonView.IsMine)
            ServiceLocator.Deregister<IPlayerInventory>(this);
    }

    private void Awake()
    {
        tools.RemoveAll(t => t == null);

        if (weapon == null && selectedIndex == 0 && tools.Count > 0)
            selectedIndex = 1;

        ApplySelectionVisuals(previousIndex: -1);
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    private int ToolCountClamped() => Math.Min(3, tools.Count);
    private int TotalSlots() => (weapon != null ? 1 : 0) + ToolCountClamped();

    private (int min, int max) Bounds()
    {
        int tc = ToolCountClamped();
        if (weapon != null)
        {
            // 0 (weapon) .. tc (último tool)
            return (0, tc);
        }
        else
        {
            // 1 .. tc (solo tools)
            return (tc > 0 ? 1 : 0, tc);
        }
    }

    public bool TryAddTool(BaseToolItem tool)
    {
        if (tool == null) return false;
        if (tools.Contains(tool)) return false;
        if (tools.Count >= 3) return false;

        tools.Add(tool);
        OnInventoryChanged?.Invoke();

        if (weapon == null && selectedIndex == 0) SelectIndex(1);
        return true;
    }

    public bool RemoveTool(BaseToolItem tool)
    {
        if (tool == null) return false;
        if (!tools.Remove(tool)) return false;

        OnInventoryChanged?.Invoke();

        var (min, max) = Bounds();
        if (selectedIndex < min || selectedIndex > max)
            SelectIndex(Mathf.Clamp(selectedIndex, min, max));
        else
            OnSelectionChanged?.Invoke(selectedIndex);

        return true;
    }

    public void SelectIndex(int index)
    {
        var (min, max) = Bounds();
        if (min == 0 && max == 0) return; // sin slots

        index = Mathf.Clamp(index, min, max);

        if (selectedIndex == index) return;
        int prev = selectedIndex;
        selectedIndex = index;
        ApplySelectionVisuals(prev);
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public void SelectNext()
    {
        int tc = ToolCountClamped();
        int total = TotalSlots();
        if (total <= 0) return;

        int prev = selectedIndex;

        if (weapon != null)
        {
            // índices válidos: 0..tc
            selectedIndex = (selectedIndex + 1) % (tc + 1);
        }
        else
        {
            // índices válidos: 1..tc
            if (tc <= 0) return;
            int currentToolIdx = Mathf.Clamp(selectedIndex - 1, 0, tc - 1); // 0..tc-1
            currentToolIdx = (currentToolIdx + 1) % tc;
            selectedIndex = currentToolIdx + 1; // 1..tc
        }

        ApplySelectionVisuals(prev);
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    public void SelectPrev()
    {
        int tc = ToolCountClamped();
        int total = TotalSlots();
        if (total <= 0) return;

        int prev = selectedIndex;

        if (weapon != null)
        {
            // 0..tc
            selectedIndex = (selectedIndex - 1 + (tc + 1)) % (tc + 1);
        }
        else
        {
            if (tc <= 0) return;
            int currentToolIdx = Mathf.Clamp(selectedIndex - 1, 0, tc - 1); // 0..tc-1
            currentToolIdx = (currentToolIdx - 1 + tc) % tc;
            selectedIndex = currentToolIdx + 1; // 1..tc
        }

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
        // Apagar tool previa si corresponde
        if (previousIndex > 0)
        {
            int prevToolIdx = previousIndex - 1;
            if (prevToolIdx >= 0 && prevToolIdx < tools.Count)
            {
                var prevTool = tools[prevToolIdx];
                prevTool?.OnDeselected();
                if (prevTool) prevTool.gameObject.SetActive(false);
            }
        }

        // Arma visible si corresponde
        if (weapon) weapon.gameObject.SetActive(IsWeaponSelected);

        // Tools
        if (!IsWeaponSelected)
        {
            for (int i = 0; i < tools.Count; i++)
                if (tools[i]) tools[i].gameObject.SetActive(false);

            var tool = GetSelectedTool();
            if (tool) tool.gameObject.SetActive(true);
            tool?.OnSelected();

            if (weapon) weapon.gameObject.SetActive(false);
        }
    }

    public void SetWeapon(BaseGun newWeapon)
    {
        if (weapon == newWeapon) return;

        if (weapon) weapon.gameObject.SetActive(false);
        weapon = newWeapon;

        if (weapon == null && selectedIndex == 0 && tools.Count > 0)
            selectedIndex = 1;

        ApplySelectionVisuals(previousIndex: -1);
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f) SelectNext();
        else if (scroll < 0f) SelectPrev();

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectIndex(0); // arma (si hay)
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectIndex(1); // tool 1
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectIndex(2); // tool 2
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectIndex(3); // tool 3
    }

    public void ClearTools()
    {
        foreach (var t in tools) t?.OnDeselected();

        tools.Clear();

        if (weapon == null && selectedIndex == 0) selectedIndex = 1;

        ApplySelectionVisuals(previousIndex: -1);
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke(selectedIndex);
    }
}
