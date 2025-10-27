using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Configuration")]
public class AudioConfig : ScriptableObject
{
    [Header("Footstep Sounds")]
    public AudioClip walkFootstep;
    public AudioClip runFootstep;
    
    [Header("Weapon Sounds")]
    public AudioClip pistolShot;
    public AudioClip rifleShot;
    public AudioClip machinegunShot;
    
    [Header("Impact Sounds")]
    public AudioClip playerImpact;
    public AudioClip wallImpact;
    
    [Header("Other Sounds")]
    public AudioClip playerDeath;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    
    [Header("Proximity Settings")]
    public float footstepMaxDistance = 12f;
    public float weaponMaxDistance = 20f;
    public float impactMaxDistance = 15f;
}
