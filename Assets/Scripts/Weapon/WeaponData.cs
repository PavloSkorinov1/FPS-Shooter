using UnityEngine;

namespace Weapon
{
    public class WeaponData : MonoBehaviour
    {
        [Header("Weapon Components")]
        public Transform barrelEnd;
        public ParticleSystem muzzleFlashVFX;
        public AudioSource shotAudioSource;
        
        [Header("Projectile")]
        public GameObject bulletPrefab;
    }
}