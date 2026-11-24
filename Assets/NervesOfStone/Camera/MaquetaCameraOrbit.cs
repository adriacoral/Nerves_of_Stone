using UnityEngine;
using UnityEngine.InputSystem;

public class MaquetaCameraOrbit : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;

    [Header("Rotación (Maqueta)")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private bool limitYaw = true;
    [SerializeField] private float maxYawOffset = 35f;
    [SerializeField] private float minPitch = 15f;
    [SerializeField] private float maxPitch = 75f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 20f;

    private float initialYaw;
    private float targetYaw;
    private float targetPitch;
    private float currentYaw;
    private float currentPitch;
    private float yawVelocity;
    private float pitchVelocity;

    private float targetDistance;
    private float currentDistance;
    private float distanceVelocity;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            if (transform.childCount > 0)
            {
                cameraTransform = transform.GetChild(0);
            }
            else if (Camera.main != null && Camera.main.transform != transform)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        if (cameraTransform == null || cameraTransform == transform)
        {
            Debug.LogWarning("MaquetaCameraOrbit debe ir en PivotCamera y tener Camera asignada como hija.");
            enabled = false;
            return;
        }

        // Lee la distancia y ángulos exactos desde donde colocaste la cámara en la escena
        Vector3 offset = cameraTransform.position - transform.position;
        currentDistance = targetDistance = Mathf.Max(offset.magnitude, 0.5f);

        Vector3 dir = offset.normalized;
        initialYaw = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
        float startPitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

        // Evita que los límites muevan la cámara al arrancar
        minPitch = Mathf.Min(minPitch, startPitch);
        maxPitch = Mathf.Max(maxPitch, startPitch);
        minDistance = Mathf.Min(minDistance, currentDistance);
        maxDistance = Mathf.Max(maxDistance, currentDistance);

        currentYaw = targetYaw = initialYaw;
        currentPitch = targetPitch = startPitch;
    }

    private void Update()
    {
        HandleInput();
    }

    private void LateUpdate()
    {
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, smoothTime);

        Quaternion orbitRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        cameraTransform.position = transform.position + orbitRotation * new Vector3(0f, 0f, -currentDistance);
        cameraTransform.LookAt(transform.position);
    }

    private void HandleInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.isPressed || mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            targetYaw += delta.x * rotationSpeed * Time.deltaTime * 0.05f;
            targetPitch -= delta.y * rotationSpeed * Time.deltaTime * 0.05f;

            if (limitYaw)
            {
                targetYaw = Mathf.Clamp(targetYaw, initialYaw - maxYawOffset, initialYaw + maxYawOffset);
            }

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= Mathf.Sign(scroll) * zoomSpeed * Time.deltaTime * 15f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }
}