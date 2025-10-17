public interface IInteractable
{
    bool CanInteract();
    void Interact(int? ownerActorNumber = null);
    string GetPrompt();
}