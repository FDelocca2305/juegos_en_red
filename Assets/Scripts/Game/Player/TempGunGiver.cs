using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Game.Inventory;

public class TempGunGiver : MonoBehaviour, IOnEventCallback
{
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == GameEvents.EVT_GIVE_TEMP_GUN)
        {
            int targetActor = (int)photonEvent.CustomData;
            if (PhotonNetwork.LocalPlayer.ActorNumber != targetActor) return;
            
            if (Game.Inventory.PlayerInventory.Local != null)
            {
                Game.Inventory.PlayerInventory.Local.GiveTempGun();
                return;
            }
            
            foreach (var inv in FindObjectsOfType<Game.Inventory.PlayerInventory>())
                if (inv.GetComponent<PhotonView>()?.IsMine == true)
                {
                    inv.GiveTempGun();
                    break;
                }
        }
    }
}