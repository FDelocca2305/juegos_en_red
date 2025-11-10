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
    
    private int roundsPlayed = 0;
    private int assassinWins = 0;
    private int innocentWins = 0;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        
        InitializeLobby();
        SetupVoiceManager();
        LoadRoomStats();
        UpdateUI();
        
        Debug.Log("ARRANCANDO EN START LOBBY");
    }
    
    private void InitializeLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
        
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartNewRound);
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
        
        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(LeaveRoom);
        
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
        
        var photonLauncher = FindObjectOfType<PhotonLauncher>();
        if (photonLauncher != null)
        {
            photonLauncher.StartGame();
        }
        else
        {
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
        if (roomNameText != null && PhotonNetwork.CurrentRoom != null)
            roomNameText.text = $"Sala: {PhotonNetwork.CurrentRoom.Name}";
        
        if (playerCountText != null)
            playerCountText.text = $"Jugadores: {PhotonNetwork.PlayerList.Length}/6";
        
        UpdatePlayerList();

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length >= 2);
        
        UpdateRoundStats();
    }
    
    private void UpdatePlayerList()
    {
        foreach (var item in playerListItems)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();
        
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
    
    public override void OnJoinedRoom()
    {
        Debug.Log("[LobbyManager] Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);
        UpdateUI();
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("[LobbyManager] Conectado al servidor maestro");
    }
    
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning($"[LobbyManager] Desconectado: {cause}");
        Debug.Log("SE DESCONECTOO");
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
        Debug.Log("SE LEFTEO LA ROOM");
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
            
            //if (phase == RoomKeys.Phase_Lobby)
                //voiceManager.PrepareVoiceForLobby();
        }
        
        if (propertiesThatChanged.ContainsKey("rounds_played") || 
            propertiesThatChanged.ContainsKey("assassin_wins") || 
            propertiesThatChanged.ContainsKey("innocent_wins"))
        {
            LoadRoomStats();
            UpdateRoundStats();
        }
    }
}
