using UnityEngine;
using LevelDesign.Async.Auth;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class Mage : _InputAuth
    {
        [Header("Scene Refs")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private InteractiveArea interactiveArea;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectile;

        [Header("Config")]
        [SerializeField] private LayerMask aimMask = ~0;
        [SerializeField] private float aimDistance = 100f;

        [Header("Data")]
        [SerializeField] private CharacterTweaksSO characterTweaks;

        [Header("Events")]
        [SerializeField] private StaminaEventChannelSO e_stamina;

        private float lastFiredTime;

        private void OnEnable() {
            aInputInit(autoPopulateGame: true);

            InputAuthManager.Instance.RequestInput(this);

            interactiveArea.SetAmount(characterTweaks.healAmount);
            interactiveArea.SetInterval(characterTweaks.healRate);
        }

        private void OnDisable() {
            if(InputAuthManager.Instance != null) { InputAuthManager.Instance.RelinquishRequest(this); }
            StopHeal();
        }

        private void Update() {
            aInputInit(autoPopulateGame: true);

            if(_inputAuthorized != true) { return; }

            if(_input.AltFire.IsPressed() && characterTweaks.canCastHealing && SpendStamina(characterTweaks.healingCost * Time.deltaTime)) {
                Heal();
                return;
            }
            StopHeal();

            if(_input.Fire.WasPressedThisFrame() && characterTweaks.canCastDamage) {
                SpawnSpell();
            }
        }

        private bool SpendStamina(float amount) {
            return e_stamina == null || e_stamina.SpendStamina(amount);
        }

        private void Heal() {
            characterAnimator.SetBool("AltAttack", true);
            interactiveArea.gameObject.SetActive(true);
        }

        private void StopHeal() {
            characterAnimator.SetBool("AltAttack", false);
            interactiveArea.gameObject.SetActive(false);
        }

        void SpawnSpell() {
            if(firePoint == null || projectile == null) { return; }
            if(Time.time < (characterTweaks.castRate + lastFiredTime)) { return; }

            Camera cam = Camera.main;
            if(cam == null) { return; }

            if(!SpendStamina(characterTweaks.castCost)) { return; }

            lastFiredTime = Time.time;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, aimDistance, aimMask, QueryTriggerInteraction.Ignore)
                ? hit.point
                : ray.GetPoint(aimDistance);

            Vector3 dir = (targetPoint - firePoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir);

            GameObject vfx = Instantiate(projectile, firePoint.position, rot);
            vfx.GetComponent<ProjectileBehaviour>()?.Init(projDamage: characterTweaks.castDamage, projSpeed: characterTweaks.castVelocity, projLifetime: characterTweaks.castLinger, impactLife: characterTweaks.castLinger);
        }
    }
}