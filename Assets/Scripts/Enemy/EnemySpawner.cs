using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject[] enemyPrefab;
    public int waveNum = 0;
    private bool inWave = false;
    public bool[] enemiesInWave;
    public int maxWave = 20;
    public bool[] availableWeapons;
    public int numWeapons = 6;
    public float minX = -5f; // Minimum X position
    public float maxX = 5f;  // Maximum X position
    public float numEnemies = 10f;
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {                        
            StartCoroutine(StartWave());
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // SpawnObject();
            enemiesInWave = new bool[enemyPrefab.Length];
            availableWeapons = new bool[numWeapons]; //we currently have 

        }
    }

     void Update()
    {
     
    }



    public IEnumerator SpawnWave(int enemiesNum)
    {
        //Debug.Log("wave #: " + waveNum + " enemies in wave: " + enemiesNum);
        if (waveNum == 1) {
            enemiesInWave[0] = true;
            availableWeapons[1] = true;
        }
        else if (waveNum == 3) {
            enemiesInWave[1] = true;
            availableWeapons[2] = true;
        }
        else if (waveNum == 5) {
            enemiesInWave[1] = false;
            enemiesInWave[2] = true;
            enemiesInWave[3] = true;
            availableWeapons[3] = true;
        }
        else if (waveNum == 8) {
            enemiesInWave[0] = true;
            enemiesInWave[1] = true;
        }
        else if (waveNum == 10) {
            enemiesInWave[4] = true; // flying
            enemiesInWave[0] = true;
            enemiesInWave[1] = false;
            enemiesInWave[2] = false;
            enemiesInWave[3] = false;
            availableWeapons[4] = true;
        }
        else if (waveNum == 13) {
            enemiesInWave[1] = true;
            enemiesInWave[2] = true;
        }
        else if (waveNum == 15) {            
            enemiesInWave[3] = true;
        }
        // Spawn a wave of enemies
        for (int i = 0; i < enemiesNum; i++)
        {
            //Debug.Log("Number of Enemies: " + enemiesNum);
            int randomIndex = GetRandomTrueIndex(enemiesInWave);
            SpawnObject(enemyPrefab[randomIndex]);
            yield return new WaitForSeconds(1f); 
        }
        // if (waveNum <= maxWave) {
        inWave = false;
        StartCoroutine(StartWave());
        // }
    }

    public IEnumerator StartWave()
    {
        //Debug.Log("Function called! IsServer: " + IsServer);

        if (IsServer) {
            yield return new WaitForSeconds(15f); 
            inWave = true;
            waveNum++;
            if (minX > -10 && maxX < 10) {
                minX -= 1;
                maxX += 1;
            }
            
            //Debug.Log("Wave " + waveNum + " started");
            StartCoroutine(SpawnWave((int)numEnemies));
            numEnemies *= 1.2f;
            
        }
    }

    void SpawnObject(GameObject objectPrefab)
    {
        float randomX = UnityEngine.Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(transform.position.x+randomX, objectPrefab.transform.position.y, transform.position.z);
        // Instantiate the objectPrefab at the current position
        GameObject objectToSpawn = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

        // Set the layer of the spawned object to the name of the GameObject the script is attached to
        string layerName = gameObject.name;  // This will get the name of the GameObject the script is attached to
        int layer = LayerMask.NameToLayer(layerName);
        objectToSpawn.layer = layer;

        // Enable MeshRenderer if it exists
        MeshRenderer meshRenderer = objectToSpawn.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Use a NetworkVariable or RPC to synchronize visibility across clients
            meshRenderer.enabled = true;
        }

        // Get NetworkObject component
        NetworkObject networkObject = objectToSpawn.GetComponent<NetworkObject>();
        
        // Ensure the NetworkObject is spawned on both server and client
        networkObject.Spawn();
    }

    private int GetRandomTrueIndex(bool[] array)
    {
        System.Random random = new System.Random();
        
        // Get all indexes where the value is true
        var trueIndexes = array
            .Select((value, index) => new { value, index })
            .Where(item => item.value)
            .Select(item => item.index)
            .ToList();

        // Return a random index from the trueIndexes list
        return trueIndexes.Count > 0 ? trueIndexes[random.Next(trueIndexes.Count)] : -1;
    }


    public int getAvailableWeapons() {
        return GetRandomTrueIndex(availableWeapons);
    }
}
