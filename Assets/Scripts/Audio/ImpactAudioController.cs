using Photon.Pun;
using UnityEngine;

public class ImpactAudioController : MonoBehaviourPunCallbacks
{
    [Header("Impact Settings")]
    [SerializeField] private string wallImpactSound = "impact_wall";
    [SerializeField] private string playerImpactSound = "impact_player";
    [SerializeField] private float maxDistance = 15f;
    
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
    }

    /// <summary>
    /// Reproduce sonido de impacto en pared/objeto
    /// </summary>
    public void PlayWallImpact(Vector3 position)
    {
        if (audioManager != null)
        {
            audioManager.PlayNetworkSound(wallImpactSound, position);
        }
    }

    /// <summary>
    /// Reproduce sonido de impacto en jugador
    /// </summary>
    public void PlayPlayerImpact(Vector3 position)
    {
        if (audioManager != null)
        {
            audioManager.PlayNetworkSound(playerImpactSound, position);
        }
    }

    /// <summary>
    /// Reproduce sonido de muerte del jugador
    /// </summary>
    public void PlayPlayerDeath(Vector3 position)
    {
        if (audioManager != null)
        {
            audioManager.PlayNetworkSound("player_death", position);
        }
    }

    /// <summary>
    /// Método para ser llamado desde RPC cuando un jugador muere
    /// </summary>
    [PunRPC]
    public void RPC_PlayPlayerDeath(string playerName, float x, float y, float z)
    {
        Vector3 deathPosition = new Vector3(x, y, z);
        PlayPlayerDeath(deathPosition);
    }

    /// <summary>
    /// Método para ser llamado desde RPC cuando hay impacto en pared
    /// </summary>
    [PunRPC]
    public void RPC_PlayWallImpact(float x, float y, float z)
    {
        Vector3 impactPosition = new Vector3(x, y, z);
        PlayWallImpact(impactPosition);
    }

    /// <summary>
    /// Método para ser llamado desde RPC cuando hay impacto en jugador
    /// </summary>
    [PunRPC]
    public void RPC_PlayPlayerImpact(float x, float y, float z)
    {
        Vector3 impactPosition = new Vector3(x, y, z);
        PlayPlayerImpact(impactPosition);
    }
}
