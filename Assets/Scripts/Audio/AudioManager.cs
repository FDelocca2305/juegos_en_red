using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AudioManager : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
        public float volume = 1f;
        public float pitch = 1f;
        public bool is3D = true;
        public float maxDistance = 15f;
    }

    [Header("Audio Settings")]
    [SerializeField] private List<AudioClipData> audioClips = new List<AudioClipData>();
    
    private AudioSource localAudioSource;
    
    private Dictionary<string, AudioClipData> audioClipDict = new Dictionary<string, AudioClipData>();
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Solo permitir una instancia por escena
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        InitializeAudioClips();
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void InitializeAudioClips()
    {
        audioClipDict.Clear();
        foreach (var clipData in audioClips)
        {
            if (!string.IsNullOrEmpty(clipData.name) && clipData.clip != null)
            {
                audioClipDict[clipData.name] = clipData;
            }
        }

        // Crear AudioSource local si no existe
        if (localAudioSource == null)
        {
            localAudioSource = gameObject.AddComponent<AudioSource>();
            localAudioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Reproduce un sonido solo localmente (solo lo escucha el jugador que lo ejecuta)
    /// </summary>
    public void PlayLocalSound(string soundName)
    {
        if (!audioClipDict.TryGetValue(soundName, out AudioClipData clipData))
        {
            Debug.LogWarning($"Audio clip '{soundName}' not found!");
            return;
        }

        PlayLocalSound(clipData);
    }

    /// <summary>
    /// Reproduce un sonido solo localmente con datos específicos
    /// </summary>
    public void PlayLocalSound(AudioClipData clipData)
    {
        if (localAudioSource != null && clipData.clip != null)
        {
            localAudioSource.PlayOneShot(clipData.clip, clipData.volume);
        }
    }

    /// <summary>
    /// Reproduce un sonido en la red que otros jugadores pueden escuchar según proximidad
    /// </summary>
    public void PlayNetworkSound(string soundName, Vector3 position)
    {
        if (!audioClipDict.TryGetValue(soundName, out AudioClipData clipData))
        {
            Debug.LogWarning($"Audio clip '{soundName}' not found!");
            return;
        }

        PlayNetworkSound(clipData, position);
    }

    /// <summary>
    /// Reproduce un sonido en la red con datos específicos
    /// </summary>
    public void PlayNetworkSound(AudioClipData clipData, Vector3 position)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPC_PlayNetworkSound), RpcTarget.All, 
                clipData.name, position.x, position.y, position.z);
        }
    }

    /// <summary>
    /// Reproduce un sonido en la red desde una posición específica
    /// </summary>
    public void PlayNetworkSoundAtPosition(string soundName, Vector3 position)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PlayNetworkSound(soundName, position);
        }
    }

    [PunRPC]
    private void RPC_PlayNetworkSound(string soundName, float x, float y, float z)
    {
        if (!audioClipDict.TryGetValue(soundName, out AudioClipData clipData))
        {
            Debug.LogWarning($"Audio clip '{soundName}' not found!");
            return;
        }

        Vector3 soundPosition = new Vector3(x, y, z);
        
        // Si es el jugador local, reproducir localmente también
        if (photonView.IsMine)
        {
            PlayLocalSound(clipData);
        }

        // Crear AudioSource temporal para el sonido de red
        CreateNetworkedAudioSource(clipData, soundPosition);
    }

    private void CreateNetworkedAudioSource(AudioClipData clipData, Vector3 position)
    {
        GameObject audioObject = new GameObject($"NetworkAudio_{clipData.name}");
        audioObject.transform.position = position;
        
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clipData.clip;
        audioSource.volume = clipData.volume;
        audioSource.pitch = clipData.pitch;
        audioSource.playOnAwake = false;
        
        if (clipData.is3D)
        {
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = clipData.maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D sound
        }

        // Aplicar proximidad si es necesario
        if (clipData.is3D)
        {
            audioObject.AddComponent<AudioProximityController>().Initialize(clipData.maxDistance);
        }

        audioSource.Play();
        
        // Destruir el objeto cuando termine de reproducir
        Destroy(audioObject, clipData.clip.length + 0.1f);
    }

    /// <summary>
    /// Agrega un nuevo clip de audio al diccionario
    /// </summary>
    public void AddAudioClip(AudioClipData clipData)
    {
        if (!string.IsNullOrEmpty(clipData.name) && clipData.clip != null)
        {
            audioClipDict[clipData.name] = clipData;
        }
    }

    /// <summary>
    /// Verifica si existe un clip de audio con el nombre dado
    /// </summary>
    public bool HasAudioClip(string soundName)
    {
        return audioClipDict.ContainsKey(soundName);
    }
}
