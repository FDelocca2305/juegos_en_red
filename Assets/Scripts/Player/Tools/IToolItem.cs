using UnityEngine;

namespace Player
{
    public interface IToolItem
    {
        string DisplayName { get; }
        Sprite Icon { get; }
        GameObject ViewModel { get; }
        float Cooldown { get; }

        void OnSelected();
        void OnDeselected();
        
        void OnPrimaryActionDown();
        void OnPrimaryActionHold();
        void OnPrimaryActionUp();
    }
}