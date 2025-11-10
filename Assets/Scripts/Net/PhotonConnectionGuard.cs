using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)]
public class PhotonConnectionGuard : MonoBehaviourPunCallbacks
{
    public static PhotonConnectionGuard I { get; private set; }

    [Header("UI")]
    [SerializeField] private ConnectionErrorPopup popupPrefab;
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private bool pauseGameOnPopup = true;

    private ConnectionErrorPopup _popup;
    private bool _showing;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        EnsurePopup();
    }

    void EnsurePopup()
    {
        if (_popup) return;
        if (popupPrefab) _popup = Instantiate(popupPrefab);
        DontDestroyOnLoad(_popup.gameObject);
        _popup.Hide();
    }
    
    void ShowError(string title, string body)
    {
        if (_showing) return;
        _showing = true;
        EnsurePopup();

        if (pauseGameOnPopup) Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;

        _popup.Show(title, body, () => StartCoroutine(GoToMenu()));
    }

    IEnumerator GoToMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(false);
            float t = Time.realtimeSinceStartup + 2f;
            while (PhotonNetwork.InRoom && Time.realtimeSinceStartup < t) yield return null;
        }
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            float t = Time.realtimeSinceStartup + 2f;
            while (PhotonNetwork.IsConnected && Time.realtimeSinceStartup < t) yield return null;
        }

        if (pauseGameOnPopup) Time.timeScale = 1f;
        _showing = false;
        SceneManager.LoadScene(menuSceneName);
    }
    
    public override void OnDisconnected(DisconnectCause cause)
        => ShowError("Connection Lost", Pretty(cause));

    public override void OnCreateRoomFailed(short returnCode, string message)
        => ShowError("Could not create room", $"{message} (#{returnCode})");

    public override void OnJoinRoomFailed(short returnCode, string message)
        => ShowError("Could not enter room", $"{message} (#{returnCode})");

    public override void OnJoinRandomFailed(short returnCode, string message)
        => ShowError("No rooms available", $"{message} (#{returnCode})");

    public override void OnCustomAuthenticationFailed(string debugMessage)
        => ShowError("Autentication Failed", debugMessage);
    
    string Pretty(DisconnectCause c) => c switch
    {
        DisconnectCause.ServerTimeout or DisconnectCause.ClientTimeout
            => "Connection time out.",
        DisconnectCause.MaxCcuReached => "Server full.",
        DisconnectCause.InvalidAuthentication => "Invalid credentials.",
        DisconnectCause.ExceptionOnConnect or DisconnectCause.DnsExceptionOnConnect
            => "Error on connection.",
        DisconnectCause.Exception => "Red error.",
        DisconnectCause.AuthenticationTicketExpired => "Session Expired.",
        _ => "Lost connection."
    };
}
