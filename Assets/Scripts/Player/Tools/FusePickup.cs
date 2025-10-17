using Photon.Pun;
using UnityEngine;

public class FusePickup : MonoBehaviour, IInteractable
{
    public bool CanInteract()
    {
        return true;
    }

    public void Interact(int? ownerActorNumber)
    {
        if (!FuseController.Exists) return;
        if (ownerActorNumber is not null)
            FuseController.I.TryPickupFuse(ownerActorNumber.Value);
    }

    public string GetPrompt() => "Take fuse";
}