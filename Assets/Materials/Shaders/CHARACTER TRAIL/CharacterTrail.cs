using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CharacterTrail - actualizado para controlar una propiedad "alpha" del shader (_alpha por defecto)
/// - Soporta DrawMesh (MaterialPropertyBlock) y GameObject instanciados.
/// - Ajusta alpha usando la propiedad del shader o cae a _Color/_BaseColor si no existe.
/// </summary>
[DisallowMultipleComponent]
public class CharacterTrail : MonoBehaviour
{
    [Header("General")]
    public float sampleInterval = 0.08f;
    public float lifeTime = 0.8f;
    public float meshScale = 1f;
    public Material trailMaterial;

    [Header("Render mode")]
    public bool useGraphicsDraw = true;

    [Header("Sources")]
    public SkinnedMeshRenderer[] skinnedSources;
    public MeshRenderer[] meshSources;

    [Header("Shader property")]
    [Tooltip("Nombre de la propiedad alpha en tu Shader Graph (ej: _alpha)")]
    public string alphaPropertyName = "_alpha";

    // internal caches
    private int alphaPropID;
    private int colorPropID;
    private int baseColorPropID;

    private struct TrailEntry
    {
        public Mesh mesh;
        public Matrix4x4 matrix;
        public float spawnTime;
        public Color color;
    }
    private List<TrailEntry> trailEntries = new List<TrailEntry>();
    private List<GameObject> spawnedTrailObjects = new List<GameObject>();

    private void Awake()
    {
        alphaPropID = Shader.PropertyToID(alphaPropertyName);
        colorPropID = Shader.PropertyToID("_Color");
        baseColorPropID = Shader.PropertyToID("_BaseColor");
    }

    private void OnEnable()
    {
        StartCoroutine(SampleRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ClearTrailsImmediate();
    }

    private IEnumerator SampleRoutine()
    {
        while (true)
        {
            SampleOnce();
            yield return new WaitForSeconds(sampleInterval);
        }
    }

    private void SampleOnce()
    {
        float now = Time.time;

        if (skinnedSources != null)
        {
            foreach (var smr in skinnedSources)
            {
                if (smr == null) continue;
                Mesh baked = new Mesh();
                smr.BakeMesh(baked);

                if (!Mathf.Approximately(meshScale, 1f))
                {
                    Vector3[] verts = baked.vertices;
                    for (int i = 0; i < verts.Length; i++) verts[i] *= meshScale;
                    baked.vertices = verts;
                    baked.RecalculateBounds();
                }

                Matrix4x4 mat = smr.transform.localToWorldMatrix;
                AddTrail(baked, mat, now, GetColorFromMaterial(smr.sharedMaterial));
            }
        }

        if (meshSources != null)
        {
            foreach (var mr in meshSources)
            {
                if (mr == null || mr.GetComponent<MeshFilter>() == null) continue;
                Mesh shared = mr.GetComponent<MeshFilter>().sharedMesh;
                if (shared == null) continue;

                Mesh copy = Instantiate(shared);
                if (!Mathf.Approximately(meshScale, 1f))
                {
                    Vector3[] verts = copy.vertices;
                    for (int i = 0; i < verts.Length; i++) verts[i] *= meshScale;
                    copy.vertices = verts;
                    copy.RecalculateBounds();
                }

                Matrix4x4 mat = mr.transform.localToWorldMatrix;
                AddTrail(copy, mat, now, GetColorFromMaterial(mr.sharedMaterial));
            }
        }
    }

    private Color GetColorFromMaterial(Material mat)
    {
        if (mat == null) return Color.white;
        if (mat.HasProperty(baseColorPropID)) return mat.GetColor(baseColorPropID);
        if (mat.HasProperty(colorPropID)) return mat.GetColor(colorPropID);
        return Color.white;
    }

    private void AddTrail(Mesh mesh, Matrix4x4 matrix, float now, Color baseColor)
    {
        if (useGraphicsDraw)
        {
            TrailEntry e = new TrailEntry
            {
                mesh = mesh,
                matrix = matrix,
                spawnTime = now,
                color = baseColor
            };
            trailEntries.Add(e);
        }
        else
        {
            GameObject go = new GameObject("TrailMesh");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            if (trailMaterial != null)
                mr.material = new Material(trailMaterial);
            else
                mr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            ApplyMatrixToTransform(go.transform, matrix);
            spawnedTrailObjects.Add(go);
            StartCoroutine(FadeAndDestroyGO(go, mr, lifeTime));
        }
    }

    private static void ApplyMatrixToTransform(Transform t, Matrix4x4 m)
    {
        t.position = m.GetColumn(3);
        t.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

    private IEnumerator FadeAndDestroyGO(GameObject go, MeshRenderer mr, float duration)
    {
        float start = Time.time;
        Material mat = mr.material;
        Color startColor = Color.white;
        if (mat.HasProperty(baseColorPropID)) startColor = mat.GetColor(baseColorPropID);
        else if (mat.HasProperty(colorPropID)) startColor = mat.GetColor(colorPropID);

        while (true)
        {
            float elapsed = Time.time - start;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(1f, 0f, t);

            if (mat.HasProperty(alphaPropID))
            {
                mat.SetFloat(alphaPropID, alpha);
            }
            else if (mat.HasProperty(colorPropID))
            {
                Color c = startColor;
                c.a = alpha;
                mat.SetColor(colorPropID, c);
            }
            else if (mat.HasProperty(baseColorPropID))
            {
                Color c = startColor;
                c.a = alpha;
                mat.SetColor(baseColorPropID, c);
            }

            if (t >= 1f) break;
            yield return null;
        }

        Destroy(go);
    }

    private void Update()
    {
        if (useGraphicsDraw)
        {
            float now = Time.time;
            MaterialPropertyBlock pb = new MaterialPropertyBlock();

            if (trailMaterial == null) return;

            for (int i = trailEntries.Count - 1; i >= 0; i--)
            {
                var e = trailEntries[i];
                float age = now - e.spawnTime;
                if (age >= lifeTime)
                {
                    if (e.mesh != null) Destroy(e.mesh);
                    trailEntries.RemoveAt(i);
                    continue;
                }
                float t = Mathf.Clamp01(age / lifeTime);
                float alpha = Mathf.Lerp(1f, 0f, t);

                // si el shader tiene _alpha
                if (trailMaterial.HasProperty(alphaPropID))
                {
                    pb.SetFloat(alphaPropID, alpha);
                }
                else if (trailMaterial.HasProperty(colorPropID))
                {
                    Color col = e.color;
                    col.a *= alpha;
                    pb.SetColor(colorPropID, col);
                }
                else if (trailMaterial.HasProperty(baseColorPropID))
                {
                    Color col = e.color;
                    col.a *= alpha;
                    pb.SetColor(baseColorPropID, col);
                }

                Graphics.DrawMesh(e.mesh, e.matrix, trailMaterial, gameObject.layer, null, 0, pb);
            }
        }

        for (int i = spawnedTrailObjects.Count - 1; i >= 0; i--)
            if (spawnedTrailObjects[i] == null) spawnedTrailObjects.RemoveAt(i);
    }

    private void ClearTrailsImmediate()
    {
        for (int i = 0; i < trailEntries.Count; i++)
            if (trailEntries[i].mesh != null) Destroy(trailEntries[i].mesh);
        trailEntries.Clear();

        for (int i = 0; i < spawnedTrailObjects.Count; i++)
            if (spawnedTrailObjects[i] != null) Destroy(spawnedTrailObjects[i]);
        spawnedTrailObjects.Clear();
    }
}
