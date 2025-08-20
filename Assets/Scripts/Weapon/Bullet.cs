using AI;
using UnityEngine;

namespace Weapon
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float bulletLifetime = 3f;
        [SerializeField] private LayerMask hittableLayers;
        [SerializeField] private int damage = 100;
        
        [Header("Hole Settings")]
        [SerializeField] private GameObject bulletHolePrefab;
        [SerializeField] private float bulletHoleLifetime = 3f;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSFX;
        [SerializeField] private GameObject sfxPlayerPrefab;
        
        private Vector3 _previousPosition;
        
        private void Start()
        {
            _previousPosition = transform.position;
            Destroy(gameObject, bulletLifetime);
        }

        private void FixedUpdate()
        {
            Vector3 direction = transform.position - _previousPosition;
            float distance = direction.magnitude;

            if (distance > 0)
            {
                if (Physics.Raycast(_previousPosition, direction, out RaycastHit hit, distance, hittableLayers))
                {
                    HandleCollision(hit);
                }
            }

            _previousPosition = transform.position;
        }

        private void HandleCollision(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<BotHealth>(out var botHealth))
            {
                botHealth.TakeDamage(damage);
            }
            else
            {
                if (bulletHolePrefab != null)
                {
                    Quaternion holeRotation = Quaternion.LookRotation(hit.normal);
                    GameObject hole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, holeRotation);
                    Destroy(hole, bulletHoleLifetime);
                }
            }
            
            if (hitSFX != null && sfxPlayerPrefab != null)
            {
                GameObject sfxInstance = Instantiate(sfxPlayerPrefab, hit.point, Quaternion.identity);
                if (sfxInstance.TryGetComponent<AudioSource>(out AudioSource audioSource))
                {
                    audioSource.clip = hitSFX;
                    audioSource.Play();
                    Destroy(sfxInstance, hitSFX.length);
                }
            }
            
            Destroy(gameObject);
        }
    }
}