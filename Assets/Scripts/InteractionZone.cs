using UnityEngine;
using UnityEngine.Events;

public class InteractionZone : Interactable
{
    [Header("Requirement Settings")]
    [Tooltip("El ID del objeto que se necesita para activar esta zona. Ej: 'Llave'")]
    public string requiredItemId = "Llave";
    
    [Tooltip("Si es verdadero, el objeto se consumirá del inventario al usarse.")]
    public bool consumeItem = true;

    [Header("Feedback Messages")]
    [Tooltip("Mensaje que se muestra si la interacción tiene éxito.")]
    public string successMessage = "¡Puerta abierta!";
    
    [Tooltip("Mensaje que se muestra si falta el objeto requerido.")]
    public string missingItemMessage = "Está cerrado. Necesitas una Llave.";

    [Header("Events")]
    [Tooltip("Eventos que ocurrirán cuando el jugador tenga el objeto correcto.")]
    public UnityEvent onInteractSuccess;

    [Tooltip("Eventos opcionales si el jugador intenta interactuar sin el objeto.")]
    public UnityEvent onInteractFail;

    private bool isUnlocked = false;

    public override void Interact(GameObject player)
    {
        // Si ya está desbloqueado, no hacemos nada más
        if (isUnlocked) return;

        Inventory inventory = player.GetComponent<Inventory>();
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();

        if (inventory != null)
        {
            if (inventory.HasItem(requiredItemId))
            {
                isUnlocked = true;
                Debug.Log($"[Zona] Éxito: {successMessage}");

                // Remover el objeto si está marcado
                if (consumeItem)
                {
                    inventory.RemoveItem(requiredItemId);
                }

                // Mostrar mensaje de éxito en pantalla
                if (playerInteraction != null)
                {
                    playerInteraction.ShowFeedback(successMessage, 3.0f);
                }

                // Lanzar los eventos de éxito
                onInteractSuccess.Invoke();
            }
            else
            {
                Debug.Log($"[Zona] Bloqueado: {missingItemMessage}");

                // Mostrar mensaje de error en pantalla
                if (playerInteraction != null)
                {
                    playerInteraction.ShowFeedback(missingItemMessage, 3.0f);
                }

                // Lanzar los eventos de fallo
                onInteractFail.Invoke();
            }
        }
    }
}
