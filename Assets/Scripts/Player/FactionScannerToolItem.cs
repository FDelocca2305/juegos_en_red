using Player;
using UI.Gameplay;
using UnityEngine;

public class FactionScannerToolItem : BaseToolItem
{
    [Header("Scanner")]
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private LayerMask hittableMask;

    private Camera _cam;
    private IGameplayUI _ui;
    private ILocalTeamProvider _localTeam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
        ServiceLocator.TryResolve(out _localTeam);
    }

    public override void OnPrimaryActionDown()
    {
        if (!IsReady()) return;

        var cam = _cam != null ? _cam : Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hittableMask))
        {
            var targetTeam = hit.collider.GetComponentInParent<ITeamProvider>();
            if (targetTeam != null && _localTeam != null)
            {
                bool ally = targetTeam.TeamId == _localTeam.LocalTeamId;
                _ui?.ShowHint(ally ? "ALLY" : "ENEMY", 1.0f);
            }
            else
            {
                _ui?.ShowHint("SIN INFORMACIÓN", 0.8f);
            }
        }
        else
        {
            _ui?.ShowHint("No hay objetivo", 0.8f);
        }

        StartCooldown();
    }
}