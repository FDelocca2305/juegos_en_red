using UI.Gameplay;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class MenuServiceInstaller : MonoBehaviour
{
    [SerializeField] private PhotonLauncher photonLauncher;

    [Header("Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private bool autoFindIfNull = true;

    private void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        if (autoFindIfNull)
        {
            if (!photonLauncher) photonLauncher = FindObjectOfType<PhotonLauncher>();
        }
        
        if (photonLauncher) ServiceLocator.Register<IPhotonLauncher>(photonLauncher);
    }

    private void OnDestroy()
    {
        if (photonLauncher) ServiceLocator.Deregister<IPhotonLauncher>(photonLauncher);
    }
}