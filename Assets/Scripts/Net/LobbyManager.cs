using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Transform playerListParent;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private TMP_Text gameStatusText;
    [SerializeField] private TMP_Text roundStatsText;
    
    [Header("Voice Chat")]
    [SerializeField] private LobbyVoiceManager voiceManager;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float voiceCheckInterval = 0.1f;
    
    private List<GameObject> playerListItems = new List<GameObject>();
    
    // Estadísticas de rondas (se cargan desde las propiedades de la sala)
    private int roundsPlayed = 0;
    private int assassinWins = 0;
    private int innocentWins = 0;
    
    private void Start()
    {
        InitializeLobby();
        SetupVoiceManager();
        LoadRoomStats();
        UpdateUI();
        
        // Verificar si estamos conectados a Photon
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("[LobbyManager] No conectado a Photon, intentando reconectar...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    private void InitializeLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
            
        // Configurar botones
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartNewRound);
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
        
        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(LeaveRoom);
        
        // Configurar fase del lobby
        if (PhotonNetwork.IsMasterClient)
        {
            RoomKeys.SetPhase(RoomKeys.Phase_Lobby);
        }
    }
    
    private void LoadRoomStats()
    {
        if (PhotonNetwork.CurrentRoom?.CustomProperties != null)
        {
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            roundsPlayed = (int)(props["rounds_played"] ?? 0);
            assassinWins = (int)(props["assassin_wins"] ?? 0);
            innocentWins = (int)(props["innocent_wins"] ?? 0);
        }
    }
    
    private void SetupVoiceManager()
    {
        if (voiceManager == null)
            voiceManager = FindObjectOfType<LobbyVoiceManager>();
    }
    
    
    private void StartNewRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Usar el PhotonLauncher para iniciar el juego
        var photonLauncher = FindObjectOfType<PhotonLauncher>();
        if (photonLauncher != null)
        {
            photonLauncher.StartGame();
        }
        else
        {
            // Fallback si no se encuentra PhotonLauncher
            var roomProperties = new Hashtable
            {
                { RoomKeys.ROOM_LEVEL, gameSceneName },
                { RoomKeys.PHASE, RoomKeys.Phase_Loading },
                { RoleManager.AssignedKey, false },
                { RoomKeys.ALIVE, true }
            };
            
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
    
    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    private void UpdateUI()
    {
        // Actualizar nombre de sala
        if (roomNameText != null && PhotonNetwork.CurrentRoom != null)
            roomNameText.text = $"Sala: {PhotonNetwork.CurrentRoom.Name}";
        
        // Actualizar contador de jugadores
        if (playerCountText != null)
            playerCountText.text = $"Jugadores: {PhotonNetwork.PlayerList.Length}/6";
        
        // Actualizar lista de jugadores
        UpdatePlayerList();
        
        // Actualizar botón de empezar
        if (startGameButton != null)
            startGameButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length >= 2;
        
        // Actualizar estadísticas
        UpdateRoundStats();
    }
    
    private void UpdatePlayerList()
    {
        // Limpiar lista actual
        foreach (var item in playerListItems)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();
        
        // Crear nuevos items
        if (playerListItemPrefab != null && playerListParent != null)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                GameObject item = Instantiate(playerListItemPrefab, playerListParent);
                var text = item.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = player.NickName;
                    if (player.IsMasterClient)
                        text.text += " (Host)";
                }
                
                playerListItems.Add(item);
            }
        }
    }
    
    private void UpdateRoundStats()
    {
        if (roundStatsText != null)
        {
            roundStatsText.text = $"Rondas: {roundsPlayed}\n\nAsesinos: {assassinWins}\n\nInocentes: {innocentWins}";
        }
    }
    
    // Photon Callbacks
    public override void OnJoinedRoom()
    {
        Debug.Log("[LobbyManager] Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);
        UpdateUI();
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("[LobbyManager] Conectado al servidor maestro");
        // Intentar unirse a la sala si no estamos en una
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("[LobbyManager] No hay sala activa, regresando al menú");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }
    }
    
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning($"[LobbyManager] Desconectado: {cause}");
        // Regresar al menú si nos desconectamos
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdateUI();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdateUI();
    }
    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        UpdateUI();
    }
    
    public override void OnLeftRoom()
    {
        // Regresar al menú principal
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomKeys.PHASE))
        {
            int phase = (int)propertiesThatChanged[RoomKeys.PHASE];
            if (phase == RoomKeys.Phase_Loading)
            {
                if (gameStatusText != null)
                    gameStatusText.text = "Iniciando nueva ronda...";
            }
        }
        
        // Recargar estadísticas si han cambiado
        if (propertiesThatChanged.ContainsKey("rounds_played") || 
            propertiesThatChanged.ContainsKey("assassin_wins") || 
            propertiesThatChanged.ContainsKey("innocent_wins"))
        {
            LoadRoomStats();
            UpdateRoundStats();
        }
    }
    
    // Métodos públicos para actualizar estadísticas
    public void OnRoundEnded(string winner)
    {
        roundsPlayed++;
        if (winner == "ASSASSIN")
            assassinWins++;
        else if (winner == "INNOCENTS")
            innocentWins++;
            
        UpdateRoundStats();
    }
    
    private void OnDestroy()
    {
        // Cleanup si es necesario
    }
}
