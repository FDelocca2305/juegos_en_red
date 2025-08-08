using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayer : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Refs")]
    [SerializeField] private Renderer[] colorRenderers;

    private static readonly Color[] Palette =
    {
        new Color32(235, 87, 87, 255),
        new Color32(39, 174, 96, 255),
        new Color32(45, 156, 219, 255),
        new Color32(241, 196, 15, 255),
    };

    private void Start()
    {
        ApplyColorFromSeat(photonView.OwnerActorNr);
    }

    private void ApplyColorFromSeat(int ownerActorNumber)
    {
        var key = NetKeys.SeatKey(ownerActorNumber);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out var v))
        {
            int seat = (int)v;
            var color = Palette[Mathf.Clamp(seat, 0, Palette.Length - 1)];
            foreach (var r in colorRenderers) r.material.color = color;
        }
    }
    
    public void OnPhotonInstantiate(PhotonMessageInfo info) { }
}
