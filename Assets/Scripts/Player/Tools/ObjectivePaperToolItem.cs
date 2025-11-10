using Photon.Pun;
using Player;
using UI.Gameplay;
using UnityEngine;

public class ObjectivePaperToolItem : BaseToolItem
{
    private IGameplayUI _ui;
    private IObjectivesTracker _tracker;
    private bool _visible;
    
    private AudioManager _audioManager;
    private PhotonView _photonView;
    
    private void Awake()
    {
        ServiceLocator.TryResolve(out _ui);
        ServiceLocator.TryResolve(out _tracker);
        if (_tracker != null) _tracker.OnObjectivesChanged += Refresh;
        _audioManager = AudioManager.Instance;
        _photonView = GetComponentInParent<PhotonView>();
    }
    
    private void OnEnable()
    {
        if (_visible) Refresh();
    }

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
        ServiceLocator.TryResolve(out _tracker);
        if (_tracker != null) _tracker.OnObjectivesChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (_tracker != null) _tracker.OnObjectivesChanged -= Refresh;
    }

    public override void OnSelected()
    {
        base.OnSelected(); 
        _visible = true; 
        Refresh();

        if (_photonView == null || _photonView.IsMine)
        {
            _audioManager?.PlayLocalSound("equip_paper");
        }
    }

    public override void OnDeselected()
    {
        base.OnDeselected(); _visible = false; _ui?.ShowObjectives(null, false);
    }

    private void Refresh()
    {
        if (!_visible) return;
        if (_ui == null) ServiceLocator.TryResolve(out _ui);
        if (_tracker == null) ServiceLocator.TryResolve(out _tracker);
        var lines = _tracker != null ? _tracker.GetFormattedObjectives() : new[] {"No objectives"};
        _ui?.ShowObjectives(lines, true);
    }
}