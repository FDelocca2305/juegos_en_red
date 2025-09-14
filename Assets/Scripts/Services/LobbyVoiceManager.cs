using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Gestiona el chat de voz global en el lobby
/// Todos los jugadores pueden escucharse entre sí sin restricciones de proximidad
/// </summary>
public class LobbyVoiceManager : MonoBehaviourPunCallbacks
{
    [Header("Voice Components")]
    [SerializeField] private Recorder voiceRecorder;
    [SerializeField] private Speaker voiceSpeaker;
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Button voiceToggleButton;
    [SerializeField] private TMPro.TMP_Text voiceStatusText;
    [SerializeField] private GameObject voiceIndicator;
    
    [Header("Settings")]
    [SerializeField] private bool startWithVoiceEnabled = true;
    [SerializeField] private float voiceCheckInterval = 0.1f;
    
    private bool isVoiceEnabled = true;
    private Coroutine voiceCheckCoroutine;
    
    private void Start()
    {
        InitializeVoiceSystem();
        SetupUI();
    }
    
    private void InitializeVoiceSystem()
    {
        // Buscar componentes de voz si no están asignados
        if (voiceRecorder == null)
            voiceRecorder = GetComponent<Recorder>();
            
        if (voiceSpeaker == null)
            voiceSpeaker = GetComponent<Speaker>();
        
        // Configurar el sistema de voz para lobby global
        if (voiceRecorder != null)
        {
            // Grupo 0 = chat global (todos se escuchan)
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
        
        // Configurar speaker para audio global
        if (voiceSpeaker != null)
        {
            var audioSource = voiceSpeaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Configuración para audio global (sin atenuación por distancia)
                audioSource.spatialBlend = 0f; // 2D audio
                audioSource.volume = 1f;
            }
            
            Debug.Log("[LobbyVoice] Speaker configurado");
        }
        else
        {
            Debug.LogError("[LobbyVoice] Speaker no encontrado!");
        }
        
        // Iniciar verificación de estado de voz
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
            voiceRecorder.TransmitEnabled = enabled;
            voiceRecorder.RecordingEnabled = enabled;
        }
        
        UpdateVoiceButtonUI();
    }
    
    public override void OnJoinedRoom()
    {
        // Asegurar que el sistema de voz esté activo al unirse a la sala
        if (voiceRecorder != null)
        {
            voiceRecorder.RecordingEnabled = isVoiceEnabled;
        }
    }
    
    private void OnDestroy()
    {
        if (voiceCheckCoroutine != null)
            StopCoroutine(voiceCheckCoroutine);
    }
    
    // Métodos públicos para control externo
    public bool IsVoiceEnabled => isVoiceEnabled;
    public bool IsRecording => voiceRecorder != null && voiceRecorder.IsCurrentlyTransmitting;
}
