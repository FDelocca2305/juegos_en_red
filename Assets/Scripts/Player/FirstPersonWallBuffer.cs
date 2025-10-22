using UnityEngine;

public class FirstPersonWallBuffer : MonoBehaviour
{
    public CharacterController controller;
    public Transform head;
    public float minDistance = 0.12f;
    public float radius = 0.1f;
    public LayerMask envMask = ~0;

    void LateUpdate()
    {
        var origin = head.position;
        var dir = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        if (Physics.SphereCast(origin, radius, dir, out var hit, minDistance, envMask, QueryTriggerInteraction.Ignore))
        {
            var back = -dir * (minDistance - hit.distance);
            controller.Move(back);
        }
    }
}
