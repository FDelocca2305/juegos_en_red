using UnityEngine;
using Photon.Pun;

public class AssassinWindow : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float cooldown = 30f;
    private float _next;

    private void OnTriggerEnter(Collider other)
    {
        var view = other.GetComponentInParent<PhotonView>();
        if (!view || !view.IsMine) return;

        if (!ServiceLocator.TryResolve<ILocalRoleProvider>(out var roles) ||
            roles.LocalRole != RoleId.Assassin) return;

        if (Time.time < _next) return;

        other.transform.root.position = exitPoint.position;
        _next = Time.time + cooldown;
        ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint("Usaste la ventana", 1f);
    }
}