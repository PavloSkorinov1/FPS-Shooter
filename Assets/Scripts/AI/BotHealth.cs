using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace AI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(BotPursuit))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class BotHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private AnimationClip deathAnimation;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip ambientSfx;
        [SerializeField] private AudioClip deathSfx;
        
        private int _currentHealth;
        private Animator _animator;
        private NavMeshAgent _agent;
        private BotPursuit _pursuitScript;
        private Collider _collider;
        private Rigidbody _rigidbody;

        
        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            _currentHealth = maxHealth;
            if (_collider != null) _collider.enabled = true;
            if (_pursuitScript != null) _pursuitScript.enabled = true;
            if (_rigidbody != null) _rigidbody.isKinematic = false;
            
            if (audioSource != null && ambientSfx != null)
            {
                audioSource.clip = ambientSfx;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void Initialize()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _pursuitScript = GetComponent<BotPursuit>();
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void TakeDamage(int damage)
        {
            if (_currentHealth <= 0) return;
            
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                StartCoroutine(Die());
            }
        }

        private IEnumerator Die()
        {
            if (_agent != null) _agent.enabled = false;
            if (_pursuitScript != null) _pursuitScript.enabled = false;
            if (_rigidbody != null) _rigidbody.isKinematic = true;

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.SetTrigger("Die");
            }
            
            if (audioSource != null)
            {
                audioSource.Stop();
                if (deathSfx != null)
                {
                    audioSource.PlayOneShot(deathSfx);
                }
            }
            
            
            if (deathAnimation != null)
            {
                yield return new WaitForSeconds(deathAnimation.length);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            gameObject.SetActive(false);
        }
    }
}