using UnityEngine;

public class FuseBox : MonoBehaviour, IInteractable
{
    public bool CanInteract() => true;

    public void Interact(int? ownerActorNumber = null)
    {
        if (!FuseController.Exists) return;
        if (!FuseController.I.IsLocalHolder()) return;

        FuseController.I.RepairPower();
    }

    public string GetPrompt() => (FuseController.Exists && FuseController.I.IsLocalHolder())
        ? "Repair (E)" : "Needs Fuse";
}