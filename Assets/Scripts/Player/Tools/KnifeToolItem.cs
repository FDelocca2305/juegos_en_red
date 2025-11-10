using Photon.Pun;
using UnityEngine;

public class KnifeToolItem : Player.BaseToolItem
{
    [SerializeField] private float range = 2f;
    [SerializeField] private float radius = 0.6f;
    [SerializeField] private float innocentKillCooldown = 15f;
    [SerializeField] private LayerMask playerMask;

    private Camera _cam;
    private AudioManager _audioManager;
    private PhotonView _photonView;

    private void Awake()
    {
        _cam = Camera.main;
        _audioManager = AudioManager.Instance;
        _photonView = GetComponentInParent<PhotonView>();
    }

    public override void OnSelected()
    {
        base.OnSelected();

        if (_photonView != null && !_photonView.IsMine) return;
        _audioManager?.PlayLocalSound("equip_knife");
    }

    public override void OnPrimaryActionDown()
    {
        if (!IsReady()) return;

        if (_photonView == null || _photonView.IsMine)
        {
            _audioManager?.PlayLocalSound("knife_stab");
        }

        var cam = _cam ? _cam : Camera.main;
        var origin = cam ? cam.transform.position : transform.position;
        var dir = cam ? cam.transform.forward : transform.forward;

        var hits = Physics.SphereCastAll(origin, radius, dir, range, playerMask);
        var myPv = GetComponentInParent<PhotonView>();

        foreach (var h in hits)
        {
            var pv = h.collider.GetComponentInParent<PhotonView>();
            if (!pv || pv == myPv) continue;

            var victimRole = h.collider.GetComponentInParent<IRoleProvider>()?.Role ?? RoleId.Innocent;
            pv.RPC("DealDamage", RpcTarget.All, myPv.Owner.NickName);

            if (victimRole == RoleId.Innocent)
                _nextUseTime = Time.time + innocentKillCooldown;
            else
                StartCooldown();

            return;
        }
        
        StartCooldown();
    }
}