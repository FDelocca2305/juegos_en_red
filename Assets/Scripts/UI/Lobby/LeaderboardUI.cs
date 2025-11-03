using LootLocker.Requests;
using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "wins_assasins_round";
    [SerializeField] int count = 10;
    [SerializeField] TMP_Text tableText;
    [SerializeField] string leaderboardKeyInnocents = "wins_innocent_round";
    [SerializeField] TMP_Text tableInnocentsText;


    public void Refresh()
    {
        if (!LootLockerBootsStrap.SessionStarted)
        {
            tableText.text = "Logging assasins leaderboard...";
            tableInnocentsText.text = "Logging innocent leaderboard...";
            return;
        }
        
        LeaderboardService.GetLeaderboardScore(leaderboardKey, count, (text) =>
        {
            tableText.text = text;

        });
        LeaderboardService.GetLeaderboardScore(leaderboardKeyInnocents, count, (text) =>
        {
            tableInnocentsText.text = text;

        });
    }
}
