using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpectatorController : MonoBehaviour
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
        _targets.Clear();
        foreach (var pc in FindObjectsOfType<PlayerController>())
        {
            var pv = pc.GetComponent<PhotonView>();
            if (!pv || pv.IsMine) continue;
            
            var owner = pv.Owner;
            bool alive = (bool)(owner.CustomProperties?["alive"] ?? true);
            if (alive) _targets.Add(pc.transform.Find("ViewPoint") ?? pc.transform);
        }

        _cam = Camera.main;
        if (_targets.Count > 0) { _idx = 0; SnapTo(_targets[_idx]); }
    }

    private void Update()
    {
        if (_idx < 0 || _targets.Count == 0) return;
        if (Time.time >= _nextSwitch)
        {
            if (Input.GetKeyDown(nextKey)) { _idx = (_idx + 1) % _targets.Count; SnapTo(_targets[_idx]); }
            if (Input.GetKeyDown(prevKey)) { _idx = (_idx - 1 + _targets.Count) % _targets.Count; SnapTo(_targets[_idx]); }
            _nextSwitch = Time.time + switchCooldown;
        }
        
        if (_cam && _targets[_idx])
        {
            _cam.transform.SetPositionAndRotation(_targets[_idx].position, _targets[_idx].rotation);
        }
    }

    private void SnapTo(Transform t)
    {
        if (_cam && t) _cam.transform.SetPositionAndRotation(t.position, t.rotation);
        ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint($"Q (prev) Spectating: {_idx+1}/{_targets.Count} E (next)", .6f);
    }
}