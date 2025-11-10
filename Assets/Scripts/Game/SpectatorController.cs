using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SpectatorController : MonoBehaviourPunCallbacks
{
    [SerializeField] private KeyCode nextKey = KeyCode.E;
    [SerializeField] private KeyCode prevKey = KeyCode.Q;
    [SerializeField] private float switchCooldown = 0.25f;

    private readonly List<Transform> _targets = new();
    private int _idx = -1;
    private float _nextSwitch;
    private Camera _cam;

    public void BeginSpectate()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        BuildTargetList();
        EnsureCamera();

        if (_targets.Count > 0)
        {
            _idx = Mathf.Clamp(_idx, 0, _targets.Count - 1);
            SnapTo(_targets[_idx]);
        }
        else
        {
            _idx = -1;
            ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()
                ?.ShowHint("No live players", 5f);
        }
    }

    private void Update()
    {
        if (_idx < 0 || _targets.Count == 0) return;

        if (Input.GetKeyDown(nextKey) && Time.time >= _nextSwitch)
        { _idx = (_idx + 1) % _targets.Count; SnapTo(_targets[_idx]); _nextSwitch = Time.time + switchCooldown; }

        if (Input.GetKeyDown(prevKey) && Time.time >= _nextSwitch)
        { _idx = (_idx - 1 + _targets.Count) % _targets.Count; SnapTo(_targets[_idx]); _nextSwitch = Time.time + switchCooldown; }

        if (!_cam) EnsureCamera();
        if (!_cam) return;
        
        if (_targets[_idx] == null) { PruneTargets(); return; }

        var t = _targets[_idx];
        _cam.transform.SetPositionAndRotation(t.position, t.rotation);
    }

    private void SnapTo(Transform t)
    {
        if (_cam && t) _cam.transform.SetPositionAndRotation(t.position, t.rotation);
        ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()
            ?.ShowHint($"Q (prev) Spectating: {_idx + 1}/{_targets.Count}  |  E (next)", 10f);
    }

    private void BuildTargetList()
    {
        _targets.Clear();

        foreach (var pc in FindObjectsOfType<PlayerController>())
        {
            var pv = pc.GetComponent<PhotonView>();
            if (!pv || pv.IsMine) continue;

            bool alive = true;
            if (pv.Owner?.CustomProperties != null &&
                pv.Owner.CustomProperties.TryGetValue(RoomKeys.ALIVE, out var v) &&
                v is bool b) alive = b;

            if (!alive) continue;

            var vp = pc.transform.Find("ViewPoint");
            _targets.Add(vp ? vp : pc.transform);
        }

        PruneTargets();
    }

    private void PruneTargets()
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
            if (_targets[i] == null) _targets.RemoveAt(i);

        if (_targets.Count == 0) { _idx = -1; return; }
        _idx = (_idx + _targets.Count) % _targets.Count;
    }

    private void EnsureCamera()
    {
        if (_cam) return;
        _cam = Camera.main;
        if (!_cam)
        {
            var go = new GameObject("SpectatorCamera");
            _cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            _cam.tag = "MainCamera";
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps)
    {
        if (changedProps != null && changedProps.ContainsKey(RoomKeys.ALIVE))
            BuildTargetList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) => BuildTargetList();
}
