using Photon.Voice.PUN;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class VoiceBootstrap : MonoBehaviour
{
    [SerializeField] private bool dontDestroyOnLoad = true;

    void Awake()
    {
        var all = FindObjectsOfType<PunVoiceClient>(true);

        if (all.Length == 0)
        {
            var go = new GameObject("PunVoiceClient");
            var pvc = go.AddComponent<PunVoiceClient>();
            Configure(pvc);

            if (dontDestroyOnLoad) DontDestroyOnLoad(go);
            Destroy(gameObject);
            return;
        }
        
        var keep = all[0];
        Configure(keep);
        if (dontDestroyOnLoad) DontDestroyOnLoad(keep.gameObject);
        
        for (int i = 1; i < all.Length; i++)
        {
            var dup = all[i];
            try
            {
                if (dup.Client != null)
                {
                    if (dup.Client.InRoom) dup.Client.OpLeaveRoom(false);
                    if (dup.Client.IsConnected) dup.Disconnect();
                }
            }
            catch { }

            Debug.LogWarning($"[Voice] Duplicate PunVoiceClient destroyed: {dup.name}", dup);
            Destroy(dup.gameObject);
        }

        Destroy(gameObject);
    }

    private static void Configure(PunVoiceClient pvc)
    {
        pvc.UsePunAppSettings      = true;
        pvc.AutoConnectAndJoin     = true;
    }
}