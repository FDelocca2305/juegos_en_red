using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class LocalOnlyAudioListener : MonoBehaviourPun
{
    void Awake()
    {
        var al = GetComponent<AudioListener>();
        al.enabled = photonView.IsMine;
    }
    void OnEnable()  { GetComponent<AudioListener>().enabled = photonView.IsMine; }
    void OnDisable() { var al = GetComponent<AudioListener>(); if (al) al.enabled = false; }
}