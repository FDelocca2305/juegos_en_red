using System.Collections;
using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [Tooltip("4 slots: [0]=weapon, [1..3]=items")]
    [SerializeField] private InventorySlotUI[] slots = new InventorySlotUI[4];

    private IPlayerInventory _inventory;

    private void Awake()
    {
        StartCoroutine(Bind());
    }
    
    private IEnumerator Bind()
    {
        yield return ServiceLocatorUtil.WaitFor<IPlayerInventory>(svc => _inventory = svc);

        _inventory.OnInventoryChanged += Refresh;
        _inventory.OnSelectionChanged += SetSelected;

        Refresh();
        SetSelected(_inventory.SelectedIndex);
    }

    private void OnDestroy()
    {
        if (_inventory == null) return;
        _inventory.OnInventoryChanged -= Refresh;
        _inventory.OnSelectionChanged -= SetSelected;
    }

    private void SetSelected(int selectedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i]?.SetSelected(i == selectedIndex);
    }

    private void Refresh()
    {
        if (slots.Length > 0 && slots[0] != null)
        {
            slots[0].SetIcon(null);
        }
        
        for (int uiIdx = 1; uiIdx < slots.Length; uiIdx++)
        {
            var slot = slots[uiIdx];
            if (slot == null) continue;

            int itemIdx = uiIdx - 1;
            if (itemIdx >= 0 && itemIdx < _inventory.Tools.Count)
            {
                var item = _inventory.Tools[itemIdx];
                slot.SetIcon(item?.Icon);
            }
            else
            {
                slot.SetIcon(null);
            }
        }
    }
}
