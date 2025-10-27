using UnityEngine;

public class AudioInitializer : MonoBehaviour
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioConfig audioConfig;
    
    private void Start()
    {
        InitializeAudioManager();
    }

    private void InitializeAudioManager()
    {
        AudioManager audioManager = AudioManager.Instance;
        
        if (audioConfig == null)
        {
            Debug.LogWarning("AudioConfig no asignado. Configurando con valores por defecto.");
            ConfigureDefaultAudio(audioManager);
            return;
        }

        // Configurar sonidos de pasos
        if (audioConfig.walkFootstep != null)
        {
            var walkClip = new AudioManager.AudioClipData
            {
                name = "footstep_slow",
                clip = audioConfig.walkFootstep,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.footstepMaxDistance
            };
            audioManager.AddAudioClip(walkClip);
        }

        if (audioConfig.runFootstep != null)
        {
            var runClip = new AudioManager.AudioClipData
            {
                name = "footstep_fast",
                clip = audioConfig.runFootstep,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.footstepMaxDistance
            };
            audioManager.AddAudioClip(runClip);
        }

        // Configurar sonidos de armas
        if (audioConfig.pistolShot != null)
        {
            var pistolClip = new AudioManager.AudioClipData
            {
                name = "shot_pistol",
                clip = audioConfig.pistolShot,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.weaponMaxDistance
            };
            audioManager.AddAudioClip(pistolClip);
        }

        if (audioConfig.rifleShot != null)
        {
            var rifleClip = new AudioManager.AudioClipData
            {
                name = "shot_rifle",
                clip = audioConfig.rifleShot,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.weaponMaxDistance
            };
            audioManager.AddAudioClip(rifleClip);
        }

        if (audioConfig.machinegunShot != null)
        {
            var machinegunClip = new AudioManager.AudioClipData
            {
                name = "shot_machinegun",
                clip = audioConfig.machinegunShot,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.weaponMaxDistance
            };
            audioManager.AddAudioClip(machinegunClip);
        }

        // Configurar sonidos de impacto
        if (audioConfig.playerImpact != null)
        {
            var playerImpactClip = new AudioManager.AudioClipData
            {
                name = "impact_player",
                clip = audioConfig.playerImpact,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.impactMaxDistance
            };
            audioManager.AddAudioClip(playerImpactClip);
        }

        if (audioConfig.wallImpact != null)
        {
            var wallImpactClip = new AudioManager.AudioClipData
            {
                name = "impact_wall",
                clip = audioConfig.wallImpact,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.impactMaxDistance
            };
            audioManager.AddAudioClip(wallImpactClip);
        }

        // Configurar sonido de muerte
        if (audioConfig.playerDeath != null)
        {
            var deathClip = new AudioManager.AudioClipData
            {
                name = "player_death",
                clip = audioConfig.playerDeath,
                volume = audioConfig.sfxVolume,
                is3D = true,
                maxDistance = audioConfig.impactMaxDistance
            };
            audioManager.AddAudioClip(deathClip);
        }

        Debug.Log("AudioManager inicializado con configuración completa.");
    }

    private void ConfigureDefaultAudio(AudioManager audioManager)
    {
        // Cargar sonidos desde Resources si no hay configuración
        LoadAudioFromResources(audioManager);
    }

    private void LoadAudioFromResources(AudioManager audioManager)
    {
        // Intentar cargar sonidos desde la carpeta Audio/SFX
        var walkFootstep = Resources.Load<AudioClip>("Audio/SFX/footstep slow");
        var runFootstep = Resources.Load<AudioClip>("Audio/SFX/footstep fast");
        var pistolShot = Resources.Load<AudioClip>("Audio/SFX/shot (pistol)");
        var rifleShot = Resources.Load<AudioClip>("Audio/SFX/shot (rifle)");
        var machinegunShot = Resources.Load<AudioClip>("Audio/SFX/shot (machinegun)");
        var playerImpact = Resources.Load<AudioClip>("Audio/SFX/impact (player)");
        var wallImpact = Resources.Load<AudioClip>("Audio/SFX/impact (hard)");
        var playerDeath = Resources.Load<AudioClip>("Audio/SFX/player death");

        // Configurar sonidos encontrados
        if (walkFootstep != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "footstep_slow",
                clip = walkFootstep,
                volume = 0.7f,
                is3D = true,
                maxDistance = 12f
            };
            audioManager.AddAudioClip(clip);
        }

        if (runFootstep != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "footstep_fast",
                clip = runFootstep,
                volume = 0.8f,
                is3D = true,
                maxDistance = 12f
            };
            audioManager.AddAudioClip(clip);
        }

        if (pistolShot != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "shot_pistol",
                clip = pistolShot,
                volume = 0.8f,
                is3D = true,
                maxDistance = 20f
            };
            audioManager.AddAudioClip(clip);
        }

        if (rifleShot != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "shot_rifle",
                clip = rifleShot,
                volume = 0.9f,
                is3D = true,
                maxDistance = 25f
            };
            audioManager.AddAudioClip(clip);
        }

        if (machinegunShot != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "shot_machinegun",
                clip = machinegunShot,
                volume = 0.9f,
                is3D = true,
                maxDistance = 25f
            };
            audioManager.AddAudioClip(clip);
        }

        if (playerImpact != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "impact_player",
                clip = playerImpact,
                volume = 0.8f,
                is3D = true,
                maxDistance = 15f
            };
            audioManager.AddAudioClip(clip);
        }

        if (wallImpact != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "impact_wall",
                clip = wallImpact,
                volume = 0.6f,
                is3D = true,
                maxDistance = 10f
            };
            audioManager.AddAudioClip(clip);
        }

        if (playerDeath != null)
        {
            var clip = new AudioManager.AudioClipData
            {
                name = "player_death",
                clip = playerDeath,
                volume = 0.9f,
                is3D = true,
                maxDistance = 20f
            };
            audioManager.AddAudioClip(clip);
        }

        Debug.Log("AudioManager inicializado con sonidos desde Resources.");
    }
}
