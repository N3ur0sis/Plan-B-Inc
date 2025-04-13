using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OutlineHighlighter : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float scaleFactor = 1.03f;

    private GameObject outlineObject;

    public void EnableOutline()
    {
        if (outlineObject != null) return;

        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null || meshRenderer == null || outlineMaterial == null) return;

        outlineObject = new GameObject("OutlineMesh");
        outlineObject.transform.SetParent(transform, false); // local space

        // Alignement exact
        outlineObject.transform.localPosition = meshRenderer.localBounds.center;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * scaleFactor;

        var filter = outlineObject.AddComponent<MeshFilter>();
        filter.mesh = meshFilter.sharedMesh;

        var renderer = outlineObject.AddComponent<MeshRenderer>();
        renderer.material = outlineMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    public void DisableOutline()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
            outlineObject = null;
        }
    }
}
