using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BotPursuit : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _playerTransform;

        private void Awake()
        {
            Initialize();
        }
        
        private void OnEnable()
        {
            FindPlayer();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            PursuePlayer();
        }

        private void Initialize()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("BotPursuit: Could not find a GameObject with the 'Player' tag.");
            }
        }

        private void PursuePlayer()
        {
            if (_playerTransform != null && _agent.isOnNavMesh)
            {
                _agent.SetDestination(_playerTransform.position);
            }
        }
    }
}