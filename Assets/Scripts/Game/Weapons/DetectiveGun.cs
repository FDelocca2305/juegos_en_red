using Photon.Pun;
using UnityEngine;

public class DetectiveGun : MonoBehaviourPun
{
    [SerializeField] private int ammo = 3;
    [SerializeField] private float cooldown = 1.2f;
    [SerializeField] private float range = 50f;
    [SerializeField] private LayerMask hitMask;

    private float nextFire;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFire && ammo > 0)
        {
            nextFire = Time.time + cooldown;
            ammo--;
            Fire();
        }
    }

    private void Fire()
    {
        var cam = GetComponentInChildren<Camera>();
        if (cam == null) return;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, hitMask))
        {
            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.ApplyDamage(1, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
}