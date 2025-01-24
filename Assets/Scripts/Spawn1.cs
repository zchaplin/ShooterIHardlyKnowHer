using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    // Reference to the object prefab you want to spawn
    public GameObject smallObject;
    public GameObject bigObject;
    public GameObject specialObject;
    private GameObject objectToSpawn;

    // Variables to control wave parameters
    public float initialWaveDuration = 5f;
    public float initialSpawnInterval = 1.5f;
    public float waveIncreaseFactor = 1.2f;
    public float spawnIntervalDecreaseFactor = 0.9f;
    public float minSpawnInterval = 0.5f;
    private int waveCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine for managing waves
        StartCoroutine(ManageWaves());
    }

    IEnumerator ManageWaves()
    {
        float currentWaveDuration = initialWaveDuration;
        float currentSpawnInterval = initialSpawnInterval;

        while (true)
        {
            yield return StartCoroutine(SpawnWave(currentWaveDuration, currentSpawnInterval));

            // Wait between 10 and 20 seconds before the next wave
            float waitTime = Random.Range(10f, 20f);
            yield return new WaitForSeconds(waitTime);

            // Increase wave difficulty
            currentWaveDuration *= waveIncreaseFactor;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * spawnIntervalDecreaseFactor);

            waveCount++;
        }
    }

    IEnumerator SpawnWave(float waveDuration, float spawnInterval)
    {
        float elapsedTime = 0f;
        int spawnCount = 0;

        while (elapsedTime < waveDuration)
        {
            spawnCount++;
            SpawnEnemy(spawnCount);

            // Wait for the next spawn
            yield return new WaitForSeconds(spawnInterval);

            elapsedTime += spawnInterval;
        }
    }

    void SpawnEnemy(int spawnCount)
    {
        // Generate a random X coordinate between -10 and 10
        float randomX = Random.Range(-10f, 10f);
        float randomChoice = Random.Range(0, 10);
        objectToSpawn = smallObject;

        if (waveCount >= 3 && spawnCount % 10 == 0)
        {
            objectToSpawn = specialObject;
        }
        else if (randomChoice > 8)
        {
            objectToSpawn = bigObject;
        }
        // Create a new position vector with the random X coordinate
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, -15);

        // Instantiate a new object at the spawnPosition with no rotation
        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPosition, transform.rotation);
        MoveForward moveForward = spawnedObject.GetComponent<MoveForward>();

        moveForward.Activate(0.5f);
    }
}
