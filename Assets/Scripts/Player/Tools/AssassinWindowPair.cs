using UnityEngine;

public class AssassinWindowPair : MonoBehaviour
{
    [Header("Endpoints (interior / exterior)")]
    [SerializeField] private AssassinWindowEndpoint sideA;
    [SerializeField] private AssassinWindowEndpoint sideB;
    [SerializeField] private float ejectDistance = 0.6f;
    
    public bool IsUsable =>
        sideA && sideB && sideA.IsConfigured && sideB.IsConfigured;

    public bool TryTeleport(Transform playerRoot, AssassinWindowEndpoint from)
    {
        if (!IsUsable || (from != sideA && from != sideB)) return false;

        var to = (from == sideA) ? sideB : sideA;
        var dst = to.LandingPoint;
        if (!dst) return false;
        
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        var flatFwd = new Vector3(dst.forward.x, 0f, dst.forward.z).normalized;
        var targetPos = dst.position + (flatFwd * ejectDistance);

        playerRoot.position = targetPos;
        if (flatFwd.sqrMagnitude > 0.001f)
            playerRoot.rotation = Quaternion.LookRotation(flatFwd, Vector3.up);

        if (cc) cc.enabled = true;

        return true;
    }

    public AssassinWindowEndpoint Other(AssassinWindowEndpoint from)
        => (from == sideA) ? sideB : sideA;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!sideA || !sideB) return;
        if (!sideA.LandingPoint || !sideB.LandingPoint) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(sideA.UsePivot.position, sideB.LandingPoint.position);
        Gizmos.DrawLine(sideB.UsePivot.position, sideA.LandingPoint.position);
    }
#endif
}