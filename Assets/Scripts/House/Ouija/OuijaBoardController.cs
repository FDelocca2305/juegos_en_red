using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class OuijaBoardController : MonoBehaviourPun
{
    [Header("Refs")]
    [SerializeField] private Transform arrow;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Timings")]
    [SerializeField] private float letterTravel = 1.5f;
    [SerializeField] private float letterHold   = 0.6f;

    Dictionary<string, Transform> _map;

    void Awake()
    {
        _map = new Dictionary<string, Transform>();
        foreach (var a in GetComponentsInChildren<OuijaLetter>(true))
        {
            var key = a.Symbol.ToUpper();
            if (!_map.ContainsKey(key)) _map.Add(key, a.transform);
        }
    }

    public bool TryGetAnchor(string sym, out Transform t) =>
        _map.TryGetValue(sym.ToUpper(), out t);
    
    [PunRPC]
    public void RPC_StartRitual(string[] sequence, double startTime)
    {
        StopAllCoroutines();
        StartCoroutine(Play(sequence, startTime));
    }

    IEnumerator Play(string[] seq, double startTime)
    {
        Debug.Log("play ritual");
        while (PhotonNetwork.Time < startTime) yield return null;

        var current = arrow.position;
        var fixedRot  = arrow.rotation;
        
        for (int i = 0; i < seq.Length; i++)
        {
            if (!TryGetAnchor(seq[i], out var target)) continue;

            Vector3 src = current;
            Vector3 dst = target.position;
            
            double legStart = PhotonNetwork.Time;
            while (PhotonNetwork.Time - legStart < letterTravel)
            {
                float t = Mathf.Clamp01((float)((PhotonNetwork.Time - legStart) / letterTravel));
                float k = ease.Evaluate(t);
                arrow.position = Vector3.Lerp(src, dst, k);
                arrow.rotation = fixedRot;
                yield return null;
            }

            arrow.position = dst;
            current = dst;

            double holdStart = PhotonNetwork.Time;
            while (PhotonNetwork.Time - holdStart < letterHold) yield return null;
        }
    }
}
