using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("AI version of the player's AltFire: channels the heal area (AltAttack bool) for healDuration while facing the target. Fails if healing is disabled in CharacterTweaks.")]
    public class MageHeal : ActionTask<NavMeshAgent>
    {
        public BBParameter<GameObject> target;
        public BBParameter<float> healDuration = 2f;
        public BBParameter<float> turnSpeed = 540f;
        [BlackboardOnly] public BBParameter<bool> isHealing;

        private EnemyMage mage;
        private bool paused;

        protected override string info { get { return "Heal for " + healDuration + "s"; } }

        protected override string OnInit()
        {
            mage = EnemyUtil.Find<EnemyMage>(agent);
            return mage == null ? "MageHeal needs an EnemyMage component on the agent." : null;
        }

        protected override void OnExecute()
        {
            paused = false;
            if(!mage.CanCastHealing) { EndAction(false); return; }
            EnemyUtil.Halt(agent);
            SetHealing(true);
        }

        protected override void OnUpdate()
        {
            if(paused) { paused = false; SetHealing(true); }

            if(target.value != null) {
                EnemyUtil.FaceTarget(agent.transform, target.value.transform.position, turnSpeed.value);
            }
            if(elapsedTime >= healDuration.value) { EndAction(true); }
        }

        protected override void OnPause()
        {
            paused = true;
            SetHealing(false);
            EnemyUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            SetHealing(false);
            EnemyUtil.Resume(agent);
        }

        private void SetHealing(bool on)
        {
            if(on) { mage.Heal(); } else { mage.StopHeal(); }
            if(!isHealing.isNone) { isHealing.value = on; }
        }
    }
}