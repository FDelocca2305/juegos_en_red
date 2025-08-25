using UnityEngine;
public class TeamTag : MonoBehaviour, ITeamProvider
{
    [SerializeField] private int teamId = 0;
    public int TeamId => teamId;
}