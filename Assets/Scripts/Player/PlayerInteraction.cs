using UnityEngine;

namespace Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Weapon.WeaponSystem weaponSystem;

        private void Awake()
        {
            if (weaponSystem == null)
            {
                Debug.LogError("PlayerInteraction: No WeaponSystem found");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (weaponSystem != null)
            {
                weaponSystem.OnPlayerTouch(other);
            }
        }
    }
}