using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Details")]
    [Tooltip("Acción que realiza el objeto. Ej: 'recoger llave', 'abrir puerta'.")]
    public string promptMessage = "interactuar";

    // Método abstracto que será implementado por cada tipo de objeto interactuable
    public abstract void Interact(GameObject player);
}
