using System.Collections;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-200)]
public class SceneReadyFlag : MonoBehaviour
{
    private IEnumerator Start()
    {
        while (!PhotonNetwork.InRoom) yield return null;
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { [RoomKeys.READY] = true }
        );
        Debug.Log("[READY] Local player marked ready.");
    }
}
