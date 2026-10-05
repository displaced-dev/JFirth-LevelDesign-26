using UnityEngine;
using LevelDesign.Data;
using LevelDesign.Systems.Player;

namespace LevelDesign.Systems.Enemy
{
    [DisallowMultipleComponent]
    public class EnemyMage : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private InteractiveArea interactiveArea;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectile;

        [Header("Animation")]
        [SerializeField] private string healBool = "AltAttack";

        [Header("Data")]
        [SerializeField] private CharacterTweaksSO characterTweaks;

        private float lastFiredTime = float.NegativeInfinity;

        public Animator Anim { get { return characterAnimator; } }
        public Transform FirePoint { get { return firePoint != null ? firePoint : transform; } }
        public bool IsHealing { get; private set; }

        public bool CanCastDamage { get { return characterTweaks != null && characterTweaks.canCastDamage && projectile != null; } }
        public bool CanCastHealing { get { return characterTweaks != null && characterTweaks.canCastHealing && interactiveArea != null; } }
        public float CastRate { get { return characterTweaks != null ? (float)characterTweaks.castRate : 1f; } }
        public float ProjectileSpeed { get { return characterTweaks != null ? (float)characterTweaks.castVelocity : 0f; } }

        private void Awake()
        {
            if(characterAnimator == null) { characterAnimator = GetComponentInChildren<Animator>(); }
        }

        private void OnEnable()
        {
            if(interactiveArea != null && characterTweaks != null) {
                interactiveArea.SetAmount(characterTweaks.healAmount);
                interactiveArea.SetInterval(characterTweaks.healRate);
            }
            StopHeal();
        }

        private void OnDisable()
        {
            StopHeal();
        }

        public bool SpawnSpell(Vector3 targetPoint)
        {
            if(!CanCastDamage || firePoint == null) { return false; }
            if(Time.time < (CastRate + lastFiredTime)) { return false; }

            lastFiredTime = Time.time;

            Vector3 dir = targetPoint - firePoint.position;
            if(dir.sqrMagnitude < 0.0001f) { dir = transform.forward; }
            Quaternion rot = Quaternion.LookRotation(dir.normalized);

            GameObject vfx = Instantiate(projectile, firePoint.position, rot);
            vfx.GetComponent<ProjectileBehaviour>()?.Init(projDamage: characterTweaks.castDamage, projSpeed: characterTweaks.castVelocity, projLifetime: characterTweaks.castLinger, impactLife: characterTweaks.castLinger);
            return true;
        }

        public void Heal()
        {
            if(!CanCastHealing) { return; }
            SetBool(healBool, true);
            interactiveArea.gameObject.SetActive(true);
            IsHealing = true;
        }

        public void StopHeal()
        {
            SetBool(healBool, false);
            if(interactiveArea != null) { interactiveArea.gameObject.SetActive(false); }
            IsHealing = false;
        }

        public void SetTrigger(string trigger)
        {
            if(HasParam(trigger, AnimatorControllerParameterType.Trigger)) { characterAnimator.SetTrigger(trigger); }
        }

        public void ResetTrigger(string trigger)
        {
            if(HasParam(trigger, AnimatorControllerParameterType.Trigger)) { characterAnimator.ResetTrigger(trigger); }
        }

        private void SetBool(string param, bool on)
        {
            if(HasParam(param, AnimatorControllerParameterType.Bool)) { characterAnimator.SetBool(param, on); }
        }

        private bool HasParam(string param, AnimatorControllerParameterType type)
        {
            if(characterAnimator == null || string.IsNullOrEmpty(param) || characterAnimator.runtimeAnimatorController == null) { return false; }
            foreach(var p in characterAnimator.parameters) {
                if(p.type == type && p.name == param) { return true; }
            }
            return false;
        }
    }
}