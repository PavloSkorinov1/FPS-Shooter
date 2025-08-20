using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    public class BotSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")] [SerializeField]
        private GameObject botPrefab;

        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private int maxBots = 5;
        [SerializeField] private float spawnInterval = 3f;

        private List<GameObject> _botPool = new List<GameObject>();
        private int _botSpawnAreaMask;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private void Initialize()
        {
            _botSpawnAreaMask = 1 << NavMesh.GetAreaFromName("BotSpawnArea");
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                int activeBotCount = _botPool.Count(bot => bot.activeInHierarchy);
                if (activeBotCount < maxBots)
                {
                    SpawnNewBot();
                }

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnNewBot()
        {
            if (spawnPoints.Count == 0 || botPrefab == null) return;

            GameObject botToSpawn = _botPool.FirstOrDefault(bot => !bot.activeInHierarchy);

            if (botToSpawn == null)
            {
                botToSpawn = Instantiate(botPrefab);
                botToSpawn.SetActive(false);
                _botPool.Add(botToSpawn);
            }

            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            if (NavMesh.SamplePosition(randomPoint.position, out NavMeshHit hit, 5f, _botSpawnAreaMask))
            {
                StartCoroutine(ActivateBot(botToSpawn, hit.position));
            }
        }

        private IEnumerator ActivateBot(GameObject bot, Vector3 position)
        {
            if (bot.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                agent.enabled = false;
                bot.transform.position = position;
                bot.SetActive(true);
                
                yield return null;

                agent.enabled = true;
            }
        }
    }
}