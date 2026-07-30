using UnityEngine;

public class TriggerSonido : MonoBehaviour
{
    private AudioSource sonido;

    void Start()
    {
        // Busca el Audio Source que le pusimos al cubo
        sonido = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entró al cubo es el jugador
        if (other.CompareTag("Player"))
        {
            sonido.Play();

            // Apaga el collider para que el sonido no se repita si el jugador regresa
            GetComponent<Collider>().enabled = false;
        }
    }
}