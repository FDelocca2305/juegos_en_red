using Photon.Pun;
using UnityEngine;

public class InventorySelectionSync : MonoBehaviourPun
{
    [SerializeField] private Transform worldItemsRoot;

    private IPlayerInventory _inv;
    private string _lastSentName;

    void Start()
    {
        _inv = GetComponent<IPlayerInventory>() ?? GetComponentInChildren<IPlayerInventory>();
        if (!worldItemsRoot)
            worldItemsRoot = FindChildRecursive(transform, "GunHolder") ?? transform;

        if (photonView.IsMine && _inv != null)
        {
            _inv.OnSelectionChanged += HandleSelectionChanged;
            _inv.OnInventoryChanged += HandleInventoryChanged;
            
            SendSelectionByName(GetCurrentSelectedGoName());
        }
    }

    void OnDestroy()
    {
        if (photonView.IsMine && _inv != null)
        {
            _inv.OnSelectionChanged -= HandleSelectionChanged;
            _inv.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    void HandleSelectionChanged(int _)
    {
        SendSelectionByName(GetCurrentSelectedGoName());
    }

    void HandleInventoryChanged()
    {
        SendSelectionByName(GetCurrentSelectedGoName());
    }

    string GetCurrentSelectedGoName()
    {
        if (_inv == null) return null;
        
        if (_inv.IsWeaponSelected && _inv.GetSelectedGun)
            return _inv.GetSelectedGun.gameObject.name;
        
        var tool = _inv.GetSelectedTool();
        if (tool) return tool.gameObject.name;

        return null;
    }

    void SendSelectionByName(string goName)
    {
        if (string.IsNullOrEmpty(goName) || goName == _lastSentName) return;
        _lastSentName = goName;
        
        photonView.RPC(nameof(RPC_ShowItemByName), RpcTarget.OthersBuffered, goName);
    }

    [PunRPC]
    void RPC_ShowItemByName(string goName)
    {
        if (!worldItemsRoot)
            worldItemsRoot = FindChildRecursive(transform, "GunHolder") ?? transform;
        
        for (int i = 0; i < worldItemsRoot.childCount; i++)
        {
            var child = worldItemsRoot.GetChild(i).gameObject;
            child.SetActive(child.name == goName);
        }
    }
    
    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (!root) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var t = FindChildRecursive(root.GetChild(i), name);
            if (t) return t;
        }
        return null;
    }
}
