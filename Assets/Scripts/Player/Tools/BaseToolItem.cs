using UnityEngine;

namespace Player
{
    public abstract class BaseToolItem : MonoBehaviour, IToolItem
    {
        [Header("UI")]
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        [Header("Visual")]
        [SerializeField] private GameObject viewModel;

        [Header("Uso")]
        [SerializeField] private float cooldown = 0.25f;

        protected float _nextUseTime;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject ViewModel => viewModel;
        public float Cooldown => cooldown;

        public virtual void OnSelected()
        {
            if (viewModel) viewModel.SetActive(true);
        }

        public virtual void OnDeselected()
        {
            if (viewModel) viewModel.SetActive(false);
        }
        
        protected bool IsReady() => Time.time >= _nextUseTime;
        protected void StartCooldown() => _nextUseTime = Time.time + cooldown;

        public virtual void OnPrimaryActionDown() { }
        public virtual void OnPrimaryActionHold() { }
        public virtual void OnPrimaryActionUp() { }
    }
}