using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Melee")]
    [Description("Stops, faces the target during wind-up, fires an Animator trigger, and waits for the attack duration. Fails immediately while on cooldown.")]
    public class MeleeAttack : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;

        [Header("Animation")]
        public BBParameter<string> attackTrigger = "Attack";
        public BBParameter<float> attackDuration = 1.2f;

        [Header("Facing")]
        public BBParameter<float> faceDuration = 0.5f;
        public BBParameter<float> turnSpeed = 720f;

        [Header("Cooldown")]
        public BBParameter<float> cooldown = 1.5f;
        public BBParameter<float> cooldownVariance = 0.75f;
        [BlackboardOnly] public BBParameter<float> saveNextAttackTimeAs;

        private Animator anim;
        private float nextAttackTime;
        private bool interrupted;

        protected override string OnInit()
        {
            anim = agent.GetComponentInChildren<Animator>();
            return null;
        }

        protected override void OnExecute()
        {
            interrupted = false;
            if(target.value == null) { EndAction(false); return; }

            if(Time.time < nextAttackTime) { EndAction(false); return; }

            EnemyUtil.Halt(agent);
            if(anim != null && !string.IsNullOrEmpty(attackTrigger.value)) { anim.SetTrigger(attackTrigger.value); }
        }

        protected override void OnUpdate()
        {
            if(interrupted) { EndAction(false); return; }

            if(target.value != null && elapsedTime < faceDuration.value) {
                EnemyUtil.FaceTarget(agent.transform, target.value.transform.position, turnSpeed.value);
            }

            if(elapsedTime >= attackDuration.value) { EndAction(true); }
        }

        protected override void OnPause()
        {
            interrupted = true;
            if(anim != null && !string.IsNullOrEmpty(attackTrigger.value)) { anim.ResetTrigger(attackTrigger.value); }
            EnemyUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            if(anim != null && !string.IsNullOrEmpty(attackTrigger.value)) { anim.ResetTrigger(attackTrigger.value); }

            if(elapsedTime > 0f) {
                nextAttackTime = Time.time + cooldown.value + Random.Range(0f, cooldownVariance.value);
                if(!saveNextAttackTimeAs.isNone) { saveNextAttackTimeAs.value = nextAttackTime; }
            }

            EnemyUtil.Resume(agent);
        }
    }
}