using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VertexHighlightOverlay : MonoBehaviour
{
    [Header("Shader")]
    public Shader highlightShader; // Drag your vertex highlight shader here

    [Header("Vertex Highlight Settings")]
    public Color vertexColor = Color.cyan;
    [Range(0.001f, 0.2f)]
    public float vertexSize = 0.02f; // start bigger for visibility
    [Range(0f, 5f)]
    public float intensity = 3f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material highlightMaterial;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter.sharedMesh == null || highlightShader == null) return;

        // Duplicate vertices per triangle and assign barycentric coords
        DuplicateVerticesPerTriangle(meshFilter.sharedMesh);

        // Create highlight material
        highlightMaterial = new Material(highlightShader);
        UpdateMaterialProperties();

        // Add highlight material as second material
        Material[] mats = meshRenderer.sharedMaterials;
        if (mats.Length == 1)
        {
            meshRenderer.materials = new Material[] { mats[0], highlightMaterial };
        }
        else
        {
            mats[mats.Length - 1] = highlightMaterial;
            meshRenderer.materials = mats;
        }
    }

    void OnValidate()
    {
        UpdateMaterialProperties();
    }

    private void UpdateMaterialProperties()
    {
        if (highlightMaterial != null)
        {
            highlightMaterial.SetColor("_VertexColor", vertexColor);
            highlightMaterial.SetFloat("_VertexSize", vertexSize);
            highlightMaterial.SetFloat("_Intensity", intensity);
        }
    }

    /// <summary>
    /// Duplicate vertices per triangle and assign barycentric coordinates
    /// </summary>
    private void DuplicateVerticesPerTriangle(Mesh mesh)
    {
        if (mesh == null) return;

        Vector3[] oldVerts = mesh.vertices;
        Vector3[] oldNormals = mesh.normals;
        Vector2[] oldUVs = mesh.uv;
        int[] oldTris = mesh.triangles;

        Vector3[] newVerts = new Vector3[oldTris.Length];
        Vector3[] newNormals = new Vector3[oldTris.Length];
        Vector2[] newUVs = new Vector2[oldTris.Length];
        Vector3[] bary = new Vector3[oldTris.Length];
        int[] newTris = new int[oldTris.Length];

        for (int i = 0; i < oldTris.Length; i += 3)
        {
            int i0 = oldTris[i + 0];
            int i1 = oldTris[i + 1];
            int i2 = oldTris[i + 2];

            newVerts[i + 0] = oldVerts[i0];
            newVerts[i + 1] = oldVerts[i1];
            newVerts[i + 2] = oldVerts[i2];

            if (oldNormals.Length == oldVerts.Length)
            {
                newNormals[i + 0] = oldNormals[i0];
                newNormals[i + 1] = oldNormals[i1];
                newNormals[i + 2] = oldNormals[i2];
            }

            if (oldUVs.Length == oldVerts.Length)
            {
                newUVs[i + 0] = oldUVs[i0];
                newUVs[i + 1] = oldUVs[i1];
                newUVs[i + 2] = oldUVs[i2];
            }

            // Assign barycentric coordinates per vertex
            bary[i + 0] = new Vector3(1, 0, 0);
            bary[i + 1] = new Vector3(0, 1, 0);
            bary[i + 2] = new Vector3(0, 0, 1);

            newTris[i + 0] = i + 0;
            newTris[i + 1] = i + 1;
            newTris[i + 2] = i + 2;
        }

        Mesh newMesh = new Mesh();
        newMesh.name = mesh.name + "_VertexHighlight";
        newMesh.vertices = newVerts;
        newMesh.normals = newNormals;
        newMesh.uv = newUVs;
        newMesh.triangles = newTris;
        newMesh.SetUVs(1, bary); // barycentric coords in UV2

        meshFilter.mesh = newMesh;
    }
}
