using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("El radio en el que el jugador detecta los objetos interactuables.")]
    public float detectionRadius = 3f;
    [Tooltip("La capa física (Layer) donde se encuentran los objetos interactuables.")]
    public LayerMask interactableLayer;

    [Header("Optional UI Components (Canvas)")]
    [Tooltip("El contenedor del texto en la UI (Panel o Canvas) que se activa/desactiva.")]
    public GameObject uiPromptContainer;
    [Tooltip("Componente TextMeshPro opcional para mostrar los mensajes.")]
    public TMPro.TextMeshProUGUI promptTextTMPro;
    [Tooltip("Componente Text tradicional de Unity opcional.")]
    public Text promptTextLegacy;

    private Interactable currentInteractable;
    private string feedbackMessage = "";
    private float feedbackTimer = 0f;

    // Método público para mostrar un mensaje temporal en la interfaz
    public void ShowFeedback(string message, float duration = 2.5f)
    {
        feedbackMessage = message;
        feedbackTimer = duration;
    }

    void Update()
    {
        // Administrar el temporizador del mensaje de feedback
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
        }
        else
        {
            feedbackMessage = "";
        }

        FindInteractable();

        // Lógica de visualización y ejecución de la interacción
        if (currentInteractable != null)
        {
            // Mostrar cómo interactuar: "Presiona E para [promptMessage]"
            ShowPrompt($"Presiona E para {currentInteractable.promptMessage}");

            if (GetInteractPressed())
            {
                currentInteractable.Interact(gameObject);
                currentInteractable = null;
            }
        }
        else
        {
            // Si no hay objeto al frente pero hay un mensaje de feedback activo, mostrarlo
            if (!string.IsNullOrEmpty(feedbackMessage))
            {
                ShowPrompt(feedbackMessage);
            }
            else
            {
                HidePrompt();
            }
        }
    }

    private void FindInteractable()
    {
        // Buscar colisionadores interactuables dentro de un radio
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
        
        Interactable closestInteractable = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            Interactable interactable = col.GetComponent<Interactable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        currentInteractable = closestInteractable;
    }

    private void ShowPrompt(string message)
    {
        if (promptTextTMPro != null)
        {
            promptTextTMPro.text = message;
        }
        else if (promptTextLegacy != null)
        {
            promptTextLegacy.text = message;
        }

        if (uiPromptContainer != null)
        {
            uiPromptContainer.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (uiPromptContainer != null)
        {
            uiPromptContainer.SetActive(false);
        }
    }

    private bool GetInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    // Dibujar el rango de interacción en la vista de escena (Scene View)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Sistema GUI de respaldo: si no se asignaron referencias de UI,
    // dibuja un cuadro de texto en pantalla automáticamente. ¡Cero configuración inicial!
    private void OnGUI()
    {
        bool hasAssignUI = (uiPromptContainer != null || promptTextTMPro != null || promptTextLegacy != null);
        
        if (!hasAssignUI)
        {
            string messageToShow = "";

            if (currentInteractable != null)
            {
                messageToShow = $"Presiona E para {currentInteractable.promptMessage}";
            }
            else if (!string.IsNullOrEmpty(feedbackMessage))
            {
                messageToShow = feedbackMessage;
            }

            if (!string.IsNullOrEmpty(messageToShow))
            {
                // Centrado en la parte inferior de la pantalla
                Rect rect = new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 45);
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 18;
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                
                // Fondo oscuro transparente elegante
                GUI.Box(rect, messageToShow, style);
            }
        }
    }
}
