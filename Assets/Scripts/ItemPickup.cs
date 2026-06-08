using UnityEngine;

public class ItemPickup : Interactable
{
    [Header("Item Settings")]
    [Tooltip("El identificador único de este objeto. Ej: 'Llave'")]
    public string itemId = "Llave";

    // Constructor implícito que cambia el prompt por defecto a 'recoger'
    private void Reset()
    {
        promptMessage = "recoger " + itemId.ToLower();
    }

    public override void Interact(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            // Añadir al inventario
            inventory.AddItem(itemId);

            // Destruir el objeto en el mundo
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("El jugador no tiene un componente Inventory adjunto.");
        }
    }
}
