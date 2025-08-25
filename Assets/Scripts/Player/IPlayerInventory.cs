using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

public interface IPlayerInventory
{
    BaseGun GetSelectedGun { get; }
    
    IReadOnlyList<BaseToolItem> Tools { get; }
    int SelectedIndex { get; }
    bool IsWeaponSelected { get; }

    event Action OnInventoryChanged;
    event Action<int> OnSelectionChanged;

    bool TryAddTool(BaseToolItem tool);
    bool RemoveTool(BaseToolItem tool);
    void SelectIndex(int index);
    void SelectNext();
    void SelectPrev();

    BaseToolItem GetSelectedTool();
}
