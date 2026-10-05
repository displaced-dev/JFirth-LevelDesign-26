using System;
using UnityEngine;
using UnityEngine.Events;
using LevelDesign.Data;
using MoreMountains.Feedbacks;

namespace LevelDesign.Systems
{
    public class Health : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private UnityEvent onDamageTaken;
        public UnityEvent OnDamageTaken => onDamageTaken;
        [SerializeField] private UnityEvent onDeath;
        public UnityEvent OnDeath => onDeath;

        [Header("Debug")]
        public float current;
        public float Max => maxHealth;
        public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;

        public float Current { get => current; private set => current = value; }
        public bool IsDead => Current <= 0f;

        [Header("Events")]
        [SerializeField] private CinematicCameraEventChannelSO e_cinematicCamera;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player damageFeedback;
        [SerializeField] private MMF_Player healingFeedback;

        private bool inCinematic;
        public bool immune;
        private bool ShouldTakeDamage() => !inCinematic && !immune;

        private void OnEnable() {
            if (e_cinematicCamera == null) { return; }
            e_cinematicCamera.OnRequestCamera += HandleCinematicStart;
            e_cinematicCamera.OnReleaseCamera += HandleCinematicEnd;
        }

        private void OnDisable() {
            if (e_cinematicCamera == null) { return; }
            e_cinematicCamera.OnRequestCamera -= HandleCinematicStart;
            e_cinematicCamera.OnReleaseCamera -= HandleCinematicEnd;
        }

        private void Awake() {
            ResetHealth();
        }

        public void Heal(float amount) {
            if(IsDead || amount < 0) { return; }
            if(healingFeedback != null) { healingFeedback.PlayFeedbacks(this.transform.position, amount); }

            Current = Mathf.Min(maxHealth, Current + amount);
        }

        public void TakeDamage(float amount)
        {
            if(IsDead || amount <= 0f || !ShouldTakeDamage()) { return; }

            if(amount > 0 && damageFeedback != null) { damageFeedback.PlayFeedbacks(this.transform.position, amount); }

            Current = Mathf.Max(0f, Current - amount);

            if(IsDead) {
                onDeath?.Invoke();
                return;
            }

            onDamageTaken?.Invoke();
        }

        public void ResetHealth() {
            Current = maxHealth;
        }

        private void HandleCinematicStart(Transform _) => inCinematic = true;
        private void HandleCinematicEnd() => inCinematic = false;
    }
}