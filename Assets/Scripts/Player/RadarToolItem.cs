using System.Collections;
using System.Collections.Generic;
using Player;
using UI.Gameplay;
using UnityEngine;

public class RadarToolItem : BaseToolItem
{
    [Header("Radar")]
    [SerializeField] private float radius = 20f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float pingInterval = 0.25f;

    private float _nextPing;
    private IGameplayUI _ui;

    private void Start()
    {
        ServiceLocator.TryResolve(out _ui);
    }

    public override void OnPrimaryActionHold()
    {
        if (Time.time < _nextPing) return;
        _nextPing = Time.time + pingInterval;

        Vector3 center = transform.root.position;
        var cols = Physics.OverlapSphere(center, radius, playerMask);

        var targets = new List<Transform>();
        foreach (var c in cols)
        {
            var root = c.transform.root;
            if (root == transform.root) continue;
            if (!targets.Contains(root)) targets.Add(root);
        }

        if (_ui != null)
        {
            _ui.ShowRadarTargets(targets.ToArray());
            _ui.ShowHint($"Radar: {targets.Count} detectados", 0.6f);
        }
        else
        {
            Debug.Log($"[Radar] Detectados: {targets.Count}");
        }
    }
}
