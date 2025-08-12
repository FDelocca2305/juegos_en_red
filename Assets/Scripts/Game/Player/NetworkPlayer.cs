using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayer : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    
    public void OnPhotonInstantiate(PhotonMessageInfo info) { }
}
