using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button createBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        createBtn.onClick.AddListener(OnCreateClicked);
        joinBtn.onClick.AddListener(OnJoinClicked);
        quitBtn.onClick.AddListener(() => Application.Quit());

        StartCoroutine(WaitForPhoton());
    }

    private IEnumerator WaitForPhoton()
    {
        while (!PhotonBootstrap.Connected)
        {
            statusText.text = "Connecting...";
            yield return null;
        }
        statusText.text = "Connected";
        createBtn.interactable = joinBtn.interactable = true;
    }

    private void OnCreateClicked()
    {
        var code = codeInput.text;
        if (!RoomCodeUtil.IsValid(code))
        {
            code = RoomCodeUtil.Generate6Digits();
            codeInput.text = code;
        }

        var options = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = false,
            CleanupCacheOnLeave = true
        };

        PhotonNetwork.CreateRoom(code, options, TypedLobby.Default);
        statusText.text = $"Creating Room {code}...";
    }

    private void OnJoinClicked()
    {
        var code = codeInput.text;
        if (!RoomCodeUtil.IsValid(code))
        {
            statusText.text = "Invalid Code. Must be 6 numeric digits.";
            return;
        }
        PhotonNetwork.JoinRoom(code);
        statusText.text = $"Joining room with code {code}...";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (returnCode == ErrorCode.GameIdAlreadyExists)
        {
            var newCode = RoomCodeUtil.Generate6Digits();
            codeInput.text = newCode;
            statusText.text = $"Error. Testing {newCode}...";
            OnCreateClicked();
            return;
        }
        statusText.text = $"On create failed: {message}";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"On join failed: {message}";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Room Joined. Loading lobby...";
        PhotonNetwork.NickName = nicknameInput.text;
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
