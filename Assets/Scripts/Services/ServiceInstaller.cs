using UI.Gameplay;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ServiceInstaller : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private GameplayUIController gameplayUI;
    [SerializeField] private LocalTeamProvider localTeamProvider;
    [SerializeField] private PhotonLauncher photonLauncher;
    [SerializeField] private PlayerSpawner playerSpawner;
    
    [Header("Optional services")]
    [SerializeField] private ObjectiveService objectiveService;
    
    [Header("Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private bool autoFindIfNull = true;

    private void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        if (autoFindIfNull)
        {
            if (spawnManager == null) spawnManager = FindObjectOfType<SpawnManager>();
            if (!gameplayUI) gameplayUI = FindObjectOfType<GameplayUIController>();
            if (!localTeamProvider) localTeamProvider = FindObjectOfType<LocalTeamProvider>();
            if (!objectiveService) objectiveService = FindObjectOfType<ObjectiveService>();
            if (!photonLauncher) photonLauncher = FindObjectOfType<PhotonLauncher>();
            if (!playerSpawner) playerSpawner = FindObjectOfType<PlayerSpawner>();
        }
        
        if (spawnManager != null) ServiceLocator.Register<ISpawnManager>(spawnManager);
        if (gameplayUI) ServiceLocator.Register<IGameplayUI>(gameplayUI);
        if (localTeamProvider) ServiceLocator.Register<ILocalTeamProvider>(localTeamProvider);
        if (objectiveService) ServiceLocator.Register<IObjectiveService>(objectiveService);
        if (photonLauncher) ServiceLocator.Register<IPhotonLauncher>(photonLauncher);
        if (playerSpawner) ServiceLocator.Register<IPlayerSpawner>(playerSpawner);
    }

    private void OnDestroy()
    {
        if (spawnManager != null) ServiceLocator.Deregister<ISpawnManager>(spawnManager);
        if (gameplayUI) ServiceLocator.Deregister<IGameplayUI>(gameplayUI);
        if (localTeamProvider) ServiceLocator.Deregister<ILocalTeamProvider>(localTeamProvider);
        if (objectiveService) ServiceLocator.Deregister<IObjectiveService>(objectiveService);
        if (photonLauncher) ServiceLocator.Deregister<IPhotonLauncher>(photonLauncher);
        if (playerSpawner) ServiceLocator.Deregister<IPlayerSpawner>(playerSpawner);
    }
}