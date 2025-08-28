using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhotonLauncher : MonoBehaviourPunCallbacks, IPhotonLauncher
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject createRoomScreen;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private GameObject roomScreen;
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private GameObject errorScreen;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private GameObject roomBrowserScreen;
    [SerializeField] private RoomButton roomButton;
    [SerializeField] private TMP_Text playerNameLabel;
    [SerializeField] private GameObject nameInputScreen;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private string levelToPlay;
    [SerializeField] private GameObject startButton;
    
    private List<RoomButton> roomButtons = new List<RoomButton>();
    private List<TMP_Text> playerLabels = new List<TMP_Text>();
    private bool hasSetNickname;

    private void Awake()
    {
        if (PhotonNetwork.AuthValues == null || string.IsNullOrEmpty(PhotonNetwork.AuthValues.UserId))
            PhotonNetwork.AuthValues = new AuthenticationValues(System.Guid.NewGuid().ToString());
    }

    private void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        PhotonNetwork.ConnectUsingSettings();
    }

    private void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in playerLabels)
        {
            Destroy(player.gameObject);
        }
        playerLabels.Clear();

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text playerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            playerLabel.text = players[i].NickName;
            playerLabel.gameObject.SetActive(true);
            
            playerLabels.Add(playerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        TMP_Text playerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        playerLabel.text = newPlayer.NickName;
        playerLabel.gameObject.SetActive(true);
            
        playerLabels.Add(playerLabel);
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNickname)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 6
            };
            PhotonNetwork.CreateRoom(roomNameInput.text, options);
            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomName.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room...";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in roomButtons)
        {
            Destroy(rb.gameObject);
        }
        
        roomButtons.Clear();
        
        roomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton button = Instantiate(roomButton, roomButton.transform.parent);
                button.SetButtonDetails(roomList[i]);
                button.gameObject.SetActive(true);
                
                roomButtons.Add(button);
            }
        }
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        
        CloseMenus();
        loadingText.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }

    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            
            PlayerPrefs.SetString("playerName", nameInput.text);
            
            CloseMenus();
            menuButtons.SetActive(true);

            hasSetNickname = true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToPlay);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
}
