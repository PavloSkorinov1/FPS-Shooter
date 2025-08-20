using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BotPatrol : MonoBehaviour
    {
        [SerializeField] private List<Transform> patrolPoints;
        [SerializeField] private Vector2 waitTimeRange = new Vector2(2f, 5f);
        
        private NavMeshAgent _agent;
        private int _previousPatrolIndex = -1;
        private bool _isWaiting;
        private void Awake()
        {
            Initialize();
        }
        
        private void OnEnable()
        {
            GoToNewPoint();
        }

        private void Start()
        {
            GoToNewPoint();
        }

        private void Update()
        {
            CheckIfDestinationReached();
        }

        private void Initialize()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void GoToNewPoint()
        {
            if (patrolPoints.Count == 0) return;

            int newIndex;
            do
            {
                newIndex = Random.Range(0, patrolPoints.Count);
            } 
            while (patrolPoints.Count > 1 && newIndex == _previousPatrolIndex);

            _previousPatrolIndex = newIndex;
            _agent.destination = patrolPoints[newIndex].position;
        }

        private void CheckIfDestinationReached()
        {
            if (!_isWaiting && !_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAndGoToNewPoint());
            }
        }
        
        private IEnumerator WaitAndGoToNewPoint()
        {
            _isWaiting = true;
            float waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
            yield return new WaitForSeconds(waitTime);
            
            GoToNewPoint();
            _isWaiting = false;
        }
    }
}
