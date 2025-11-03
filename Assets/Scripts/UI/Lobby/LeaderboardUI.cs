using LootLocker.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "wins_assasins_round";
    [SerializeField] int count = 10;
    [SerializeField] TMPro.TextMeshPro tableText;

    public void Refresh()
    {
        if (!LootLockerBootsStrap.SessionStarted)
        {
            tableText.text = "Logging...";
            return;
        }
        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                tableText.text = "Error";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank Name             Score");
            sb.AppendLine("---------------------------");

            var items = response.items;
            if(items ==  null || items.Length == 0)
            {
                sb.AppendLine("Not registed leaderboard");
            }
            else
            {
                foreach(var item in items)
                {
                    string name = string.IsNullOrEmpty(item.player.name) ? "Player " + item.player.id : item.player.name;
                    sb.AppendLine($"{item.rank,4} {name,-16} {item.score,6}");
                }
            }
            tableText.text = sb.ToString();
        });
    }
    public void OnSubmitScoreTMP(TMPro.TMP_InputField scoreInput)
    {
        if (int.TryParse(scoreInput.text, out var score))
        {
            LeaderboardService.SubmitScore(score, leaderboardKey, _ => Refresh());
        }
    }
    public void OnSetNameTMP(TMPro.TMP_InputField nameInput)
    {
        PlayerNameHelper.SetPlayerName(nameInput.text);
    }
}
