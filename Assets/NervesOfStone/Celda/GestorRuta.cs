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

    private readonly List<Celda> rutaActual = new List<Celda>();
    public IReadOnlyList<Celda> RutaActual => rutaActual;

    // Avisará automáticamente al visualizador (Paso 2) cuando cambie la ruta
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

        // Evita clicar una celda cuando pulses el botón del Canvas en el Paso 3
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
        // Si haces clic en la última celda elegida, se quita (deshacer)
        if (rutaActual.Count > 0 && rutaActual[rutaActual.Count - 1] == celda)
        {
            rutaActual.RemoveAt(rutaActual.Count - 1);
            Debug.Log($"Celda desmarcada. Seleccionadas: {rutaActual.Count}/{maxCeldas}");
            OnRutaActualizada?.Invoke(rutaActual);
            return;
        }

        // No repetir celdas ya elegidas ni pasar del límite
        if (rutaActual.Contains(celda) || rutaActual.Count >= maxCeldas)
            return;

        rutaActual.Add(celda);
        Debug.Log($"Añadida {celda.name}. Seleccionadas: {rutaActual.Count}/{maxCeldas}");
        OnRutaActualizada?.Invoke(rutaActual);
    }

    public void LimpiarRuta()
    {
        rutaActual.Clear();
        OnRutaActualizada?.Invoke(rutaActual);
    }
}