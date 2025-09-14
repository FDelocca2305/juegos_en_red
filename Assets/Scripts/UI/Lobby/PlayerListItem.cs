using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    
    public void SetupPlayer(string playerName, bool isMasterClient)
    {
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }
    }
}
