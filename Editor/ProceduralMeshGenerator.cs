using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Procedural Mesh Generator - Create 3D Meshes from Code
/// Version 1.0 | Generate Rocks, Crystals, and Organic Shapes
/// </summary>
public class ProceduralMeshGenerator : EditorWindow
{
    public enum MeshType { Rock, Crystal, Sphere, Tube, Torus, Terrain }
    
    private MeshType meshType = MeshType.Rock;
    private int subdivisions = 3;
    private float size = 1f;
    private float noiseStrength = 0.3f;
    private float noiseScale = 2f;
    private int seed = 0;
    private bool generateCollider = true;
    private bool generateLODs = false;
    private Material customMaterial;
    
    private Vector2 scrollPosition;
    private GameObject lastGenerated;
    
    [MenuItem("Tools/Procedural Mesh Generator")]
    public static void ShowWindow()
    {
        ProceduralMeshGenerator window = GetWindow<ProceduralMeshGenerator>("Mesh Generator");
        window.minSize = new Vector2(380, 500);
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 18;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🔷 PROCEDURAL MESH GENERATOR", headerStyle);
        GUILayout.Label("Create 3D Meshes Procedurally", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(10);
        
        // Mesh Type
        GUILayout.Label("🎨 Mesh Type", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        meshType = (MeshType)EditorGUILayout.EnumPopup("Type", meshType);
        DrawTypePreview();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Settings
        GUILayout.Label("⚙️ Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        subdivisions = EditorGUILayout.IntSlider("Detail Level", subdivisions, 1, 6);
        size = EditorGUILayout.Slider("Size", size, 0.1f, 10f);
        
        if (meshType == MeshType.Rock || meshType == MeshType.Terrain)
        {
            noiseStrength = EditorGUILayout.Slider("Noise Strength", noiseStrength, 0f, 1f);
            noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.5f, 10f);
        }
        
        seed = EditorGUILayout.IntField("Random Seed", seed);
        
        if (GUILayout.Button("🎲 Randomize Seed"))
        {
            seed = Random.Range(0, 99999);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Options
        GUILayout.Label("📦 Export Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        generateCollider = EditorGUILayout.Toggle("Add Mesh Collider", generateCollider);
        generateLODs = EditorGUILayout.Toggle("Generate LODs", generateLODs);
        customMaterial = (Material)EditorGUILayout.ObjectField("Custom Material", customMaterial, typeof(Material), false);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // Generate Button
        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.9f);
        if (GUILayout.Button("🔷 GENERATE MESH", GUILayout.Height(45)))
        {
            GenerateMesh();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate 5 Variations", GUILayout.Height(30)))
        {
            GenerateVariations(5);
        }
        if (GUILayout.Button("Save as Prefab", GUILayout.Height(30)))
        {
            SaveAsPrefab();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Export as .asset", GUILayout.Height(25)))
        {
            ExportMeshAsset();
        }
        
        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawTypePreview()
    {
        string desc = meshType switch
        {
            MeshType.Rock => "🪨 Organic rock with noise displacement",
            MeshType.Crystal => "💎 Crystalline structure with sharp edges",
            MeshType.Sphere => "⚪ Smooth sphere with subdivisions",
            MeshType.Tube => "🔵 Hollow tube/pipe shape",
            MeshType.Torus => "🍩 Donut shape",
            MeshType.Terrain => "🏔️ Flat terrain patch with noise",
            _ => ""
        };
        EditorGUILayout.HelpBox(desc, MessageType.None);
    }
    
    private void GenerateMesh()
    {
        Random.InitState(seed);
        
        Mesh mesh = meshType switch
        {
            MeshType.Rock => GenerateRock(),
            MeshType.Crystal => GenerateCrystal(),
            MeshType.Sphere => GenerateSphere(),
            MeshType.Tube => GenerateTube(),
            MeshType.Torus => GenerateTorus(),
            MeshType.Terrain => GenerateTerrain(),
            _ => GenerateSphere()
        };
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        GameObject obj = new GameObject(meshType.ToString() + "_" + seed);
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        
        mf.sharedMesh = mesh;
        
        if (customMaterial != null)
        {
            mr.sharedMaterial = customMaterial;
        }
        else
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetDefaultColor();
            mr.sharedMaterial = mat;
        }
        
        if (generateCollider)
        {
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = true;
        }
        
        Selection.activeGameObject = obj;
        lastGenerated = obj;
        
        Debug.Log("✅ Generated " + meshType + " mesh with " + mesh.vertexCount + " vertices!");
    }
    
    private Color GetDefaultColor()
    {
        return meshType switch
        {
            MeshType.Rock => new Color(0.5f, 0.5f, 0.5f),
            MeshType.Crystal => new Color(0.4f, 0.7f, 0.9f),
            MeshType.Sphere => Color.white,
            MeshType.Tube => new Color(0.6f, 0.4f, 0.2f),
            MeshType.Torus => new Color(0.8f, 0.6f, 0.2f),
            MeshType.Terrain => new Color(0.4f, 0.5f, 0.3f),
            _ => Color.white
        };
    }
    
    private Mesh GenerateRock()
    {
        Mesh mesh = GenerateSphere();
        Vector3[] verts = mesh.vertices;
        
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            float noise = Mathf.PerlinNoise(v.x * noiseScale + seed, v.z * noiseScale + seed);
            noise += Mathf.PerlinNoise(v.y * noiseScale * 2 + seed, v.x * noiseScale * 2 + seed) * 0.5f;
            verts[i] = v * (1f + (noise - 0.5f) * noiseStrength * 2);
        }
        
        mesh.vertices = verts;
        return mesh;
    }
    
    private Mesh GenerateCrystal()
    {
        Mesh mesh = new Mesh();
        
        int segments = 6;
        float height = size * 2;
        float radius = size * 0.4f;
        
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        
        // Bottom point
        verts.Add(Vector3.down * height * 0.3f);
        
        // Middle ring
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float r = radius * (1f + Random.Range(-0.2f, 0.2f));
            verts.Add(new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r));
        }
        
        // Top point
        verts.Add(Vector3.up * height * 0.7f);
        
        // Bottom triangles
        for (int i = 0; i < segments; i++)
        {
            tris.Add(0);
            tris.Add(1 + (i + 1) % segments);
            tris.Add(1 + i);
        }
        
        // Top triangles
        int topIndex = verts.Count - 1;
        for (int i = 0; i < segments; i++)
        {
            tris.Add(topIndex);
            tris.Add(1 + i);
            tris.Add(1 + (i + 1) % segments);
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        
        return mesh;
    }
    
    private Mesh GenerateSphere()
    {
        Mesh mesh = new Mesh();
        
        int latitudes = Mathf.Max(3, subdivisions * 4);
        int longitudes = Mathf.Max(4, subdivisions * 6);
        
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        
        for (int lat = 0; lat <= latitudes; lat++)
        {
            float theta = lat * Mathf.PI / latitudes;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            
            for (int lon = 0; lon <= longitudes; lon++)
            {
                float phi = lon * 2 * Mathf.PI / longitudes;
                float x = Mathf.Cos(phi) * sinTheta;
                float y = cosTheta;
                float z = Mathf.Sin(phi) * sinTheta;
                
                verts.Add(new Vector3(x, y, z) * size);
            }
        }
        
        for (int lat = 0; lat < latitudes; lat++)
        {
            for (int lon = 0; lon < longitudes; lon++)
            {
                int current = lat * (longitudes + 1) + lon;
                int next = current + longitudes + 1;
                
                tris.Add(current);
                tris.Add(next);
                tris.Add(current + 1);
                
                tris.Add(current + 1);
                tris.Add(next);
                tris.Add(next + 1);
            }
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        
        return mesh;
    }
    
    private Mesh GenerateTube()
    {
        Mesh mesh = new Mesh();
        
        int segments = Mathf.Max(6, subdivisions * 4);
        float height = size * 2;
        float outerRadius = size;
        float innerRadius = size * 0.7f;
        
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        
        // Create rings
        for (int ring = 0; ring <= 1; ring++)
        {
            float y = ring == 0 ? -height / 2 : height / 2;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                verts.Add(new Vector3(Mathf.Cos(angle) * outerRadius, y, Mathf.Sin(angle) * outerRadius));
                verts.Add(new Vector3(Mathf.Cos(angle) * innerRadius, y, Mathf.Sin(angle) * innerRadius));
            }
        }
        
        // Connect rings
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            
            // Outer wall
            int bo = i * 2;
            int bno = next * 2;
            int to = segments * 2 + i * 2;
            int tno = segments * 2 + next * 2;
            
            tris.AddRange(new[] { bo, to, bno });
            tris.AddRange(new[] { bno, to, tno });
            
            // Inner wall
            int bi = i * 2 + 1;
            int bni = next * 2 + 1;
            int ti = segments * 2 + i * 2 + 1;
            int tni = segments * 2 + next * 2 + 1;
            
            tris.AddRange(new[] { bi, bni, ti });
            tris.AddRange(new[] { bni, tni, ti });
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        
        return mesh;
    }
    
    private Mesh GenerateTorus()
    {
        Mesh mesh = new Mesh();
        
        int mainSegments = Mathf.Max(8, subdivisions * 6);
        int tubeSegments = Mathf.Max(6, subdivisions * 4);
        float mainRadius = size;
        float tubeRadius = size * 0.3f;
        
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        
        for (int i = 0; i <= mainSegments; i++)
        {
            float u = i * Mathf.PI * 2 / mainSegments;
            Vector3 center = new Vector3(Mathf.Cos(u), 0, Mathf.Sin(u)) * mainRadius;
            
            for (int j = 0; j <= tubeSegments; j++)
            {
                float v = j * Mathf.PI * 2 / tubeSegments;
                Vector3 point = center + new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(v), Mathf.Sin(u) * Mathf.Cos(v)) * tubeRadius;
                verts.Add(point);
            }
        }
        
        for (int i = 0; i < mainSegments; i++)
        {
            for (int j = 0; j < tubeSegments; j++)
            {
                int current = i * (tubeSegments + 1) + j;
                int next = current + tubeSegments + 1;
                
                tris.Add(current);
                tris.Add(next);
                tris.Add(current + 1);
                
                tris.Add(current + 1);
                tris.Add(next);
                tris.Add(next + 1);
            }
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        
        return mesh;
    }
    
    private Mesh GenerateTerrain()
    {
        Mesh mesh = new Mesh();
        
        int res = Mathf.Max(4, subdivisions * 8);
        
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        
        for (int z = 0; z <= res; z++)
        {
            for (int x = 0; x <= res; x++)
            {
                float xPos = ((float)x / res - 0.5f) * size * 2;
                float zPos = ((float)z / res - 0.5f) * size * 2;
                float y = Mathf.PerlinNoise(x * noiseScale * 0.1f + seed, z * noiseScale * 0.1f + seed) * noiseStrength * size;
                verts.Add(new Vector3(xPos, y, zPos));
            }
        }
        
        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                int current = z * (res + 1) + x;
                tris.Add(current);
                tris.Add(current + res + 1);
                tris.Add(current + 1);
                
                tris.Add(current + 1);
                tris.Add(current + res + 1);
                tris.Add(current + res + 2);
            }
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        
        return mesh;
    }
    
    private void GenerateVariations(int count)
    {
        int originalSeed = seed;
        
        for (int i = 0; i < count; i++)
        {
            seed = originalSeed + i * 1000;
            GenerateMesh();
            
            if (lastGenerated != null)
            {
                lastGenerated.transform.position = new Vector3(i * size * 2.5f, 0, 0);
            }
        }
        
        seed = originalSeed;
        Debug.Log("✅ Generated " + count + " variations!");
    }
    
    private void SaveAsPrefab()
    {
        if (lastGenerated == null)
        {
            EditorUtility.DisplayDialog("No Mesh", "Generate a mesh first!", "OK");
            return;
        }
        
        string path = "Assets/_GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "_GeneratedMeshes");
        }
        
        string prefabPath = path + "/" + lastGenerated.name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(lastGenerated, prefabPath);
        
        Debug.Log("✅ Saved prefab: " + prefabPath);
        EditorUtility.DisplayDialog("Saved!", "Prefab saved to:\n" + prefabPath, "OK");
    }
    
    private void ExportMeshAsset()
    {
        if (lastGenerated == null)
        {
            EditorUtility.DisplayDialog("No Mesh", "Generate a mesh first!", "OK");
            return;
        }
        
        MeshFilter mf = lastGenerated.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;
        
        string path = "Assets/_GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "_GeneratedMeshes");
        }
        
        string assetPath = path + "/" + lastGenerated.name + ".asset";
        AssetDatabase.CreateAsset(Instantiate(mf.sharedMesh), assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Exported mesh: " + assetPath);
        EditorUtility.DisplayDialog("Exported!", "Mesh asset saved to:\n" + assetPath, "OK");
    }
}
