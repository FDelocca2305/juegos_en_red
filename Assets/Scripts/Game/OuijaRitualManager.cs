using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class OuijaRitualManager : MonoBehaviourPunCallbacks
{
    public static OuijaRitualManager I { get; private set; }
    void Awake(){ I = this; }

    [Header("Config")]
    [SerializeField] private int minParticipants = 2;
    [SerializeField] private int maxParticipants = 5;
    [SerializeField] private float lockDelay    = 2.0f;

    [Header("Refs")]
    [SerializeField] private OuijaBoardController board;

    readonly HashSet<int> _ready = new();
    Coroutine _lockRoutine;
    bool _ritualRunning;
    
    static bool TryToInt(object o, out int val){ try { val = System.Convert.ToInt32(o); return true; } catch { val = 0; return false; } }
    static bool RolesAssignedInRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room?.CustomProperties != null
               && room.CustomProperties.TryGetValue(RoleManager.AssignedKey, out var v)
               && v is bool b && b;
    }

    static Photon.Realtime.Player GetAssassinPlayer()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room?.CustomProperties != null &&
            room.CustomProperties.TryGetValue(RoomRoleKeys.ASSASSIN_ACTOR, out var v) &&
            TryToInt(v, out int assassinActor))
            return PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == assassinActor);

        return PhotonNetwork.PlayerList.FirstOrDefault(p =>
            p.CustomProperties != null &&
            p.CustomProperties.TryGetValue(RoleManager.RoleKey, out var r) &&
            TryToInt(r, out int ri) && ri == (int)RoleId.Assassin);
    }

    public void LocalPadSet(bool onPad)
    {
        photonView.RPC(nameof(RPC_SetReady), RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber, onPad);
    }

    [PunRPC]
    void RPC_SetReady(int actorNumber, bool ready, PhotonMessageInfo _)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ready) _ready.Add(actorNumber);
        else       _ready.Remove(actorNumber);

        Debug.Log($"[Ouija] ready={_ready.Count} running={_ritualRunning}");

        if (_ritualRunning) return;
        
        if (_ready.Count >= minParticipants)
        {
            if (_lockRoutine == null) _lockRoutine = StartCoroutine(LockThenStart());
        }
        else
        {
            if (_lockRoutine != null) { StopCoroutine(_lockRoutine); _lockRoutine = null; }
        }
    }

    IEnumerator LockThenStart()
    {
        double start = PhotonNetwork.Time + lockDelay;
        Debug.Log($"[Ouija] lock… starting at {start:F3}");

        while (PhotonNetwork.Time < start)
        {
            if (_ready.Count < minParticipants) { _lockRoutine = null; yield break; }
            yield return null;
        }

        var participants = _ready.Take(maxParticipants).ToArray();
        if (participants.Length < minParticipants) { _lockRoutine = null; yield break; }

        _ritualRunning = true;
        _lockRoutine = null;
        StartRitualFor(participants);
    }

    void StartRitualFor(int[] actorNumbers)
    {
        if (!RolesAssignedInRoom()) { _ritualRunning = false; return; }

        var assassin = GetAssassinPlayer();
        var assassinName = (assassin?.NickName ?? "UNKNOWN").ToUpper().Replace(" ", "");

        int n = Mathf.Min(actorNumbers.Length, assassinName.Length);
        var seq = new List<string>(n);
        for (int i = 0; i < n; i++) seq.Add(assassinName[i].ToString());

        double startTime = PhotonNetwork.Time + 1.0;
        Debug.Log("[Ouija] ritual START");
        if (board != null)
        {
            AudioManager.Instance?.PlayNetworkSoundAtPosition("ouija_ritual", board.transform.position);
        }
        board.photonView.RPC("RPC_StartRitual", RpcTarget.All, seq.ToArray(), startTime);

        _ready.Clear();
        Invoke(nameof(EnableNextRitual), 240f);
    }

    void EnableNextRitual(){ _ritualRunning = false; }
}
