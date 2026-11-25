using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Celda : MonoBehaviour
{
    [Tooltip("Altura extra sobre la superficie de la casilla para que la línea no se hunda")]
    [SerializeField] private float alturaSobreSuperficie = 0.05f;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    public Vector3 PuntoDestino
    {
        get
        {
            if (col == null) col = GetComponent<Collider>();

            // Usa el centro geométrico y la cara superior del collider, ignorando el pivote 3D
            Vector3 centro = col.bounds.center;
            centro.y = col.bounds.max.y + alturaSobreSuperficie;
            return centro;
        }
    }
}