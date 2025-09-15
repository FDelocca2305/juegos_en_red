using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisconnectToMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private string menuSceneName = "MenuScene";

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[DisconnectToMenu] Disconnected: {cause} -> loading {menuSceneName}");
        
        try { ServiceLocator.Reset(); } catch {}
        
        SceneManager.LoadScene(menuSceneName);
    }
}