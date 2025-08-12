using Photon.Pun;
using UnityEngine;

public class TempGun : MonoBehaviourPun
{
    [SerializeField] private float range = 40f;
    [SerializeField] private LayerMask hitMask;
    private bool used;

    void Update()
    {
        if (!photonView.IsMine || used) return;
        if (Input.GetMouseButtonDown(0))
        {
            used = true;
            var cam = GetComponentInChildren<Camera>();
            if (cam && Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, hitMask))
            {
                hit.collider.GetComponentInParent<IDamageable>()?.ApplyDamage(1, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            
            GetComponentInParent<PlayerInventory>()?.ConsumeTempGun();
        }
    }
}