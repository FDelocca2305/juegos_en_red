using UI.Gameplay;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ServiceInstaller : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerShootController playerShootController;
    [SerializeField] private PlayerUIController playerUIController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private GameplayUIController gameplayUI;
    [SerializeField] private LocalTeamProvider localTeamProvider;
    
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
            if (playerController == null) playerController = FindObjectOfType<PlayerController>();
            if (playerShootController == null) playerShootController = FindObjectOfType<PlayerShootController>();
            if (playerUIController == null) playerUIController = FindObjectOfType<PlayerUIController>();
            if (playerInventory == null) playerInventory = FindObjectOfType<PlayerInventory>();
            if (spawnManager == null) spawnManager = FindObjectOfType<SpawnManager>();
            if (!gameplayUI) gameplayUI = FindObjectOfType<GameplayUIController>();
            if (!localTeamProvider) localTeamProvider = FindObjectOfType<LocalTeamProvider>();
            if (!objectiveService) objectiveService = FindObjectOfType<ObjectiveService>();
            if (gameplayUI) ServiceLocator.Register<IGameplayUI>(gameplayUI);
            if (localTeamProvider) ServiceLocator.Register<ILocalTeamProvider>(localTeamProvider);
            if (objectiveService) ServiceLocator.Register<IObjectiveService>(objectiveService);
        }

        if (playerController != null) ServiceLocator.Register<IPlayerController>(playerController);
        if (playerShootController != null) ServiceLocator.Register<IPlayerShootController>(playerShootController);
        if (playerUIController != null) ServiceLocator.Register<IPlayerUIController>(playerUIController);
        if (playerInventory != null) ServiceLocator.Register<IPlayerInventory>(playerInventory);
        if (spawnManager != null) ServiceLocator.Register<ISpawnManager>(spawnManager);
    }

    private void OnDestroy()
    {
        if (playerController != null) ServiceLocator.Deregister<IPlayerController>(playerController);
        if (playerShootController != null) ServiceLocator.Deregister<IPlayerShootController>(playerShootController);
        if (playerUIController != null) ServiceLocator.Deregister<IPlayerUIController>(playerUIController);
        if (playerInventory != null) ServiceLocator.Deregister<IPlayerInventory>(playerInventory);
        if (spawnManager != null) ServiceLocator.Deregister<ISpawnManager>(spawnManager);
        if (gameplayUI) ServiceLocator.Deregister<IGameplayUI>(gameplayUI);
        if (localTeamProvider) ServiceLocator.Deregister<ILocalTeamProvider>(localTeamProvider);
        if (objectiveService) ServiceLocator.Deregister<IObjectiveService>(objectiveService);
    }
}