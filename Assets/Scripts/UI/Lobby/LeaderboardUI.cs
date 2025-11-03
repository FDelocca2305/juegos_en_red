using LootLocker.Requests;
using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "wins_assasins_round";
    [SerializeField] int count = 10;
    [SerializeField] TMP_Text tableText;

    public void Refresh()
    {
        if (!LootLockerBootsStrap.SessionStarted)
        {
            tableText.text = "Logging...";
            return;
        }
        
        LeaderboardService.GetLeaderboardScore(leaderboardKey, count, (text) =>
        {
            tableText.text = text;
        });

    }
}
