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
    [Tooltip("Velocidad de movimiento del jugador.")]
    public float speed = 6f;
    [Tooltip("Fuerza de gravedad aplicada al jugador.")]
    public float gravity = -9.81f;
    [Tooltip("Altura máxima del salto.")]
    public float jumpHeight = 1.5f;
    
    [Header("Rotation Settings")]
    [Tooltip("Tiempo de suavizado para la rotación del personaje.")]
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private Vector3 velocity;
    private bool isGrounded;

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

        // Si hay movimiento
        if (direction.magnitude >= 0.1f)
        {
            // Calcular el ángulo de rotación objetivo relativo a la rotación de la cámara
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            
            // Suavizar la rotación del jugador
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calcular la dirección final del movimiento
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
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
}
