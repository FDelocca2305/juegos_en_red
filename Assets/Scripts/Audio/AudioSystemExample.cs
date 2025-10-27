using UnityEngine;

/// <summary>
/// Script de ejemplo que demuestra cómo usar el sistema de audio
/// </summary>
public class AudioSystemExample : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testFootstepKey = KeyCode.F;
    [SerializeField] private KeyCode testShootKey = KeyCode.G;
    [SerializeField] private KeyCode testImpactKey = KeyCode.H;
    [SerializeField] private KeyCode testDeathKey = KeyCode.J;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(testFootstepKey))
        {
            TestFootstepSound();
        }

        if (Input.GetKeyDown(testShootKey))
        {
            TestShootSound();
        }

        if (Input.GetKeyDown(testImpactKey))
        {
            TestImpactSound();
        }

        if (Input.GetKeyDown(testDeathKey))
        {
            TestDeathSound();
        }
    }

    private void TestFootstepSound()
    {
        Debug.Log("Probando sonido de paso...");
        audioManager.PlayNetworkSoundAtPosition("footstep_slow", transform.position);
    }

    private void TestShootSound()
    {
        Debug.Log("Probando sonido de disparo...");
        audioManager.PlayNetworkSoundAtPosition("shot_pistol", transform.position);
    }

    private void TestImpactSound()
    {
        Debug.Log("Probando sonido de impacto...");
        Vector3 testPosition = transform.position + Vector3.forward * 2f;
        audioManager.PlayNetworkSound("impact_wall", testPosition);
    }

    private void TestDeathSound()
    {
        Debug.Log("Probando sonido de muerte...");
        Vector3 testPosition = transform.position + Vector3.right * 2f;
        audioManager.PlayNetworkSound("player_death", testPosition);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Controles de Prueba de Audio:");
        GUILayout.Label($"F - Sonido de paso");
        GUILayout.Label($"G - Sonido de disparo");
        GUILayout.Label($"H - Sonido de impacto");
        GUILayout.Label($"J - Sonido de muerte");
        GUILayout.EndArea();
    }
}
