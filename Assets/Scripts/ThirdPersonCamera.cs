using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("El objetivo (jugador) al que la cámara seguirá.")]
    public Transform target;
    [Tooltip("Desplazamiento (offset) de la cámara respecto al jugador.")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    [Tooltip("Suavizado del movimiento de la cámara.")]
    public float smoothSpeed = 0.125f;

    [Header("Rotation Settings")]
    [Tooltip("Velocidad de rotación con el mouse.")]
    public float rotationSpeed = 2f;
    [Tooltip("Límite inferior para mirar hacia abajo.")]
    public float minY = -20f;
    [Tooltip("Límite superior para mirar hacia arriba.")]
    public float maxY = 60f;

    [Header("Collision Settings")]
    [Tooltip("Activa o desactiva la colisión de la cámara con el entorno (solo aplica en tercera persona).")]
    public bool enableCollision = true;
    [Tooltip("Qué capas físicas bloquearán la cámara (usualmente todo excepto el Jugador y Triggers).")]
    public LayerMask collisionLayers = ~0; // Por defecto colisiona con TODO
    [Tooltip("El radio de la esfera de colisión de la cámara para detectar paredes.")]
    public float cameraRadius = 0.2f;
    [Tooltip("Distancia mínima permitida al jugador en caso de colisión estrecha.")]
    public float minDistance = 0.5f;

    [Header("First Person Settings")]
    [Tooltip("Alterna el modo de cámara actual (Primera Persona / Tercera Persona).")]
    public bool isFirstPerson = false;
    [Tooltip("Altura de la vista de ojos en Primera Persona.")]
    public float firstPersonHeight = 1.6f;
    [Tooltip("Desplazamiento horizontal/frontal de la cámara para evitar ver el interior de la cabeza.")]
    public float firstPersonForwardOffset = 0.15f;

    private float currentX = 0f;
    private float currentY = 0f;
    private bool originalAlwaysFaceCamera;
    private PlayerMovement playerMovement;

    void Start()
    {
        // Ocultar y bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializar los ángulos actuales de la cámara basados en su rotación inicial
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Obtener el script PlayerMovement y guardar su configuración original
        if (target != null)
        {
            playerMovement = target.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                originalAlwaysFaceCamera = playerMovement.alwaysFaceCamera;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Detectar si se cambia de cámara con la tecla V
        if (GetToggleViewPressed())
        {
            isFirstPerson = !isFirstPerson;
        }

        // Obtener la entrada del mouse
        Vector2 mouseInput = GetMouseInput();
        currentX += mouseInput.x * rotationSpeed;
        currentY -= mouseInput.y * rotationSpeed;

        // Limitar la rotación vertical para evitar que la cámara dé la vuelta completa
        currentY = Mathf.Clamp(currentY, minY, maxY);

        if (isFirstPerson)
        {
            // Forzar el modo de alineación en el jugador para permitir strafe
            if (playerMovement != null)
            {
                playerMovement.alwaysFaceCamera = true;
            }

            // Rotar la cámara basada en la entrada del ratón
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            transform.rotation = rotation;

            // Posicionar en los ojos del jugador
            Vector3 firstPersonPosition = target.position + Vector3.up * firstPersonHeight + transform.forward * firstPersonForwardOffset;
            transform.position = firstPersonPosition;

            // Hacer que el cuerpo del jugador rote horizontalmente con el ratón
            target.rotation = Quaternion.Euler(0f, currentX, 0f);
        }
        else
        {
            // Restaurar el comportamiento de alineación original del jugador
            if (playerMovement != null)
            {
                playerMovement.alwaysFaceCamera = originalAlwaysFaceCamera;
            }

            // Calcular la rotación y la posición objetivo base de la cámara (sin colisión)
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 defaultPosition = target.position + rotation * offset;
            Vector3 targetPosition = defaultPosition;

            // Lógica de colisión de la cámara
            if (enableCollision)
            {
                // El origen es la cabeza/pecho aproximado del jugador
                Vector3 raycastOrigin = target.position + Vector3.up * (offset.y * 0.75f);
                Vector3 raycastDirection = (defaultPosition - raycastOrigin).normalized;

                // Comenzar el cast un poco alejado del centro del jugador para evitar colisionar con su propio cuerpo
                float startOffset = 0.6f;
                Vector3 startPoint = raycastOrigin + raycastDirection * startOffset;
                float maxDistance = Vector3.Distance(startPoint, defaultPosition);

                if (maxDistance > 0)
                {
                    RaycastHit hit;
                    // Usamos SphereCast para tener una colisión más realista y evitar atravesar esquinas
                    if (Physics.SphereCast(startPoint, cameraRadius, raycastDirection, out hit, maxDistance, collisionLayers, QueryTriggerInteraction.Ignore))
                    {
                        // Restamos un pequeño margen de 0.1f al punto de impacto para que la cámara no se pegue a la pared
                        float newDistance = Mathf.Max(hit.distance + startOffset - 0.1f, minDistance);
                        targetPosition = raycastOrigin + raycastDirection * newDistance;
                    }
                }
            }

            // Interpolar suavemente entre la posición actual y la objetivo calculada
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

            // Hacer que la cámara mire hacia el jugador (un poco por encima de su base)
            transform.LookAt(target.position + Vector3.up * offset.y * 0.5f);
        }
    }

    // Helper para dar soporte automático a ambos sistemas de Input
    private Vector2 GetMouseInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            // Multiplicamos por un factor de escala para que se comporte de forma similar al Input Manager antiguo
            return Mouse.current.delta.ReadValue() * 0.05f;
        }
        return Vector2.zero;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }

    private bool GetToggleViewPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.V);
#endif
    }
}
