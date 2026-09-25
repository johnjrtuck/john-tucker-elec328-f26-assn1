using System.Collections.Generic;
using UnityEngine;

// Decorative background flourish -- NOT one of your three required
// deformations, just atmosphere. Spawns lumpy, randomly-shaped rocks that
// tumble in place and drift slowly across the background, looping back
// around once they drift far enough away.
public class AsteroidField : MonoBehaviour
{
    [Header("Field shape")]
    public int asteroidCount = 20;
    public float minDistance = 15f;
    public float maxDistance = 30f;

    [Header("Asteroid look")]
    public int resolution = 8;
    public float minRadius = 0.3f;
    public float maxRadius = 1.2f;
    public float bumpiness = 0.35f;
    public Material asteroidMaterial;

    [Header("Motion")]
    public float minDriftSpeed = 0.2f;
    public float maxDriftSpeed = 1f;
    public float minSpinSpeed = 10f;
    public float maxSpinSpeed = 60f;

    class Asteroid
    {
        public Transform transform;
        public Vector3 driftDirection;
        public float driftSpeed;
        public Vector3 spinAxis;
        public float spinSpeed;
    }

    List<Asteroid> asteroids = new List<Asteroid>();

    void Start()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            SpawnAsteroid();
        }
    }

    void SpawnAsteroid()
    {
        GameObject go = new GameObject("Asteroid");
        go.transform.SetParent(transform);
        go.transform.position = RandomPointInShell();

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        if (asteroidMaterial != null) mr.material = asteroidMaterial;

        float radius = Random.Range(minRadius, maxRadius);
        Vector3[] cubeVerts = CubeSphereUtility.GenerateCubeVertices(resolution, radius);
        Vector3[] sphereVerts = CubeSphereUtility.GenerateSphereFromCube(cubeVerts, radius);
        Vector3[] lumpyVerts = MakeLumpy(sphereVerts, radius);
        int[] triangles = CubeSphereUtility.GenerateCubeTriangles(resolution);

        Mesh mesh = new Mesh();
        mesh.name = "Asteroid";
        mesh.vertices = lumpyVerts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        Asteroid a = new Asteroid();
        a.transform = go.transform;
        a.driftDirection = Random.onUnitSphere;
        a.driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed);
        a.spinAxis = Random.onUnitSphere;
        a.spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        asteroids.Add(a);
    }

    Vector3[] MakeLumpy(Vector3[] baseVerts, float radius)
    {
        Vector3[] result = new Vector3[baseVerts.Length];
        Vector3 seedOffset = new Vector3(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Vector3 dir = baseVerts[i].normalized;
            Vector3 samplePoint = dir * 2f + seedOffset;
            float n1 = Mathf.PerlinNoise(samplePoint.x, samplePoint.y);
            float n2 = Mathf.PerlinNoise(samplePoint.y, samplePoint.z);
            float noise = (n1 + n2) * 0.5f - 0.5f;
            result[i] = dir * (radius + noise * radius * bumpiness);
        }

        return result;
    }

    Vector3 RandomPointInShell()
    {
        Vector3 dir = Random.onUnitSphere;
        float dist = Random.Range(minDistance, maxDistance);
        return transform.position + dir * dist;
    }

    void Update()
    {
        foreach (Asteroid a in asteroids)
        {
            a.transform.position += a.driftDirection * a.driftSpeed * Time.deltaTime;
            a.transform.Rotate(a.spinAxis, a.spinSpeed * Time.deltaTime, Space.World);

            float dist = Vector3.Distance(a.transform.position, transform.position);
            if (dist > maxDistance * 1.5f)
            {
                a.transform.position = RandomPointInShell();
            }
        }
    }
}