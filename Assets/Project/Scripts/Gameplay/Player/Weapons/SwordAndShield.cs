using UnityEngine;
using LevelDesign.Async.Auth;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class SwordAndShield : _InputAuth
    {
        [Header("Scene Refs")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private SwordHitbox swordHitbox;
        [SerializeField] private InteractiveArea interactiveArea;
        [SerializeField] private RigInfo rig;
        [SerializeField] private CharacterTweaksSO characterTweaks;

        [Header("Config")]
        [SerializeField] private float swingRate = .65f;

        [Header("Event")]
        [SerializeField] private StaminaEventChannelSO e_stamina;

        private float nextSwingTime;

        private void OnEnable() {
            aInputInit(autoPopulateGame: true);
            InputAuthManager.Instance.RequestInput(this);
            interactiveArea.ignoreHealth = rig.health;
            interactiveArea.SetAmount(characterTweaks.meleeDamage);
        }

        private void OnDisable() {
            if(InputAuthManager.Instance != null) { InputAuthManager.Instance.RelinquishRequest(this); }
            StopBlock();
            swordHitbox.EndHit();
        }

        private void Update() {
            aInputInit(autoPopulateGame: true);

            if(_inputAuthorized != true) { return; }
            if(interactiveArea.ignoreHealth == null && rig.health != null) { interactiveArea.ignoreHealth = rig.health; }

            if(_input.AltFire.IsPressed() && characterTweaks.canShieldBlock && SpendStamina(characterTweaks.blockCost * Time.deltaTime)) {
                Block();
                return;
            }
            StopBlock();

            if(_input.Fire.WasPressedThisFrame()
                && characterTweaks.canSwingSword
                && Time.time >= nextSwingTime
                && SpendStamina(characterTweaks.swingAttackCost)) {
                SwingSword();
            }
        }

        private bool SpendStamina(float amount) {
            return e_stamina == null || e_stamina.SpendStamina(amount);
        }

        private void Block() {
            characterAnimator.SetBool("AltAttack", true);
            rig.health.immune = true;
        }

        private void StopBlock() {
            characterAnimator.SetBool("AltAttack", false);
            rig.health.immune = false;
        }

        private void SwingSword() {
            nextSwingTime = Time.time + swingRate;
            characterAnimator.ResetTrigger("Attack");
            characterAnimator.SetTrigger("Attack");
        }
    }
}