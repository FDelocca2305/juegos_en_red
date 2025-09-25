using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class LocalOnlyAudioListener : MonoBehaviourPun
{
    AudioListener al;

    void Awake()
    {
        al = GetComponent<AudioListener>();
        Apply();
    }

    void OnEnable()  => Apply();
    void OnDisable() { if (al) al.enabled = false; }

    void Apply()
    {
        bool enable =
            photonView == null ||                
            !PhotonNetwork.InRoom ||             
            photonView.IsMine;                   

        al.enabled = enable;
    }
}