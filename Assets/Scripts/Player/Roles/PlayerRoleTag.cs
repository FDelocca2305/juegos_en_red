using ExitGames.Client.Photon;
using Photon.Pun;

public class PlayerRoleTag : MonoBehaviourPunCallbacks, IRoleProvider
{
    public RoleId Role { get; private set; } = RoleId.Innocent;
    public event System.Action<RoleId> OnRoleChanged;

    private void OnEnable() => RefreshFromOwner();
    private void Start()    => RefreshFromOwner();

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (target != photonView.Owner || changedProps == null) return;
        
        if (changedProps.ContainsKey(RoleManager.RoleKey) ||
            changedProps.ContainsKey(RoleManager.AssignedKey))
        {
            RefreshFromOwner();
        }
    }

    private void RefreshFromOwner()
    {
        var owner = photonView.Owner; if (owner == null) return;
        var props = owner.CustomProperties;
        if (props != null && props.TryGetValue(RoleManager.RoleKey, out var v) && TryToInt(v, out var i))
        {
            var newRole = (RoleId)i;
            if (newRole != Role)
            {
                Role = newRole;
                OnRoleChanged?.Invoke(Role);
            }
        }
    }

    private static bool TryToInt(object o, out int val)
    {
        switch (o)
        {
            case int i: val = i; return true;
            case byte b: val = b; return true;
            case sbyte sb: val = sb; return true;
            case short s: val = s; return true;
            case ushort us: val = us; return true;
            case uint ui: val = (int)ui; return true;
            case long l: val = (int)l; return true;
            case ulong ul: val = (int)ul; return true;
            case float f: val = (int)f; return true;
            case double d: val = (int)d; return true;
            default:
                try { val = System.Convert.ToInt32(o); return true; }
                catch { val = 0; return false; }
        }
    }
}
