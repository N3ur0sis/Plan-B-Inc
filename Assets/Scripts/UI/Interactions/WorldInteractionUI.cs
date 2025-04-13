using UnityEngine;

/// <summary>
/// Oriente l’objet pour faire face à la caméra.
/// </summary>
public class BillboardUI : MonoBehaviour
{
    private Transform camTransform;

    private void Start()
    {
        if (Camera.main != null)
            camTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Update camera reference if needed
        if (Camera.main != null && Camera.main.transform != camTransform)
            camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (camTransform == null) return;

        transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
    }
}
