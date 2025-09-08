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
    [SerializeField] private ObjectivesTracker objectivesTracker;
    [SerializeField] private PieceSpawnGroup pieceSpawnGroup;
    
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
            if (!objectivesTracker) objectivesTracker = FindObjectOfType<ObjectivesTracker>();
            if (!photonLauncher) photonLauncher = FindObjectOfType<PhotonLauncher>();
            if (!playerSpawner) playerSpawner = FindObjectOfType<PlayerSpawner>();
            if (!pieceSpawnGroup) pieceSpawnGroup = FindObjectOfType<PieceSpawnGroup>();
        }
        
        if (spawnManager != null) ServiceLocator.Register<ISpawnManager>(spawnManager);
        if (gameplayUI) ServiceLocator.Register<IGameplayUI>(gameplayUI);
        if (localTeamProvider) ServiceLocator.Register<ILocalTeamProvider>(localTeamProvider);
        if (objectivesTracker) ServiceLocator.Register<IObjectivesTracker>(objectivesTracker);
        if (photonLauncher) ServiceLocator.Register<IPhotonLauncher>(photonLauncher);
        if (playerSpawner) ServiceLocator.Register<IPlayerSpawner>(playerSpawner);
        if (pieceSpawnGroup) ServiceLocator.Register<IPieceSpawnProvider>(pieceSpawnGroup);
    }

    private void OnDestroy()
    {
        if (spawnManager != null) ServiceLocator.Deregister<ISpawnManager>(spawnManager);
        if (gameplayUI) ServiceLocator.Deregister<IGameplayUI>(gameplayUI);
        if (localTeamProvider) ServiceLocator.Deregister<ILocalTeamProvider>(localTeamProvider);
        if (objectivesTracker) ServiceLocator.Deregister<IObjectivesTracker>(objectivesTracker);
        if (photonLauncher) ServiceLocator.Deregister<IPhotonLauncher>(photonLauncher);
        if (playerSpawner) ServiceLocator.Deregister<IPlayerSpawner>(playerSpawner);
        if (pieceSpawnGroup) ServiceLocator.Deregister<IPieceSpawnProvider>(pieceSpawnGroup);
    }
}