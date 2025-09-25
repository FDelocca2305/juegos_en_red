using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyVoiceManager : MonoBehaviourPunCallbacks
{
    [Header("Voice Components")]
    [SerializeField] private Recorder voiceRecorder;
    [SerializeField] private Speaker voiceSpeaker;
    [SerializeField] private GameObject speakerPrefab;
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Button voiceToggleButton;
    [SerializeField] private TMPro.TMP_Text voiceStatusText;
    [SerializeField] private GameObject voiceIndicator;
    
    [Header("Settings")]
    [SerializeField] private bool startWithVoiceEnabled = true;
    [SerializeField] private float voiceCheckInterval = 0.1f;
    
    private bool isVoiceEnabled = true;
    private Coroutine voiceCheckCoroutine;
    
    void Awake()
    {
        var pvc = PunVoiceClient.Instance;
        pvc.UsePunAppSettings  = true;
        pvc.AutoConnectAndJoin = true;
        
        pvc.SpeakerPrefab = speakerPrefab;

        DontDestroyOnLoad(pvc.gameObject);
        foreach (var other in FindObjectsOfType<PunVoiceClient>())
            if (other != pvc) Destroy(other.gameObject);
        
        if (voiceRecorder == null) voiceRecorder = GetComponent<Recorder>();
        if (voiceRecorder != null)
        {
            pvc.PrimaryRecorder = voiceRecorder;
            voiceRecorder.RecordWhenJoined  = true;
            voiceRecorder.RecordingEnabled  = startWithVoiceEnabled;
            voiceRecorder.TransmitEnabled   = startWithVoiceEnabled;
        }
    }
    
    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scn, LoadSceneMode mode)
    {
        if (scn.name == "LobbyScene")
            PrepareVoiceForLobby();
    }
    
    private void Start()
    {
        InitializeVoiceSystem();
        SetupUI();
    }
    
    private void InitializeVoiceSystem()
    {
        if (voiceRecorder == null)
            voiceRecorder = GetComponent<Recorder>();
            
        if (voiceSpeaker == null)
            voiceSpeaker = GetComponent<Speaker>();
        
        if (voiceRecorder != null)
        {
            voiceRecorder.InterestGroup = 0;
            voiceRecorder.TransmitEnabled = startWithVoiceEnabled;
            voiceRecorder.RecordWhenJoined = true;
            voiceRecorder.VoiceDetection = true;
            voiceRecorder.RecordingEnabled = startWithVoiceEnabled;
            
            isVoiceEnabled = startWithVoiceEnabled;
            
            Debug.Log($"[LobbyVoice] Recorder configurado - InterestGroup: {voiceRecorder.InterestGroup}, RecordingEnabled: {voiceRecorder.RecordingEnabled}");
        }
        else
        {
            Debug.LogError("[LobbyVoice] Recorder no encontrado!");
        }
        
        if (voiceSpeaker != null)
        {
            var audioSource = voiceSpeaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.spatialBlend = 0f;
                audioSource.volume = 1f;
            }
            
            Debug.Log("[LobbyVoice] Speaker configurado");
        }
        else
        {
            Debug.LogError("[LobbyVoice] Speaker no encontrado!");
        }
        
        if (voiceCheckCoroutine == null)
            voiceCheckCoroutine = StartCoroutine(VoiceStatusCheck());
    }
    
    private void SetupUI()
    {
        if (voiceToggleButton != null)
        {
            voiceToggleButton.onClick.AddListener(ToggleVoice);
            UpdateVoiceButtonUI();
        }
    }
    
    private System.Collections.IEnumerator VoiceStatusCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(voiceCheckInterval);
            
            if (voiceRecorder != null)
            {
                bool isRecording = voiceRecorder.IsCurrentlyTransmitting;
                UpdateVoiceStatusUI(isRecording);
            }
        }
    }
    
    private void UpdateVoiceStatusUI(bool isRecording)
    {
        if (voiceStatusText != null)
        {
            if (isRecording)
            {
                voiceStatusText.text = "Speaking...";
                voiceStatusText.color = Color.green;
            }
            else
            {
                voiceStatusText.text = "Mute";
                voiceStatusText.color = Color.gray;
            }
        }
        
        if (voiceIndicator != null)
            voiceIndicator.SetActive(isRecording);
    }
    
    private void UpdateVoiceButtonUI()
    {
        if (voiceToggleButton != null)
        {
            var buttonText = voiceToggleButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = isVoiceEnabled ? "Mute" : "Activate";
            }
        }
    }
    
    public void ToggleVoice()
    {
        isVoiceEnabled = !isVoiceEnabled;
        
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = isVoiceEnabled;
            voiceRecorder.RecordingEnabled = isVoiceEnabled;
        }
        
        UpdateVoiceButtonUI();
        
        Debug.Log($"[LobbyVoice] Voice {(isVoiceEnabled ? "enabled" : "disabled")}");
    }
    
    public void SetVoiceEnabled(bool enabled)
    {
        isVoiceEnabled = enabled;
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled  = enabled;
            voiceRecorder.RecordingEnabled = enabled;
        }
    }
    
    public override void OnJoinedRoom()
    {
        PrepareVoiceForLobby();
    }
    
    public void PrepareVoiceForLobby()
    {
        var pvc = PunVoiceClient.Instance;
        if (pvc == null || voiceRecorder == null) return;
        
        voiceRecorder.UserData = null;
        
        if (pvc.SpeakerPrefab == null && speakerPrefab != null)
            pvc.SpeakerPrefab = speakerPrefab;

        if (pvc.PrimaryRecorder != voiceRecorder)
            pvc.PrimaryRecorder = voiceRecorder;

        pvc.AddRecorder(voiceRecorder);
        voiceRecorder.RecordingEnabled = isVoiceEnabled;
        voiceRecorder.TransmitEnabled  = isVoiceEnabled;
        
        string expected = PhotonNetwork.CurrentRoom?.Name + PunVoiceClient.VoiceRoomNameSuffix;
        if (!pvc.Client.InRoom || pvc.Client.CurrentRoom?.Name != expected)
        {
            if (pvc.Client.InRoom) pvc.Client.OpLeaveRoom(false);
            pvc.ConnectAndJoinRoom();
        }

        voiceRecorder.RestartRecording();
    }
    
    private void OnDestroy()
    {
        if (voiceCheckCoroutine != null)
            StopCoroutine(voiceCheckCoroutine);
    }
    
    public bool IsVoiceEnabled => isVoiceEnabled;
    public bool IsRecording => voiceRecorder != null && voiceRecorder.IsCurrentlyTransmitting;
}