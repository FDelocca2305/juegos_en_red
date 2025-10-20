using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RitualPad : MonoBehaviourPun
{
    [SerializeField] private int padIndex;
    public Renderer symbolRenderer;

    void Awake(){ if (TryGetComponent(out Collider c)) c.isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PhotonView>(out var pv)) return;
        if (!pv.IsMine) return;

        if (symbolRenderer) symbolRenderer.enabled = true;
        OuijaRitualManager.I?.LocalPadSet(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<PhotonView>(out var pv)) return;
        if (!pv.IsMine) return;

        if (symbolRenderer) symbolRenderer.enabled = false;
        OuijaRitualManager.I?.LocalPadSet(false);
    }
}