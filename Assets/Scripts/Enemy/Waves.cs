using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public int waveNum = 0;
    private bool inWave = false;
    public bool[] enemiesInWave;

    // Start is called before the first frame update
    void Start()
    {
        enemiesInWave = new bool[enemyPrefab.Length];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnWave(int enemiesNum)
    {
        if (waveNum == 1) {
            enemiesInWave[0] = true;
        }
        // Spawn a wave of enemies
        for (int i = 0; i < enemiesNum; i++)
        {
            // Instantiate a random
        }
    }

    public void StartWave()
    {
        inWave = true;
        waveNum++;
        SpawnWave(waveNum);
    }
}
