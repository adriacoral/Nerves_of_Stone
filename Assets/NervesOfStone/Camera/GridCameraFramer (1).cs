using UnityEngine;

/// Cámara fija que encuadra la cuadrícula del nivel.
/// Mide las losas UNA vez (al activarse), así las animaciones no mueven la cámara.
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class GridCameraFramer : MonoBehaviour
{
    [Header("Nivel")]
    [SerializeField] Transform target;                           // raíz del nivel: las losas son hijas suyas
    [SerializeField] bool measureChildren = true;                // mide las losas hijas
    [SerializeField] Vector2Int gridSize = new Vector2Int(4, 3); // solo se usa si no mide
    [SerializeField] float cellSize = 2f;
    [SerializeField] float blockHeight = 1f;

    [Header("Ángulo")]
    [SerializeField, Range(30f, 80f)] float pitch = 55f;
    [SerializeField, Range(-45f, 45f)] float yaw = 0f;

    [Header("Encuadre")]
    [SerializeField, Range(1f, 1.5f)] float margin = 1.15f;
    [SerializeField, Range(0f, 0.5f)] float uiRightFraction = 0.25f; // pantalla que tapa el panel derecho
    [SerializeField] float orthoDistance = 30f;

    Camera cam;
    Vector3 center, half;

    void OnEnable() { Refit(); }
    void OnValidate() { Refit(); }
    void LateUpdate() { if (Application.isPlaying) Apply(); else Refit(); }

    [ContextMenu("Refit")]
    public void Refit()
    {
        if (cam == null) cam = GetComponent<Camera>();
        Measure();
        Apply();
    }

    void Measure()
    {
        Vector3 basePos = target != null ? target.position : Vector3.zero;
        center = basePos + Vector3.up * (blockHeight * 0.5f);
        half = new Vector3(gridSize.x * cellSize, blockHeight, gridSize.y * cellSize) * 0.5f;

        if (!measureChildren || target == null) return;
        var rs = target.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        center = b.center;
        half = b.extents;
    }

    void Apply()
    {
        if (cam == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion inv = Quaternion.Inverse(rot);
        float usable = 1f - uiRightFraction;
        float aspect = cam.aspect;
        float tanV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float tanH = tanV * aspect;

        float dist = 0f, size = 0f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? -half.x : half.x,
                (i & 2) == 0 ? -half.y : half.y,
                (i & 4) == 0 ? -half.z : half.z);

            Vector3 p = inv * corner;
            float x = Mathf.Abs(p.x) * margin;
            float y = Mathf.Abs(p.y) * margin;

            dist = Mathf.Max(dist, x / (tanH * usable) - p.z, y / tanV - p.z);
            size = Mathf.Max(size, y, x / (aspect * usable));
        }

        bool ortho = cam.orthographic;
        if (ortho) cam.orthographicSize = size;

        float d = ortho ? orthoDistance : dist;
        float shift = ortho ? uiRightFraction * size * aspect : uiRightFraction * dist * tanH;

        transform.SetPositionAndRotation(
            center - rot * Vector3.forward * d + rot * Vector3.right * shift, rot);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, half * 2f);
    }
}
