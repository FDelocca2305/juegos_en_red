using Player;
using UI.Gameplay;

public class ObjectivePaperToolItem : BaseToolItem
{
    private IGameplayUI _ui;
    private IObjectiveService _obj;

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
        ServiceLocator.TryResolve(out _obj);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        _ui?.ShowObjectives(_obj?.GetObjectives() ?? new[] { "Sin objetivos" }, true);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        _ui?.ShowObjectives(null, false);
    }
}