using UnityEngine;
using Photon.Pun;

public class AssassinWindowAbility : MonoBehaviourPun
{
    [Header("Config")]
    [SerializeField] private float interactDistance = 2.0f;
    [SerializeField] private float facingDotThreshold = 0.55f; // 0.5–0.6
    [SerializeField] private float cooldownSeconds = 3f;
    [SerializeField] private LayerMask windowMask;

    [Header("Anti-bounce")]
    [SerializeField] private float postTeleportGrace = 0.75f;
    [SerializeField] private float samePairBlock = 0.75f;
    [SerializeField] private float minDistanceToPivot = 0.6f;
    [SerializeField] private float overlapRadius = 0.35f;

    private Camera _cam;
    private float _nextUseTime;
    private float _ignoreWindowsUntil;
    private AssassinWindowPair _lastPairUsed;
    private float _lastPairBlockUntil;
    private bool _requireKeyRelease;

    private void Awake()
    {
        if (photonView.IsMine) _cam = Camera.main;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (!ServiceLocator.TryResolve<ILocalRoleProvider>(out var roles) ||
            roles.LocalRole != RoleId.Assassin) return;

        // Grace post-TP
        if (Time.time < _ignoreWindowsUntil) return;

        if (!_cam) _cam = Camera.main;

        // Usá SphereCast para ser más permisivo
        var origin = _cam.transform.position;
        var dir    = _cam.transform.forward;
        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(origin, 0.1f, dir, out hit, interactDistance, windowMask,
                                               QueryTriggerInteraction.Collide);
        if (!hitSomething) return;

        var endpoint = hit.collider.GetComponentInParent<AssassinWindowEndpoint>();
        if (!endpoint || !endpoint.IsConfigured) return;

        var pair = endpoint.Pair;
        if (pair == null || !pair.IsUsable) return;

        // Bloqueo de par reciente
        if (_lastPairUsed == pair && Time.time < _lastPairBlockUntil) return;

        // Distancia mínima al pivot (si estás pegado, no permitir)
        Vector3 toPivot = endpoint.UsePivot.position - _cam.transform.position;
        float distToPivot = toPivot.magnitude;
        if (distToPivot < minDistanceToPivot) return;

        // Mirando razonablemente al pivot
        Vector3 dirToPivot = toPivot / Mathf.Max(distToPivot, 0.0001f);
        if (Vector3.Dot(_cam.transform.forward, dirToPivot) < facingDotThreshold) return;

        // Si estás dentro de triggers de ventana, no permitir
        if (Physics.OverlapSphere(transform.position, overlapRadius, windowMask, QueryTriggerInteraction.Collide).Length > 0)
            return;

        // Cooldown de la habilidad
        float remaining = _nextUseTime - Time.time;
        if (remaining > 0f)
        {
            ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()
                ?.ShowHint($"Ventana en cooldown ({remaining:0.0}s)", 0.05f);
            return;
        }

        // Evitar auto-repeat mientras la tecla siga apretada
        if (_requireKeyRelease)
        {
            if (!Input.GetKeyUp(KeyCode.E)) return;
            _requireKeyRelease = false;
            return;
        }

        // Prompt
        ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()
            ?.ShowHint("Press <b>E</b> to use window", 0.05f);

        if (Input.GetKeyDown(KeyCode.E))
        {
            var root = transform.root;
            if (pair.TryTeleport(root, endpoint))
            {
                _nextUseTime       = Time.time + cooldownSeconds;
                _ignoreWindowsUntil = Time.time + postTeleportGrace;
                _lastPairUsed      = pair;
                _lastPairBlockUntil = Time.time + samePairBlock;
                _requireKeyRelease = true;

                ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()
                    ?.ShowHint("Usaste la ventana", 1f);
            }
        }
    }
}
