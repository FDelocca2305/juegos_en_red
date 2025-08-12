using Photon.Pun;
using UnityEngine;

public class AssassinMelee : MonoBehaviourPun
{
    [SerializeField] private float range = 2f;
    [SerializeField] private float cooldown = 0.8f;
    private float nextHit;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetMouseButtonDown(0) && Time.time >= nextHit)
        {
            nextHit = Time.time + cooldown;
            TryHit();
        }
    }

    private void TryHit()
    {
        var cam = GetComponentInChildren<Camera>();
        var origin = cam ? cam.transform.position : transform.position + Vector3.up;
        var dir = cam ? cam.transform.forward : transform.forward;

        if (Physics.SphereCast(origin, 0.5f, dir, out var hit, range))
        {
            hit.collider.GetComponentInParent<IDamageable>()?.ApplyDamage(1, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
}