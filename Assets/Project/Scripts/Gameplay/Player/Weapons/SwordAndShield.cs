using UnityEngine;
using LevelDesign.Async.Auth;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class SwordAndShield : _InputAuth
    {
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private SwordHitbox swordHitbox;
        [SerializeField] private InteractiveArea interactiveArea;
        [SerializeField] private RigInfo rig;
        [SerializeField] private CharacterTweaksSO characterTweaks;

        private void OnEnable() {
            aInputInit(autoPopulateGame: true);
            InputAuthManager.Instance.RequestInput(this);
            interactiveArea.ignoreHealth = rig.health;
            interactiveArea.SetAmount(characterTweaks.meleeDamage);
        }

        private void OnDisable() {
            if(InputAuthManager.Instance != null) { InputAuthManager.Instance.RelinquishRequest(this); }
            characterAnimator.SetBool("AltAttack", false);
            rig.health.immune = false;
            swordHitbox.EndHit();
        }

        private void Update() {
            aInputInit(autoPopulateGame: true);

            if(_inputAuthorized != true) { return; }
            if(interactiveArea.ignoreHealth == null && rig.health != null) { interactiveArea.ignoreHealth = rig.health; }

            if(_input.AltFire.IsPressed() && characterTweaks.canShieldBlock) {
                Block();
                return;
            }
            else {
                characterAnimator.SetBool("AltAttack", false);
                rig.health.immune = false;
            }

            if(_input.Fire.WasPressedThisFrame() && characterTweaks.canSwingSword) {
                SwingSword();
            }
        }

        private void Block() {
            characterAnimator.SetBool("AltAttack", true);
            rig.health.immune = true;
        }

        private void SwingSword() {
            characterAnimator.SetTrigger("Attack");
        }
    }
}