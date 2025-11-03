using LootLocker.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static void SubmitScore(int score, string leaderboardKey, System.Action<bool> onDone = null)
    {
        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fail to score");
                onDone?.Invoke(false);
                return;
            }
            Debug.Log("Send it score");
            onDone?.Invoke(true);

        });
    }

    public static void GetLeaderboardScore(string leaderboardKey, int count, Action<string> changeText)
    {
        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                changeText?.Invoke("Error");
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank Name             Score");
            sb.AppendLine("---------------------------");

            var items = response.items;
            if(items ==  null || items.Length == 0)
            {
                sb.AppendLine("Not registered leaderboard");
            }
            else
            {
                foreach(var item in items)
                {
                    string name = string.IsNullOrEmpty(item.player.name) ? "Player " + item.player.id : item.player.name;
                    sb.AppendLine($"{item.rank,4} {name,-16} {item.score,6}");
                }
            }
            changeText?.Invoke(sb.ToString());
        });
    }
}
