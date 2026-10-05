using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign.Systems
{
    public enum AreaType
    {
        Fire,
        InstantDamage,
        HealingZone,
        HealingPickup
    }

    [RequireComponent(typeof(BoxCollider))]
    public class InteractiveArea : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private AreaType areaType;
        [SerializeField] private float interval;
        [SerializeField] private float amount = 10f;

        public Health ignoreHealth;

        private readonly Dictionary<Health, float> nextInteraction = new Dictionary<Health, float>();

        private void OnTriggerEnter(Collider other)
        {
            if (!TryGetTarget(other, out Health health)) { return; }

            switch (areaType)
            {
                case AreaType.InstantDamage:
                    if (TryConsumeCooldown(health)) health.TakeDamage(amount);
                    break;
                case AreaType.HealingPickup:
                    health.Heal(amount);
                    gameObject.SetActive(false);
                    break;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if(areaType != AreaType.Fire && areaType != AreaType.HealingZone) { return; }
            if(!TryGetTarget(other, out Health health)) { return; }
            if(!TryConsumeCooldown(health)) { return; }

            if(areaType == AreaType.Fire) {
                health.TakeDamage(amount);
            }
            else {
                health.Heal(amount);
            } 
        }

        private void OnTriggerExit(Collider other)
        {
            if(areaType == AreaType.InstantDamage) { return; }
            if(other.TryGetComponent(out Health health)) {
                nextInteraction.Remove(health);
            }
        }

        private void OnDisable() => nextInteraction.Clear();

        private bool TryGetTarget(Collider other, out Health health)
        {
            return other.TryGetComponent(out health) && health != ignoreHealth;
        }

        private bool TryConsumeCooldown(Health health)
        {
            if(nextInteraction.TryGetValue(health, out float next) && Time.time < next) { return false; }

            nextInteraction[health] = Time.time + interval;
            return true;
        }

        public void SetAmount(float a) => amount = a;
        public void SetInterval(float i) => interval = i;
    }
}