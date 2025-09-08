public interface IInteractable
{
    bool CanInteract();
    void Interact();
    string GetPrompt();
}