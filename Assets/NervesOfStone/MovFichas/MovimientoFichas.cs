using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovimientoFichas : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GestorRuta gestorRuta;
    [SerializeField] private Button botonMover;
    [Tooltip("Arrastra aquí las fichas rojas (ficha, ficha (1), ficha (2)...)")]
    [SerializeField] private Transform[] fichas;

    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 3.5f;
    [SerializeField] private float retrasoEntreFichas = 0.15f;
    [Tooltip("Sube o baja este valor si las fichas quedan flotando o algo hundidas en la casilla")]
    [SerializeField] private float alturaExtraFicha = 0f;

    private bool estaMoviendo;

    // Pequeña separación para que las fichas no se monten unas encima de otras
    private readonly Vector3[] offsetsFormacion = new Vector3[]
    {
        new Vector3(0f, 0f, 0.2f),
        new Vector3(-0.2f, 0f, -0.15f),
        new Vector3(0.2f, 0f, -0.15f),
        new Vector3(0f, 0f, -0.25f)
    };

    private void OnEnable()
    {
        if (gestorRuta != null)
            gestorRuta.OnRutaActualizada += ActualizarEstadoBoton;
    }

    private void OnDisable()
    {
        if (gestorRuta != null)
            gestorRuta.OnRutaActualizada -= ActualizarEstadoBoton;
    }

    private void Start()
    {
        ActualizarEstadoBoton(gestorRuta != null ? gestorRuta.RutaActual : null);
    }

    private void ActualizarEstadoBoton(IReadOnlyList<Celda> ruta)
    {
        if (botonMover != null)
            botonMover.interactable = !estaMoviendo && gestorRuta != null && gestorRuta.RutaCompleta;
    }

    // Este método se conecta al OnClick() del botón en el Canvas
    public void IniciarRecorrido()
    {
        if (estaMoviendo || gestorRuta == null || !gestorRuta.RutaCompleta)
            return;

        StartCoroutine(MoverGrupoCoroutine());
    }

    private IEnumerator MoverGrupoCoroutine()
    {
        estaMoviendo = true;
        if (botonMover != null) botonMover.interactable = false;

        List<Vector3> puntosBase = new List<Vector3>();
        foreach (Celda celda in gestorRuta.RutaActual)
        {
            puntosBase.Add(celda.PuntoDestino + Vector3.up * alturaExtraFicha);
        }

        int fichasTerminadas = 0;

        for (int i = 0; i < fichas.Length; i++)
        {
            if (fichas[i] == null)
            {
                fichasTerminadas++;
                continue;
            }

            Vector3 offset = offsetsFormacion[i % offsetsFormacion.Length];
            float retraso = i * retrasoEntreFichas;

            StartCoroutine(MoverFichaIndividual(fichas[i], puntosBase, offset, retraso, () => fichasTerminadas++));
        }

        yield return new WaitUntil(() => fichasTerminadas >= fichas.Length);

        estaMoviendo = false;
        gestorRuta.LimpiarRuta();
    }

    private IEnumerator MoverFichaIndividual(Transform ficha, List<Vector3> puntos, Vector3 offset, float retrasoInicial, System.Action alTerminar)
    {
        if (retrasoInicial > 0f)
            yield return new WaitForSeconds(retrasoInicial);

        for (int i = 0; i < puntos.Count; i++)
        {
            Vector3 destino = puntos[i] + offset;

            while (Vector3.Distance(ficha.position, destino) > 0.01f)
            {
                ficha.position = Vector3.MoveTowards(ficha.position, destino, velocidad * Time.deltaTime);
                yield return null;
            }

            ficha.position = destino;
        }

        alTerminar?.Invoke();
    }
}