using Player;
using UI.Gameplay;
using UnityEngine;

public class DetectiveDetectorToolItem : BaseToolItem
{
    [SerializeField] private int uses = 2;
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private LayerMask playerMask;

    private Camera _cam;
    private IGameplayUI _ui;

    private void Awake() => _cam = Camera.main;
    private void Start() => ServiceLocator.TryResolve(out _ui);

    public override void OnPrimaryActionDown()
    {
        if (!IsReady() || uses <= 0) return;

        var cam = _cam ? _cam : Camera.main;
        var ray = cam.ViewportPointToRay(new Vector3(.5f, .5f));
        if (Physics.Raycast(ray, out var hit, maxDistance, playerMask))
        {
            var rp = hit.collider.GetComponentInParent<IRoleProvider>();
            _ui?.ShowHint(rp != null && rp.Role == RoleId.Assassin ? "ASESINO" : "INOCENTE", 1.2f);
            uses--;
        }
        else _ui?.ShowHint("No hay objetivo", .6f);

        StartCooldown();
    }
}