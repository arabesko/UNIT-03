using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class CombineAndWeldSelected : MonoBehaviour
{
    [MenuItem("Tools/Combine & Weld Selected")]
    static void CombineAndWeld()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("Selecciona objetos con MeshFilter!");
            return;
        }

        float weldThreshold = 0.01f; // ajustá si es necesario (0.001 - 0.01)
        var combinedVerts = new List<Vector3>();
        var combinedNormals = new List<Vector3>();
        var combinedUVs = new List<Vector2>();
        var combinedColors = new List<Color>();
        var combinedTangents = new List<Vector4>();
        var combinedTris = new List<int>();

        var map = new Dictionary<string, int>(); // key -> new index

        int globalIndex = 0;

        foreach (var go in Selection.gameObjects)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var mesh = mf.sharedMesh;
            var verts = mesh.vertices;
            var normals = mesh.normals.Length == verts.Length ? mesh.normals : null;
            var uvs = mesh.uv.Length == verts.Length ? mesh.uv : null;
            var colors = mesh.colors.Length == verts.Length ? mesh.colors : null;
            var tangs = mesh.tangents.Length == verts.Length ? mesh.tangents : null;
            var tris = mesh.triangles;

            // transform vertices from local to world (so relative positions match)
            for (int i = 0; i < verts.Length; i++)
                verts[i] = go.transform.localToWorldMatrix.MultiplyPoint3x4(verts[i]);

            for (int ti = 0; ti < tris.Length; ti += 3)
            {
                int i0 = tris[ti], i1 = tris[ti + 1], i2 = tris[ti + 2];
                int[] newIdx = new int[3];

                int[] src = new int[] { i0, i1, i2 };
                bool degenerate = false;
                Vector3 a = verts[i0], b = verts[i1], c = verts[i2];
                if (Vector3.Distance(a, b) < 1e-6f || Vector3.Distance(a, c) < 1e-6f || Vector3.Distance(b, c) < 1e-6f)
                    degenerate = true;

                if (degenerate) continue;

                for (int k = 0; k < 3; k++)
                {
                    int si = src[k];
                    // key: rounded position + rounded uv (if exist)
                    StringBuilder key = new StringBuilder();
                    key.Append(RoundVec3(verts[si], weldThreshold));
                    if (uvs != null) key.Append("|UV:" + RoundVec2(uvs[si], 1e-4f)); // tighten uv tolerance if needed

                    string s = key.ToString();
                    if (!map.TryGetValue(s, out int newIndex))
                    {
                        newIndex = globalIndex++;
                        map[s] = newIndex;
                        combinedVerts.Add(verts[si]);
                        combinedNormals.Add(normals != null ? normals[si] : Vector3.zero);
                        combinedUVs.Add(uvs != null ? uvs[si] : Vector2.zero);
                        combinedColors.Add(colors != null ? colors[si] : Color.white);
                        combinedTangents.Add(tangs != null ? tangs[si] : Vector4.zero);
                    }
                    newIdx[k] = newIndex;
                }

                // skip triangles that collapsed to line/point
                if (newIdx[0] == newIdx[1] || newIdx[0] == newIdx[2] || newIdx[1] == newIdx[2]) continue;

                combinedTris.Add(newIdx[0]);
                combinedTris.Add(newIdx[1]);
                combinedTris.Add(newIdx[2]);
            }
        }

        if (combinedVerts.Count == 0)
        {
            Debug.LogWarning("No se generó mesh combinado.");
            return;
        }

        Mesh newMesh = new Mesh();
        newMesh.indexFormat = combinedVerts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        newMesh.SetVertices(combinedVerts);
        newMesh.SetTriangles(combinedTris, 0);
        if (combinedUVs.Count == combinedVerts.Count) newMesh.SetUVs(0, combinedUVs);
        if (combinedNormals.Count == combinedVerts.Count) newMesh.SetNormals(combinedNormals);
        if (combinedColors.Count == combinedVerts.Count) newMesh.SetColors(combinedColors);
        if (combinedTangents.Count == combinedVerts.Count) newMesh.SetTangents(combinedTangents);

        newMesh.RecalculateNormals();
#if UNITY_2017_1_OR_NEWER
        newMesh.RecalculateTangents();
#endif
        newMesh.RecalculateBounds();

        // save asset
        string folder = "Assets/CombinedMeshes";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "CombinedMeshes");
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/CombinedMesh.asset");
        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // create GO with mesh
        GameObject outGO = new GameObject("Combined_Welded");
        var mfOut = outGO.AddComponent<MeshFilter>();
        var mrOut = outGO.AddComponent<MeshRenderer>();
        mfOut.sharedMesh = newMesh;
        // assign first selected material (user may want to change)
        var firstMr = Selection.gameObjects[0].GetComponent<MeshRenderer>();
        if (firstMr) mrOut.sharedMaterial = firstMr.sharedMaterial;

        Debug.Log("Combined & Welded mesh creado: " + path + ". Revisa UVs y normal maps.");
    }

    static string RoundVec3(Vector3 v, float thr)
    {
        int x = Mathf.RoundToInt(v.x / thr);
        int y = Mathf.RoundToInt(v.y / thr);
        int z = Mathf.RoundToInt(v.z / thr);
        return x + "_" + y + "_" + z;
    }
    static string RoundVec2(Vector2 v, float thr)
    {
        int x = Mathf.RoundToInt(v.x / thr);
        int y = Mathf.RoundToInt(v.y / thr);
        return x + "_" + y;
    }
}
