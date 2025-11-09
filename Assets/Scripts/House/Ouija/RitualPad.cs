using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RitualPad : MonoBehaviourPun
{
    [SerializeField] private int padIndex;
    public GameObject symbolActivated;

    void Awake()
    {
        if (TryGetComponent(out Collider c)) c.isTrigger = true;
        if (symbolActivated)
        {
            symbolActivated.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PhotonView>(out var pv)) return;
        if (!pv.IsMine) return;

        if (symbolActivated) symbolActivated.SetActive(true);
        OuijaRitualManager.I?.LocalPadSet(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<PhotonView>(out var pv)) return;
        if (!pv.IsMine) return;

        if (symbolActivated) symbolActivated.SetActive(false);
        OuijaRitualManager.I?.LocalPadSet(false);
    }
}