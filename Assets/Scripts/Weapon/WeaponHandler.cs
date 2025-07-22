using Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Weapon
{
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform barrelEnd;
        [SerializeField] private float bulletSpeed = 5500f;

        [Header("Audio")]
        [SerializeField] private AudioSource weaponAudioSource;
        [SerializeField] private AudioClip shotSfx;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem muzzleFlashVFX;
        
        private PlayerInputActions _playerInputActions;

        private void Awake()
        {
            Initialize();
            BindInputActions();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Disable();
        }

        private void Initialize()
        {
            _playerInputActions = new PlayerInputActions();
        }

        private void BindInputActions()
        {
            _playerInputActions.Player.Fire.performed += OnFire;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            PlayShotSound();
            PlayMuzzleFlash();
            
            if (bulletPrefab == null)
            {
                Debug.LogError("WeaponHandler: Bullet Prefab not assigned.");
                return;
            }
            
            if (barrelEnd == null)
            {
                Debug.LogError("WeaponHandler: Barrel End not assigned.");
                return;
            }
            
            GameObject bulletInstance = Instantiate(bulletPrefab, barrelEnd.position, barrelEnd.rotation);
            
            if (bulletInstance.TryGetComponent<Rigidbody>(out Rigidbody bulletRigidbody))
            {
                bulletRigidbody.AddForce(barrelEnd.forward * bulletSpeed);
            }
        }
        
        private void PlayShotSound()
        {
            if (weaponAudioSource != null && shotSfx != null)
            {
                weaponAudioSource.PlayOneShot(shotSfx);
            }
        }
        private void PlayMuzzleFlash()
        {
            if (muzzleFlashVFX != null)
            {
                muzzleFlashVFX.Play();
            }
        }
        
    }
}