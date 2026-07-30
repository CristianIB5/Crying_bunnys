using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("El componente CharacterController del jugador.")]
    public CharacterController controller;
    [Tooltip("La cámara para orientar el movimiento relativo a ella.")]
    public Transform cameraTransform;

    [Header("Movement Settings")]
    [Tooltip("Velocidad de caminata del jugador.")]
    public float speed = 6f;
    [Tooltip("Velocidad al correr (sprint) presionando Shift.")]
    public float sprintSpeed = 10f;
    [Tooltip("Fuerza de gravedad aplicada al jugador.")]
    public float gravity = -9.81f;
    [Tooltip("Altura máxima del salto.")]
    public float jumpHeight = 1.5f;

    [Header("Sprint Settings")]
    [Tooltip("Tiempo máximo continuo (en segundos) que el jugador puede correr.")]
    public float maxSprintTime = 5f;
    [Tooltip("Tiempo de recuperación (en segundos) para recargar el sprint de 0 a su capacidad máxima.")]
    public float sprintCooldownTime = 3f;
    
    [Header("Rotation Settings")]
    [Tooltip("Tiempo de suavizado para la rotación del personaje.")]
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Camera Alignment Settings")]
    [Tooltip("Si está activo, el personaje siempre rotará para mirar hacia donde apunta la cámara (eje Y).")]
    public bool alwaysFaceCamera = false;

    [Header("Animation Settings")]
    [Tooltip("El componente Animator para controlar las animaciones del jugador (se auto-detectará si se deja vacío).")]
    public Animator animator;
    [Tooltip("Nombre del parámetro booleano del Animator que controla la caminata.")]
    public string isWalkingParam = "isWalking";
    [Tooltip("Si es verdadero, la velocidad de la animación se multiplicará dinámicamente cuando el jugador esté corriendo.")]
    public bool adjustAnimationSpeed = true;

    private Vector3 velocity;
    private bool isGrounded;
    private float currentSprintTime;
    private float currentSpeed;
    private bool isSprinting;

    // Propiedades públicas para consultar el estado del sprint desde otros scripts (ej: UI de estamina)
    public float SprintStaminaPercent => maxSprintTime > 0 ? Mathf.Clamp01(currentSprintTime / maxSprintTime) : 0f;
    public bool IsSprinting => isSprinting;

    void Start()
    {
        // Si no se asignó el CharacterController, intenta obtenerlo automáticamente
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        // Si no se asignó la cámara, busca la cámara principal
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Auto-detectar Animator en el objeto actual o en los hijos
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Inicializar velocidades y tiempo de sprint
        currentSprintTime = maxSprintTime;
        currentSpeed = speed;
    }

    void Update()
    {
        // Comprobar si el jugador toca el suelo
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Fuerza hacia abajo constante para mantener al jugador pegado al suelo
        }

        // Obtener la entrada del movimiento
        Vector2 input = GetMovementInput();
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        // Lógica de sprint (correr)
        bool sprintInput = GetSprintHeld();
        bool isMoving = direction.magnitude >= 0.1f;

        if (sprintInput && isMoving && currentSprintTime > 0f)
        {
            isSprinting = true;
            currentSpeed = sprintSpeed;
            
            // Consumir estamina
            currentSprintTime -= Time.deltaTime;
            if (currentSprintTime < 0f) currentSprintTime = 0f;
        }
        else
        {
            isSprinting = false;
            currentSpeed = speed;

            // Regenerar estamina si no estamos corriendo
            if (currentSprintTime < maxSprintTime)
            {
                currentSprintTime += Time.deltaTime * (maxSprintTime / sprintCooldownTime);
                if (currentSprintTime > maxSprintTime) currentSprintTime = maxSprintTime;
            }
        }

        // Lógica de rotación y movimiento basada en la alineación de cámara
        if (alwaysFaceCamera)
        {
            // El jugador siempre rota para alinearse con la cámara horizontalmente
            float targetAngle = cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Mover en relación a la propia orientación local del jugador (strafe)
            if (isMoving)
            {
                Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }
        }
        else
        {
            // El jugador gira hacia la dirección en la que camina
            if (isMoving)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }
        }

        // Actualizar parámetros del Animator
        if (animator != null)
        {
            animator.SetBool(isWalkingParam, isMoving);

            if (adjustAnimationSpeed)
            {
                if (isMoving)
                {
                    // Si el jugador corre, acelera proporcionalmente la animación de caminata
                    animator.speed = isSprinting ? (sprintSpeed / speed) : 1f;
                }
                else
                {
                    animator.speed = 1f;
                }
            }
        }

        // Salto
        if (GetJumpPressed() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Aplicar la gravedad en el eje Y
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Helper para dar soporte tanto al sistema de Input antiguo como al nuevo de forma automática
    private Vector2 GetMovementInput()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
        }
        return input;
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    private bool GetJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private bool GetSprintHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }
}
