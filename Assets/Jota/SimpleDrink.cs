using UnityEngine;

public class SimpleDrink : MonoBehaviour
{
    public Augment augment; // Só guarda qual augment esta bebida dá

    // Opcional: adicionar um pequeno efeito visual
    void Update()
    {
        // Faz a bebida flutuar suavemente (opcional)
        transform.Rotate(0, 30 * Time.deltaTime, 0);
        transform.position += Vector3.up * Mathf.Sin(Time.time) * 0.001f;
    }
}