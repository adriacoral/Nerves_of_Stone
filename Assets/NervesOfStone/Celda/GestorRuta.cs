using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GestorRuta : MonoBehaviour
{
    [Header("Configuración de Ruta")]
    [SerializeField] private int maxCeldas = 5;
    [SerializeField] private LayerMask capaCeldas;

    [Header("Casillas Obligatorias")]
    [Tooltip("Casilla por la que el jugador debe empezar obligatoriamente")]
    [SerializeField] private Celda casillaInicio;
    [Tooltip("Casilla de meta donde termina la ruta")]
    [SerializeField] private Celda casillaFinal;

    private readonly List<Celda> rutaActual = new List<Celda>();
    public IReadOnlyList<Celda> RutaActual => rutaActual;

    // Servirá en el Paso 3 para validar que la ruta llegó hasta la meta antes de mover las fichas
    public bool RutaCompleta => casillaFinal != null && rutaActual.Count > 1 && rutaActual[rutaActual.Count - 1] == casillaFinal;

    public event Action<IReadOnlyList<Celda>> OnRutaActualizada;

    private Camera camaraPrincipal;

    private void Awake()
    {
        camaraPrincipal = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        DetectarClicCelda();
    }

    private void DetectarClicCelda()
    {
        Vector2 posicionRaton = Mouse.current.position.ReadValue();
        Ray rayo = camaraPrincipal.ScreenPointToRay(posicionRaton);

        if (Physics.Raycast(rayo, out RaycastHit impacto, 100f, capaCeldas))
        {
            if (impacto.collider.TryGetComponent(out Celda celda))
            {
                GestionarSeleccion(celda);
            }
        }
    }

    private void GestionarSeleccion(Celda celda)
    {
        // 1. Si haces clic en la última celda elegida, se quita (deshacer)
        if (rutaActual.Count > 0 && rutaActual[rutaActual.Count - 1] == celda)
        {
            rutaActual.RemoveAt(rutaActual.Count - 1);
            Debug.Log($"Celda desmarcada. Seleccionadas: {rutaActual.Count}/{maxCeldas}");
            OnRutaActualizada?.Invoke(rutaActual);
            return;
        }

        // 2. Si la ruta está vacía, obliga a empezar por la casilla de inicio
        if (rutaActual.Count == 0 && casillaInicio != null && celda != casillaInicio)
        {
            Debug.Log($"Debes empezar por la casilla de inicio ({casillaInicio.name}).");
            return;
        }

        // 3. Si ya se alcanzó la casilla final, bloquea añadir más casillas
        if (RutaCompleta)
        {
            Debug.Log("Ruta ya completada hasta la meta. Desmarca la última casilla si quieres cambiarla.");
            return;
        }

        // 4. No repetir celdas ni superar el límite
        if (rutaActual.Contains(celda) || rutaActual.Count >= maxCeldas)
            return;

        rutaActual.Add(celda);

        if (celda == casillaFinal)
            Debug.Log($"¡Ruta completa hasta {celda.name}! ({rutaActual.Count}/{maxCeldas})");
        else
            Debug.Log($"Añadida {celda.name}. Seleccionadas: {rutaActual.Count}/{maxCeldas}");

        OnRutaActualizada?.Invoke(rutaActual);
    }

    public void LimpiarRuta()
    {
        rutaActual.Clear();
        OnRutaActualizada?.Invoke(rutaActual);
    }
}