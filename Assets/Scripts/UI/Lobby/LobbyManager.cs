using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private Transform playersContainer;
    [SerializeField] private LobbyPlayerItem playerItemPrefab;
    [SerializeField] private Button readyBtn;
    [SerializeField] private Button cancelReadyBtn;
    [SerializeField] private Button leaveBtn;
    [SerializeField] private Button startBtn;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text infoText;

    private readonly Dictionary<int, LobbyPlayerItem> _items = new();

    private static readonly Color[] Palette =
    {
        new Color32(235, 87, 87, 255),
        new Color32(39, 174, 96, 255),
        new Color32(45, 156, 219, 255),
        new Color32(241, 196, 15, 255),
    };

    private void Start()
    {
        roomCodeText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
        readyBtn.onClick.AddListener(() => SetReady(true));
        cancelReadyBtn.onClick.AddListener(() => SetReady(false));
        leaveBtn.onClick.AddListener(() => PhotonNetwork.LeaveRoom());
        startBtn.onClick.AddListener(StartGame);

        BuildList();
        TryAssignSeatForLocalIfMaster();
        RefreshUI();
    }

    private void BuildList()
    {
        foreach (Transform t in playersContainer) Destroy(t.gameObject);
        _items.Clear();

        foreach (var p in PhotonNetwork.PlayerList.OrderBy(pl => pl.ActorNumber))
            AddOrUpdate(p);
    }

    private void AddOrUpdate(Player p)
    {
        if (!_items.TryGetValue(p.ActorNumber, out var item))
        {
            item = Instantiate(playerItemPrefab, playersContainer);
            _items[p.ActorNumber] = item;
        }

        int seat = GetSeatOf(p);
        var color = Palette[Mathf.Clamp(seat, 0, Palette.Length - 1)];

        bool ready = p.CustomProperties.TryGetValue(NetKeys.READY, out var r) && (bool)r;

        item.SetData(p, seat, color, ready, p.IsMasterClient);
    }

    private int GetSeatOf(Player p)
    {
        var key = NetKeys.SeatKey(p.ActorNumber);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out var v))
            return (int)v;
        
        var ordered = PhotonNetwork.PlayerList.OrderBy(pl => pl.ActorNumber).ToList();
        return Mathf.Clamp(ordered.IndexOf(p), 0, 3);
    }

    private void TryAssignSeatForLocalIfMaster()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        var used = new HashSet<int>();
        foreach (var pl in PhotonNetwork.PlayerList)
        {
            var key = NetKeys.SeatKey(pl.ActorNumber);
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out var v))
                used.Add((int)v);
        }

        foreach (var pl in PhotonNetwork.PlayerList)
        {
            var key = NetKeys.SeatKey(pl.ActorNumber);
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
            {
                int seat = Enumerable.Range(0, 4).First(i => !used.Contains(i));
                used.Add(seat);
                var ht = new Hashtable { { key, seat } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
            }
        }
    }

    private void SetReady(bool value)
    {
        var ht = new Hashtable { { NetKeys.READY, value } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
        RefreshUI();
    }

    private bool AllReady()
    {
        return PhotonNetwork.PlayerList.Length > 0 &&
               PhotonNetwork.PlayerList.All(p => p.CustomProperties.TryGetValue(NetKeys.READY, out var r) && (bool)r);
    }

    private void RefreshUI()
    {
        foreach (var p in PhotonNetwork.PlayerList) AddOrUpdate(p);

        bool amReady = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(NetKeys.READY, out var r) && (bool)r;
        readyBtn.gameObject.SetActive(!amReady);
        cancelReadyBtn.gameObject.SetActive(amReady);

        startBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startBtn.interactable = PhotonNetwork.IsMasterClient && AllReady();

        infoText.text = AllReady()
            ? (PhotonNetwork.IsMasterClient ? "All Ready. Can Start." : "Waiting Host...")
            : "Waiting All Ready...";
    }

    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient || !AllReady()) return;

        var ht = new Hashtable { { NetKeys.GAME_STARTED, true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TryAssignSeatForLocalIfMaster();
        AddOrUpdate(newPlayer);
        RefreshUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_items.TryGetValue(otherPlayer.ActorNumber, out var item))
        {
            Destroy(item.gameObject);
            _items.Remove(otherPlayer.ActorNumber);
        }
        RefreshUI();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        AddOrUpdate(targetPlayer);
        RefreshUI();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(NetKeys.GAME_STARTED))
            Debug.Log("Flag gameStarted changed");

        foreach (var p in PhotonNetwork.PlayerList) AddOrUpdate(p);
        RefreshUI();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MenuScene");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        TryAssignSeatForLocalIfMaster();
        RefreshUI();
    }
}