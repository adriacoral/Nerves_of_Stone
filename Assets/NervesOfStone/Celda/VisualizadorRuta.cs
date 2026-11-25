using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VisualizadorRuta : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GestorRuta gestorRuta;

    [Header("Estilo Fantasmal")]
    [SerializeField] private Color colorFantasmal = new Color(0.3f, 0.9f, 1f, 0.6f);
    [SerializeField] private float grosorInicio = 0.25f;
    [SerializeField] private float grosorPunta = 0.04f;

    private LineRenderer linea;

    private void Awake()
    {
        linea = GetComponent<LineRenderer>();
        linea.useWorldSpace = true;
        linea.startWidth = grosorInicio;
        linea.endWidth = grosorPunta;
        linea.startColor = colorFantasmal;
        linea.endColor = colorFantasmal;
        linea.numCapVertices = 6;
        linea.numCornerVertices = 6;
        linea.positionCount = 0;
    }

    private void OnEnable()
    {
        if (gestorRuta != null)
            gestorRuta.OnRutaActualizada += ActualizarLinea;
    }

    private void OnDisable()
    {
        if (gestorRuta != null)
            gestorRuta.OnRutaActualizada -= ActualizarLinea;
    }

    private void ActualizarLinea(IReadOnlyList<Celda> celdas)
    {
        if (celdas == null || celdas.Count == 0)
        {
            linea.positionCount = 0;
            return;
        }

        // Con 1 casilla marcada, dibuja un punto inicial en su centro
        if (celdas.Count == 1)
        {
            Vector3 p = celdas[0].PuntoDestino;
            linea.positionCount = 2;
            linea.SetPosition(0, p);
            linea.SetPosition(1, p + Vector3.forward * 0.02f);
            return;
        }

        // A partir de 2 casillas, conecta directamente de casilla en casilla
        linea.positionCount = celdas.Count;
        for (int i = 0; i < celdas.Count; i++)
        {
            linea.SetPosition(i, celdas[i].PuntoDestino);
        }
    }
}