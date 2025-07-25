using Input;
using UnityEngine;
using UnityEngine.InputSystem;
    
namespace Weapon
{
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private Camera mainCamera;

        [Header("Weapon Settings")]
        [SerializeField] private float bulletSpeed = 100f;
        [SerializeField] private LayerMask aimHittableLayers;
        
        private PlayerInputActions _playerInputActions;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            Initialize();
            BindInputActions();
        }

        private void OnEnable() => _playerInputActions.Player.Enable();
        private void OnDisable() => _playerInputActions.Player.Disable();

        private void Initialize()
        {
            if (weaponSystem == null)
            {
                Debug.LogError("WeaponHandler: WeaponSystem reference not set");
            }
        }

        private void BindInputActions()
        {
            _playerInputActions.Player.Fire.performed += OnFire;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            WeaponData currentWeaponData = weaponSystem.CurrentWeaponData;
            if (currentWeaponData == null) return;

            if (currentWeaponData.bulletPrefab == null)
            {
                Debug.LogError("WeaponHandler: The current weapon's bullet prefab is not set");
                return;
            }
            
            if (currentWeaponData.muzzleFlashVFX != null)
            {
                currentWeaponData.muzzleFlashVFX.Play();
            }

            if (currentWeaponData.shotAudioSource != null)
            {
                currentWeaponData.shotAudioSource.Play();
            }

            Vector3 aimDirection = GetAimDirection();
            
            GameObject bullet = Instantiate(currentWeaponData.bulletPrefab, currentWeaponData.barrelEnd.position, Quaternion.LookRotation(aimDirection));
            if (bullet.TryGetComponent<Rigidbody>(out Rigidbody bulletRb))
            {
                bulletRb.AddForce(aimDirection * bulletSpeed, ForceMode.Impulse);
            }
        }
        
        private Vector3 GetAimDirection()
        {
            Vector3 targetPoint;
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, 999f, aimHittableLayers))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = mainCamera.transform.position + mainCamera.transform.forward * 100f;
            }
            
            WeaponData currentWeaponData = weaponSystem.CurrentWeaponData;
            if (currentWeaponData != null)
            {
                return (targetPoint - currentWeaponData.barrelEnd.position).normalized;
            }
            return (targetPoint - mainCamera.transform.position).normalized;
        }
    }
}