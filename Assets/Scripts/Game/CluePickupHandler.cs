using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ClueMasterHandler : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private int cluesForGun = 5;

    public void OnEvent(EventData photonEvent)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (photonEvent.Code == GameEvents.EVT_CLAIM_CLUE)
        {
            var data = (object[])photonEvent.CustomData;
            int viewId = (int)data[0];
            int actor = (int)data[1];

            var view = PhotonView.Find(viewId);
            if (view == null) return;
            
            view.RPC("RPC_Claimed", RpcTarget.All);

            var player = GetPlayer(actor);
            if (player == null) return;

            int clues = player.CustomProperties.TryGetValue(NetKeys.CLUES, out var c) ? (int)c : 0;
            clues++;
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { NetKeys.CLUES, clues } });

            if (clues >= cluesForGun)
            {
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { NetKeys.CLUES, 0 } });

                PhotonNetwork.RaiseEvent(GameEvents.EVT_GIVE_TEMP_GUN, actor,
                    new RaiseEventOptions { TargetActors = new[] { actor } },
                    SendOptions.SendReliable);
            }
        }
    }

    private Player GetPlayer(int actorNumber)
    {
        foreach (var p in PhotonNetwork.PlayerList) if (p.ActorNumber == actorNumber) return p;
        return null;
    }
}