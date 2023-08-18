using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class EnemySpawner : NetworkBehaviour
{
    public List<Transform> spawnPoints;
    public List<NetworkPrefabRef> enemyPrefabs;
    public bool isSpawning = false;
    public float spawnMinterval;
    public float spawnMaxterval;
    public float spawnSpeedupPerMin;
    public int totalThreshold, basicThreshold, midThreshold;
    public int thresholdSpeedUpPer30Second;
    TickTimer timer, startTimer;

    [Networked] public float score { get; set; } = 0f;
    public int spawnTypeBias;
    public double diffTimerReduc;
    bool singleSpawn = false, startTimed = true;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (!Runner.IsServer)
            return;

        if (!isSpawning)
            return;

        if (startTimed){
            startTimer = TickTimer.CreateFromSeconds(Runner, 1f);
            startTimed = false;
        }
        if (!startTimer.Expired(Runner))
            return;

        // score = Time.realtimeSinceStartup*100f;
        score += Runner.DeltaTime * 100f;
        diffTimerReduc = score / 100 / 60 * spawnSpeedupPerMin;
        spawnTypeBias = (int)Mathf.Round(score / 100 / 30 * thresholdSpeedUpPer30Second);


        if (timer.ExpiredOrNotRunning(Runner))
        {
            timer = TickTimer.CreateFromSeconds(Runner, UnityEngine.Random
                                .Range(spawnMinterval - (float)diffTimerReduc, spawnMaxterval - (float)diffTimerReduc));


            //! Spawn batch of enemies (new biased spawn)
            //? Based on score + randomness, choose a type of enemy to spawn
            //? Based on enemy type, choose how many enemies to spawn
            int pick = Random.Range(0, totalThreshold);
            int enemyType = 1;
            if (pick <= basicThreshold - spawnTypeBias)
                enemyType = 1;
            else if (pick <= midThreshold - spawnTypeBias)
                enemyType = 2;
            else
                enemyType = 3;

            switch (enemyType)
            {
                case 1:
                    SpawnEnemies(4, 0);
                    break;

                case 2:
                    SpawnEnemies(2, Random.Range(1, 3));
                    break;

                case 3:
                    SpawnEnemies(1, 3);
                    break;
            }

            //! Legacy random spawn
            // //? Currently scaling hp 1%/sec
            // void AdjustStats(NetworkRunner runner, NetworkObject obj)
            // {
            //     obj.gameObject.GetComponent<UnitStats>().maxHealth *= 1 + (score / 100) / 100;
            //     obj.gameObject.GetComponent<UnitStats>().currentHealth = obj.gameObject.GetComponent<UnitStats>().maxHealth;
            // }
            // Runner.Spawn(enemyPrefabs[UnityEngine.Random.Range((int)0, (int)enemyPrefabs.Count)],
            //                 spawnPoints[UnityEngine.Random.Range((int)0, (int)spawnPoints.Count)].position,
            //                 Quaternion.identity, inputAuthority: null, AdjustStats, predictionKey: null);


            //singleSpawn = true;
        }
    }

    public void SpawnEnemies(int count, int prefabIndex)
    {
        //? Currently scaling hp 1%/10sec
        void AdjustStats(NetworkRunner runner, NetworkObject obj)
        {
            obj.gameObject.GetComponent<UnitStats>().maxHealth *= 1 + (score / 100) / 100 /10;
            obj.gameObject.GetComponent<UnitStats>().currentHealth = obj.gameObject.GetComponent<UnitStats>().maxHealth;
        }

        NavMeshHit hit;
        Vector3 spPos;
        if (NavMesh.SamplePosition(
            spawnPoints[UnityEngine.Random.Range((int)0, (int)spawnPoints.Count)].position,
            out hit, 5f, NavMesh.AllAreas))
        {
            spPos = hit.position;
        }
        else
        {
            Debug.LogError("No valid point found on navmesh close to spawn points");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            try
            {
                Runner.Spawn(enemyPrefabs[prefabIndex],
                                spPos,
                                Quaternion.identity, inputAuthority: null, AdjustStats, predictionKey: null);
            }
            catch
            {
                //Engine/api error ignrored
            }
        }
    }
}
