using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Instancia estática única global (Patrón Singleton)
    public static GameManager Instance { get; private set; }

    [Header("Player & Interaction Systems")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;

    [Header("Core Gameplay Managers")]
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private QuestManager questManager;

    [Header("Services & UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager;

    // Propiedades públicas de solo lectura para acceder a cada sistema
    public PlayerMovement PlayerMovement => playerMovement;
    public Inventory Inventory => inventory;
    public PlayerInteraction PlayerInteraction => playerInteraction;
    public ThirdPersonCamera ThirdPersonCamera => thirdPersonCamera;
    public CombatSystem CombatSystem => combatSystem;
    public DialogueManager DialogueManager => dialogueManager;
    public QuestManager QuestManager => questManager;
    public UIManager UIManager => uiManager;
    public AudioManager AudioManager => audioManager;

    private void Awake()
    {
        // Implementación del Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[GameManager] Ya existe una instancia en {Instance.gameObject.name}. Destruyendo duplicado en {gameObject.name}.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Opcional: Descomenta la siguiente línea si deseas que el GameManager persista entre escenas
        // DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // Liberar la referencia de la instancia al destruir el objeto
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
