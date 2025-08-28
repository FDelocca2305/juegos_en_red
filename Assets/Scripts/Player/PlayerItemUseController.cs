using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class PlayerItemUseController : MonoBehaviourPunCallbacks
    {
        private IPlayerInventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<IPlayerInventory>() ?? GetComponentInParent<IPlayerInventory>();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (_inventory.IsWeaponSelected) return;

                var tool = _inventory.GetSelectedTool();
                if (tool == null) return;

                if (Input.GetMouseButtonDown(0)) tool.OnPrimaryActionDown();
                if (Input.GetMouseButton(0)) tool.OnPrimaryActionHold();
                if (Input.GetMouseButtonUp(0)) tool.OnPrimaryActionUp();
            }
        }
    }
}