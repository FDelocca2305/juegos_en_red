using UnityEngine;
using Photon.Pun;

public class AssassinWindowEndpoint : MonoBehaviour
{
    [SerializeField] private AssassinWindowPair pair;
    [SerializeField] private Transform usePivot;
    [SerializeField] private Transform landingPoint;

    public AssassinWindowPair Pair => pair;
    public Transform UsePivot => usePivot ? usePivot : transform;
    public Transform LandingPoint => landingPoint;

    public bool IsConfigured =>
        pair != null && landingPoint != null;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(UsePivot.position, 0.11f);
        if (landingPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(landingPoint.position, 0.12f);
        }
    }
#endif
}