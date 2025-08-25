using UnityEngine;

public class ObjectiveService : MonoBehaviour, IObjectiveService
{
    [SerializeField] [TextArea] private string[] objectives;
    public string[] GetObjectives() => objectives;
}
