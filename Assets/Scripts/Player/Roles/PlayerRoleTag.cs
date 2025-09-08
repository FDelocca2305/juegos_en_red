using ExitGames.Client.Photon;
using Photon.Pun;

public class PlayerRoleTag : MonoBehaviourPunCallbacks, IRoleProvider
{
    public RoleId Role { get; private set; } = RoleId.Innocent;

    private void Start() => Refresh();

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (target == photonView.Owner && changedProps.ContainsKey(RoleManager.RoleKey)) Refresh();
    }

    private void Refresh()
    {
        var props = photonView.Owner?.CustomProperties;
        if (props != null && props.TryGetValue(RoleManager.RoleKey, out var v) && v is int i)
            Role = (RoleId)i;
    }
}