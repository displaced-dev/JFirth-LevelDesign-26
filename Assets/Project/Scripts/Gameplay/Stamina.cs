using System;
using UnityEngine;
using LevelDesign.Data;

namespace LevelDesign.Systems
{
    public class Stamina : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float regenPerSecond = 20f;
        [SerializeField] private float regenDelay = 1f;

        [Header("Events")]
        [SerializeField] private StaminaEventChannelSO e_stamina;
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;

        public float Current { get; private set; }
        public float Max { get { return maxStamina; } }

        private float lastSpendTime = float.NegativeInfinity;
        private Func<float, bool> handler;

        private void Awake()
        {
            Current = maxStamina;
            handler = TrySpend;
        }

        private void OnEnable()
        {
            if(e_stamina != null) { e_stamina.OnSpendRequested = handler; }
            if(e_playerkilled != null) { e_playerkilled.OnReviveRequested += Reset; }
        }

        private void OnDisable()
        {
            if(e_stamina != null && e_stamina.OnSpendRequested == handler) { e_stamina.OnSpendRequested = null; }
            if(e_playerkilled != null) { e_playerkilled.OnReviveRequested -= Reset; }
        }

        private void Update()
        {
            if(Time.time - lastSpendTime >= regenDelay) {
                Current = Mathf.Min(maxStamina, Current + regenPerSecond * Time.deltaTime);
            }
        }

        public void Reset() {
            Current = maxStamina;
        }

        public bool TrySpend(float amount)
        {
            if(amount > Current) { return false; }
            Current -= amount;
            lastSpendTime = Time.time;
            return true;
        }
    }
}
