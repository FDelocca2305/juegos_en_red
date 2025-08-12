using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CluePickup : MonoBehaviourPun
{
    private bool claimed;

    private void OnTriggerEnter(Collider other)
    {
        if (claimed) return;
        var view = other.GetComponentInParent<PhotonView>();
        if (view != null && view.IsMine)
        {
            var data = new object[] { photonView.ViewID, view.OwnerActorNr };
            PhotonNetwork.RaiseEvent(GameEvents.EVT_CLAIM_CLUE, data,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }
    }
    
    [PunRPC] private void RPC_Claimed()
    {
        if (claimed) return;
        claimed = true;
        PhotonNetwork.Destroy(gameObject);
    }
}