using Photon.Pun;
using UnityEngine;

public class AudioProximityController : MonoBehaviour
{
    private AudioSource audioSource;
    private Transform localListener;
    private float maxDistance;
    private float originalVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            originalVolume = audioSource.volume;
        }
    }

    private void Start()
    {
        // Buscar la cámara local (listener)
        var localCam = FindLocalCamera();
        if (localCam != null)
        {
            localListener = localCam.transform;
        }

        if (localListener == null)
        {
            Debug.LogWarning("No se encontró cámara local para AudioProximityController.");
            enabled = false;
        }
    }

    public void Initialize(float maxDist)
    {
        maxDistance = maxDist;
    }

    private void Update()
    {
        if (localListener == null || audioSource == null) return;

        float distance = Vector3.Distance(transform.position, localListener.position);
        
        if (distance <= maxDistance)
        {
            // Calcular volumen basado en la distancia
            float volumeMultiplier = Mathf.Clamp01(1 - (distance / maxDistance));
            audioSource.volume = originalVolume * volumeMultiplier;
            audioSource.enabled = true;
        }
        else
        {
            // Si está muy lejos, desactivar el audio
            audioSource.enabled = false;
        }
    }

    private Camera FindLocalCamera()
    {
        // Buscar la cámara principal activa
        foreach (var cam in Camera.allCameras)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy)
            {
                // Verificar si es la cámara del jugador local
                var photonView = cam.GetComponentInParent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    return cam;
                }
            }
        }

        // Fallback a Camera.main
        return Camera.main;
    }
}
