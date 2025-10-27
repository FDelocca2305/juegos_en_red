using Photon.Pun;
using UnityEngine;

public class FootstepAudioController : MonoBehaviourPunCallbacks
{
    [Header("Footstep Settings")]
    [SerializeField] private string walkFootstepSound = "footstep_slow";
    [SerializeField] private string runFootstepSound = "footstep_fast";
    [SerializeField] private float walkStepInterval = 0.6f;
    [SerializeField] private float runStepInterval = 0.4f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    private float lastStepTime;
    private bool isMoving;
    private bool isRunning;
    private Vector3 lastPosition;
    private CharacterController characterController;
    private AudioManager audioManager;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioManager = AudioManager.Instance;
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        CheckMovement();
        
        if (isMoving && IsGrounded())
        {
            PlayFootstep();
        }
    }

    private void CheckMovement()
    {
        Vector3 currentPosition = transform.position;
        float movementSpeed = Vector3.Distance(currentPosition, lastPosition) / Time.deltaTime;
        
        isMoving = movementSpeed > 0.1f;
        isRunning = movementSpeed > 3f;
        
        lastPosition = currentPosition;
    }

    private bool IsGrounded()
    {
        if (characterController != null)
        {
            return characterController.isGrounded;
        }
        
        // Fallback: raycast hacia abajo
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void PlayFootstep()
    {
        float currentTime = Time.time;
        float stepInterval = isRunning ? runStepInterval : walkStepInterval;
        
        if (currentTime - lastStepTime >= stepInterval)
        {
            string soundName = isRunning ? runFootstepSound : walkFootstepSound;
            
            if (audioManager != null)
            {
                audioManager.PlayNetworkSoundAtPosition(soundName, transform.position);
            }
            
            lastStepTime = currentTime;
        }
    }

    /// <summary>
    /// Método público para reproducir un paso manualmente
    /// </summary>
    public void PlayManualFootstep(bool isRunningStep = false)
    {
        string soundName = isRunningStep ? runFootstepSound : walkFootstepSound;
        if (audioManager != null)
        {
            audioManager.PlayNetworkSoundAtPosition(soundName, transform.position);
        }
    }

    /// <summary>
    /// Configurar los intervalos de pasos
    /// </summary>
    public void SetStepIntervals(float walkInterval, float runInterval)
    {
        walkStepInterval = walkInterval;
        runStepInterval = runInterval;
    }

    /// <summary>
    /// Configurar los nombres de los sonidos
    /// </summary>
    public void SetFootstepSounds(string walkSound, string runSound)
    {
        walkFootstepSound = walkSound;
        runFootstepSound = runSound;
    }
}
