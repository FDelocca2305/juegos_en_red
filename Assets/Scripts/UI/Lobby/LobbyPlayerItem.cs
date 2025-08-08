using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text seatText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Image colorSwatch;
    [SerializeField] private TMP_Text roleText;

    public void SetData(Player p, int seat, Color color, bool ready, bool isMaster)
    {
        nameText.text = $"{p.NickName} ({p.ActorNumber})";
        seatText.text = $"Seat: {seat}";
        readyText.text = ready ? "Ready" : "Not Ready";
        roleText.text = isMaster ? "Host" : "";
        colorSwatch.color = color;
    }
}
