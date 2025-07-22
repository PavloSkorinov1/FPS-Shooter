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
        
        [Header("Hole Settings")]
        [SerializeField] private GameObject bulletHolePrefab;
        [SerializeField] private float bulletHoleLifetime = 3f;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSfx;
        
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
            if (hitSfx != null)
            {
                AudioSource.PlayClipAtPoint(hitSfx, hit.point);
            }
            
            if (bulletHolePrefab != null)
            {
                Quaternion holeRotation = Quaternion.LookRotation(hit.normal);
                GameObject hole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, holeRotation);
                Destroy(hole, bulletHoleLifetime);
            }
            
            Destroy(gameObject);
        }
    }
}