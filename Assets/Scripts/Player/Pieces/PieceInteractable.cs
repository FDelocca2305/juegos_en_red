using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PieceInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Piece";
    [SerializeField] private int objectiveIndex;

    private PieceManagerLocal owner;
    private bool collected;

    public void Init(PieceManagerLocal m, int index, string title)
    {
        owner = m; objectiveIndex = index; displayName = title;
    }

    public bool CanInteract() => !collected;
    public string GetPrompt() => "Pick Up Piece";

    public void Interact(int? ownerActorNumber)
    {
        if (collected) return;
        collected = true;
        AudioManager.Instance?.PlayLocalSound("piece_pickup");
        owner?.OnPieceCollected(objectiveIndex, displayName);
        Destroy(gameObject);
    }
}