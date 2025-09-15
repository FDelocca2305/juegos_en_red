using Player;
using UI.Gameplay;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class DetectiveDetectorToolItem : BaseToolItem
{
    [Header("Uses")]
    [SerializeField] private int maxUses = 2;
    private int usesLeft;

    [Header("Detection")]
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private float aimRadius = 0.35f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask occluders;

    private Camera _cam;
    private IGameplayUI _ui;
    private PhotonView _ownerPv;

    private void Awake()
    {
        _cam = Camera.main;
        _ownerPv = GetComponentInParent<PhotonView>();
        usesLeft = Mathf.Max(0, maxUses);
    }

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
    }

    public override void OnPrimaryActionDown()
    {
        if (!IsReady()) return;
        
        if (usesLeft <= 0)
        {
            _ui?.ShowHint("Low battery", 0.9f);
            StartCooldown();
            return;
        }

        var cam = _cam ? _cam : Camera.main;
        if (!cam)
        {
            StartCooldown();
            return;
        }

        var origin = cam.transform.position;
        var dir    = cam.transform.forward;

        int maskToUse = (playerMask.value != 0) ? playerMask.value : Physics.DefaultRaycastLayers;

        var hits = Physics.SphereCastAll(
                        origin,
                        aimRadius,
                        dir,
                        maxDistance,
                        maskToUse,
                        QueryTriggerInteraction.Collide)
                    .OrderBy(h => h.distance);

        IRoleProvider rpFound = null;

        foreach (var h in hits)
        {
            var pv = h.collider.GetComponentInParent<PhotonView>();
            if (pv && _ownerPv && pv.ViewID == _ownerPv.ViewID) continue;
            
            if (occluders.value != 0 &&
                Physics.Linecast(origin, h.point, out var block, occluders, QueryTriggerInteraction.Ignore))
                continue;

            rpFound = h.collider.GetComponentInParent<IRoleProvider>();
            if (rpFound != null) break;
        }

        string msg;
        if (rpFound != null)
        {
            msg = (rpFound.Role == RoleId.Assassin) ? "ASSASSIN" : "INNOCENT";
        }
        else
        {
            msg = "No objective";
        }
        
        usesLeft--;
        
        if (usesLeft <= 0)        msg += " — Low battery";
        else                      msg += $" ({usesLeft} left)";

        _ui?.ShowHint(msg, 1.2f);
        StartCooldown();
    }
}
