using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(PhotonVoiceView))]
public class PlayerVoiceSetup : MonoBehaviourPunCallbacks
{
    [SerializeField] private Recorder recorder;
    [SerializeField] private Speaker  speakerWithAudioSource;

    private PhotonVoiceView pvv;

    private void Awake()
    {
        if (!recorder)               recorder               = GetComponentInChildren<Recorder>(true);
        if (!speakerWithAudioSource) speakerWithAudioSource = GetComponentInChildren<Speaker>(true);
    }

    private void Start()
    {
        if (photonView.IsMine && recorder)
        {
            recorder.InterestGroup    = (byte)photonView.OwnerActorNr;
            recorder.TransmitEnabled  = true;
            recorder.RecordWhenJoined = true;
            recorder.VoiceDetection   = true;
            
            var client = PunVoiceClient.Instance?.Client;
            if (client != null && client.InRoom)
                recorder.RecordingEnabled = true;
        }
        else if (!photonView.IsMine && speakerWithAudioSource)
        {
            var src = speakerWithAudioSource.GetComponent<AudioSource>();
            if (src)
            {
                src.spatialBlend = 1f;
                src.rolloffMode  = AudioRolloffMode.Logarithmic;
                src.minDistance  = 2f;
                src.maxDistance  = 20f;
            }
        }
    }

    public override void OnJoinedRoom()
    {
        if (photonView.IsMine && recorder)
            recorder.RecordingEnabled = true;
    }
}
