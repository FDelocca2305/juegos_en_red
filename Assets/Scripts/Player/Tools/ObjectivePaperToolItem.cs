using Player;
using UI.Gameplay;
using UnityEngine;

public class ObjectivePaperToolItem : BaseToolItem
{
    private IGameplayUI _ui;
    private IObjectivesTracker _tracker;
    private bool _visible;

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
        base.OnSelected(); _visible = true; Refresh();
    }

    public override void OnDeselected()
    {
        base.OnDeselected(); _visible = false; _ui?.ShowObjectives(null, false);
    }

    private void Refresh()
    {
        if (!_visible) return;
        var lines = _tracker != null ? _tracker.GetFormattedObjectives() : new[] {"No objectives"};
        _ui?.ShowObjectives(lines, true);
    }
}