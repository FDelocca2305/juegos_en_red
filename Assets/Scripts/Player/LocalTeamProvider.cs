using UnityEngine;

public class LocalTeamProvider : MonoBehaviour, ILocalTeamProvider
{
    [SerializeField] private int localTeamId = 0;
    public int LocalTeamId => localTeamId;
}