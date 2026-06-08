using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // Lista para almacenar los nombres o identificadores de los objetos
    private List<string> items = new List<string>();

    // Añade un objeto al inventario
    public void AddItem(string itemId)
    {
        if (!items.Contains(itemId))
        {
            items.Add(itemId);
            Debug.Log($"[Inventario] Objeto añadido: {itemId}");
        }
    }

    // Comprueba si el jugador tiene el objeto
    public bool HasItem(string itemId)
    {
        return items.Contains(itemId);
    }

    // Remueve un objeto del inventario (al usarlo, por ejemplo)
    public bool RemoveItem(string itemId)
    {
        if (items.Contains(itemId))
        {
            items.Remove(itemId);
            Debug.Log($"[Inventario] Objeto usado/removido: {itemId}");
            return true;
        }
        return false;
    }

    // Devuelve una lista de los objetos actuales
    public List<string> GetItems()
    {
        return new List<string>(items);
    }
}
