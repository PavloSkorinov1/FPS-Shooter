using UnityEngine;

namespace Weapon
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Levitation Settings")]
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.25f;
        
        private Vector3 _startPosition;
        private bool _isLevitating = true;
        
        private Light _pointLight;
        private Collider _collider;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            _startPosition = transform.position;
        }
        
        private void FixedUpdate()
        {
            if (_isLevitating)
            {
                HandleLevitation();
            }
        }

        private void Initialize()
        {
            _pointLight = GetComponentInChildren<Light>();
            _collider = GetComponent<Collider>();
        }
        
        public void OnPickup()
        {
            _isLevitating = false;
            if (_pointLight != null) _pointLight.enabled = false;
            if (_collider != null) _collider.enabled = false;
            enabled = false;
        }

        public void OnDrop()
        {
            _isLevitating = true;
            _startPosition = transform.position;
            if (_pointLight != null) _pointLight.enabled = true;
            if (_collider != null) _collider.enabled = true;
            enabled = true;
        }

        private void HandleLevitation()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

    }
}