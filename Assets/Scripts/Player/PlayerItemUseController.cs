using UnityEngine;

namespace Player
{
    public class PlayerItemUseController : MonoBehaviour
    {
        private IPlayerInventory _inventory;

        private void Awake()
        {
            _inventory = ServiceLocator.Resolve<IPlayerInventory>();
        }

        private void Update()
        {
            if (_inventory.IsWeaponSelected) return;

            var tool = _inventory.GetSelectedTool();
            if (tool == null) return;

            if (Input.GetMouseButtonDown(0)) tool.OnPrimaryActionDown();
            if (Input.GetMouseButton(0))     tool.OnPrimaryActionHold();
            if (Input.GetMouseButtonUp(0))   tool.OnPrimaryActionUp();
        }
    }
}