using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input;

namespace Weapon
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private float dropForce = 1f;
        [SerializeField] private float dropCooldown = 1f;
        [SerializeField] private Transform playerTransform; 

        [Header("Layer Settings")]
        [SerializeField] private LayerMask pickupLayer;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pulloutSfx;
        
        private List<GameObject> _heldWeapons = new List<GameObject>();
        private GameObject _currentWeapon;
        private WeaponData _currentWeaponData;
        private PlayerInputActions _playerInputActions;
        
        private int _currentWeaponIndex = -1;
        public WeaponData CurrentWeaponData => _currentWeaponData;

        private void Awake()
        {
            Initialize();
            BindInputActions();
        }
        
        private void OnEnable() => _playerInputActions.Player.Enable();
        private void OnDisable() => _playerInputActions.Player.Disable();

        private void Initialize()
        {
            _playerInputActions = new PlayerInputActions();
            if (playerTransform == null)
            {
                Debug.LogError("WeaponSystem: Player Transform reference not set");
            }
        }

        private void BindInputActions()
        {
            _playerInputActions.Player.Drop.performed += ctx => DropCurrentWeapon();
            _playerInputActions.Player.SwitchWeapon.performed += ctx => OnSwitchWeapon(ctx.ReadValue<Vector2>());
        }
        
        public void OnPlayerTouch(Collider other)
        {
            if ((pickupLayer.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<WeaponPickup>(out WeaponPickup pickupComponent))
                {
                    EquipWeapon(pickupComponent);
                }
            }
        }
        
        private void EquipWeapon(WeaponPickup newWeaponItem)
        {
            GameObject weaponObject = newWeaponItem.gameObject;
            _heldWeapons.Add(weaponObject);

            weaponObject.transform.SetParent(weaponHolder);
            weaponObject.transform.localPosition = Vector3.zero;
            weaponObject.transform.localRotation = Quaternion.identity;
            
            _currentWeapon = weaponObject;
            _currentWeaponData = _currentWeapon.GetComponent<WeaponData>();

            newWeaponItem.OnPickup();
            SwitchToWeapon(_heldWeapons.Count - 1);
        }

        private void DropCurrentWeapon()
        {
            if (_currentWeapon == null) return;

            GameObject weaponToDrop = _currentWeapon;
            _heldWeapons.Remove(weaponToDrop);
            
            weaponToDrop.transform.SetParent(null);
            
            if (!weaponToDrop.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = weaponToDrop.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            rb.AddForce((playerTransform.forward + Vector3.up * 0.5f) * dropForce, ForceMode.Impulse);
            
            if (weaponToDrop.TryGetComponent<WeaponPickup>(out var weaponItem))
            {
                weaponItem.OnDrop();
            }
            
            StartCoroutine(DropCooldownRoutine(weaponToDrop));

            _currentWeapon = null;
            _currentWeaponData = null;

            if (_heldWeapons.Count > 0)
            {
                _currentWeaponIndex--;
                if (_currentWeaponIndex < 0) _currentWeaponIndex = 0;
                SwitchToWeapon(_currentWeaponIndex);
            }
            else
            {
                _currentWeaponIndex = -1;
            }
        }
        
        private void OnSwitchWeapon(Vector2 scrollInput)
        {
            if (_heldWeapons.Count <= 1) return;

            if (scrollInput.y > 0)
            {
                _currentWeaponIndex++;
                if (_currentWeaponIndex >= _heldWeapons.Count)
                {
                    _currentWeaponIndex = 0;
                }
            }
            else if (scrollInput.y < 0)
            {
                _currentWeaponIndex--;
                if (_currentWeaponIndex < 0)
                {
                    _currentWeaponIndex = _heldWeapons.Count - 1;
                }
            }
            
            SwitchToWeapon(_currentWeaponIndex);
        }
        
        private void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= _heldWeapons.Count) return;

            if (_currentWeapon != null)
            {
                _currentWeapon.SetActive(false);
            }
            
            if (audioSource != null && pulloutSfx != null)
            {
                audioSource.PlayOneShot(pulloutSfx);
            }

            _currentWeaponIndex = index;
            _currentWeapon = _heldWeapons[_currentWeaponIndex];
            _currentWeapon.SetActive(true);
            _currentWeaponData = _currentWeapon.GetComponent<WeaponData>();
        }
        
        private IEnumerator DropCooldownRoutine(GameObject droppedWeapon)
        {
            int originalLayer = LayerMask.NameToLayer("WeaponPickup");
            droppedWeapon.layer = LayerMask.NameToLayer("IgnorePickup");

            yield return new WaitForSeconds(dropCooldown);
            
            if(droppedWeapon != null)
            {
                if(droppedWeapon.TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
                droppedWeapon.layer = originalLayer;
            }
        }
    }
}