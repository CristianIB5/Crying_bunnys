using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FlashlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Referencia al inventario del jugador.")]
    public Inventory inventory;
    
    [Tooltip("El objeto de luz de la linterna (hijo del jugador o cámara) que se activará.")]
    public GameObject flashlightObject;

    [Header("Settings")]
    [Tooltip("El ID del objeto en el inventario que activa la linterna.")]
    public string flashlightItemId = "Linterna";

    private bool hasFlashlight = false;
    private bool isFlashlightOn = false;

    void Start()
    {
        // Si no se asignó, buscar el inventario en el objeto actual o padre
        if (inventory == null)
        {
            inventory = GetComponentInParent<Inventory>();
        }

        // Asegurar que la linterna inicie apagada al comenzar
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }
    }

    void Update()
    {
        if (inventory == null) return;

        // Comprobar constantemente si tenemos el objeto en el inventario
        bool checkInventory = inventory.HasItem(flashlightItemId);

        // Si cambia el estado de posesión de la linterna
        if (checkInventory != hasFlashlight)
        {
            hasFlashlight = checkInventory;

            // Si nos quitan la linterna del inventario, la apagamos automáticamente
            if (!hasFlashlight && flashlightObject != null)
            {
                isFlashlightOn = false;
                flashlightObject.SetActive(false);
            }
        }

        // Si el jugador posee la linterna y pulsa la tecla F, se enciende/apaga
        if (hasFlashlight && GetTogglePressed())
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if (flashlightObject != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightObject.SetActive(isFlashlightOn);
            Debug.Log($"[Linterna] {(isFlashlightOn ? "Encendida" : "Apagada")}");
        }
    }

    private bool GetTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }
}
