using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class AssassinSabotageAbility : MonoBehaviourPunCallbacks
{
    [SerializeField] private float cooldownSeconds = 60f;
    private float _nextUse;

    public bool IsAssassinLocal =>
        photonView.IsMine &&
        ServiceLocator.TryResolve<ILocalRoleProvider>(out var roles) &&
        roles.LocalRole == RoleId.Assassin;

    public float RemainingCooldown => Mathf.Max(0f, _nextUse - Time.time);
    public bool CanUse() => IsAssassinLocal && Time.time >= _nextUse;
    
    public void Consume() { _nextUse = Time.time + cooldownSeconds; }
}