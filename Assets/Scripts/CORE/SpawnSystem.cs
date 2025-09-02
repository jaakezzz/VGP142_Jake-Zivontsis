using UnityEngine;
using System.Linq;

public class SpawnSystem : MonoBehaviour
{
    public Transform[] spawnPoints;     // assign spawns here
    public GameObject[] spawnables;     // prefabs to choose from
    public int toSpawn = 3;

    public Terrain terrain;             // snap y to terrain

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length < toSpawn)
        {
            Debug.LogWarning("Not enough spawn points configured.");
            return;
        }

        // Pick distinct random points
        var chosen = spawnPoints.OrderBy(_ => Random.value).Take(toSpawn).ToArray();

        for (int i = 0; i < chosen.Length; i++)
        {
            var prefab = spawnables[Random.Range(0, spawnables.Length)];
            var p = chosen[i].position;

            // snap to terrain height if provided
            if (terrain && terrain.terrainData)
            {
                float h = terrain.SampleHeight(p) + terrain.GetPosition().y;
                p.y = h;
            }

            var rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, p, rot);
        }
    }
}
