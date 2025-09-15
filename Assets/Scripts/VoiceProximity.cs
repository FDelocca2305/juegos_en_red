using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonVoiceView))]
public class VoiceProximity : MonoBehaviour
{
    public float maxHearingDistance = 15f;

    private PhotonView photonView;
    private PhotonVoiceView voiceView;
    private AudioSource audioSource;
    private Transform localListener;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        voiceView = GetComponent<PhotonVoiceView>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            // Este es el jugador local, no necesita reproducir audio de sí mismo
            enabled = false;
            return;
        }

        if (voiceView != null && voiceView.SpeakerInUse != null)
        {
            audioSource = voiceView.SpeakerInUse.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource no encontrado en el Speaker.");
            enabled = false;
            return;
        }

        // Buscar la cámara local (listener)
        var localCam = FindLocalCamera();
        if (localCam != null)
        {
            localListener = localCam.transform;
        }

        if (localListener == null)
        {
            Debug.LogWarning("No se encontró cámara local.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (localListener == null || audioSource == null) return;

        float distance = Vector3.Distance(transform.position, localListener.position);
        float volume = Mathf.Clamp01(1 - (distance / maxHearingDistance));

        audioSource.volume = volume;
        audioSource.enabled = volume > 0.01f;
    }

    private Camera FindLocalCamera()
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy)
                return cam;
        }

        return Camera.main;
    }
}
