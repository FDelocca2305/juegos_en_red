using System.Linq;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class OuijaCandleRitualController : MonoBehaviourPun
{
    public static OuijaCandleRitualController I { get; private set; }
    
    [SerializeField] private CandleInteractable[] candles;
    [SerializeField] private GameObject ouijaRoot;
    
    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationClip;
    [SerializeField, Range(0f,1f)] private float activationVolume = 1f;
    
    [SerializeField] private bool activated;

    void Awake()
    {
        I = this;
        if (candles == null || candles.Length == 0)
            candles = FindObjectsOfType<CandleInteractable>(true);
        
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (ouijaRoot) ouijaRoot.SetActive(activated);
    }

    public void OnCandleStateChanged()
    {
        if (activated) return;
        if (candles != null && candles.Length > 0 && candles.All(c => c && c.IsLit))
        {
            photonView.RPC(nameof(RPC_EnableOuijaBuffered), RpcTarget.AllBuffered);
            
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RPC_PlayActivationSfx), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_EnableOuijaBuffered()
    {
        if (activated) return;
        activated = true;
        if (ouijaRoot) ouijaRoot.SetActive(true);
    }

    [PunRPC]
    void RPC_PlayActivationSfx()
    {
        if (audioSource && activationClip)
            audioSource.PlayOneShot(activationClip, activationVolume);
    }
}
