using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private int hp = 1;
    private bool isDead;

    public void ApplyDamage(int amount, int attackerActor)
    {
        if (!photonView.IsMine || isDead) return;

        hp -= amount;
        if (hp <= 0)
        {
            isDead = true;
            
            var ht = new Hashtable { { NetKeys.ALIVE, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
            
            photonView.RPC(nameof(RPC_Die), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_Die()
    {
        if (TryGetComponent<FirstPersonController>(out var fps))
            fps.enabled = false;
        
        if (TryGetComponent<Collider>(out var col))
            col.enabled = false;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // TODO: ocultar modelo, activar espectador, reproducir animación/sfx, etc.
    }
}