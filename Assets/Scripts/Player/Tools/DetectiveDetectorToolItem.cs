using Player;
using UI.Gameplay;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class DetectiveDetectorToolItem : BaseToolItem
{
    [SerializeField] private int uses = 2;
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
    }

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
    }

    public override void OnPrimaryActionDown()
    {
        if (!IsReady() || uses <= 0) return;

        var cam = _cam ? _cam : Camera.main;
        if (!cam)
        {
            StartCooldown();
            return;
        }

        var origin = cam.transform.position;
        var dir = cam.transform.forward;

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
            
            if (occluders.value != 0)
            {
                if (Physics.Linecast(origin, h.point, out var block, occluders, QueryTriggerInteraction.Ignore))
                    continue;
            }

            rpFound = h.collider.GetComponentInParent<IRoleProvider>();
            if (rpFound != null) break;
        }

        if (rpFound != null)
        {
            _ui?.ShowHint(rpFound.Role == RoleId.Assassin ? "ASSASSIN" : "INOCENT", 1.2f);
            uses--;
        }
        else
        {
            _ui?.ShowHint("No objective", .6f);
        }

        StartCooldown();
    }
}
