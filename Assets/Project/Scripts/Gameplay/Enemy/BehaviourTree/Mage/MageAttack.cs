using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("Stops, faces the target, fires an Animator trigger, then casts the projectile (via EnemyMage) at castDelay. Leads moving targets. Fails immediately while on cooldown.")]
    public class MageAttack : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;

        [Header("Animation")]
        public BBParameter<string> attackTrigger = "Attack";
        public BBParameter<float> castDelay = 0.35f;
        public BBParameter<float> attackDuration = 0.9f;

        [Header("Aim")]
        public BBParameter<Vector3> aimOffset = Vector3.up;
        [SliderField(0f, 1f)] public BBParameter<float> leadFactor = 0.5f;
        public BBParameter<float> turnSpeed = 720f;

        [Header("Cooldown")]
        public BBParameter<float> cooldown = 1.5f;
        public BBParameter<float> cooldownVariance = 0.75f;
        [BlackboardOnly] public BBParameter<float> saveNextAttackTimeAs;

        private EnemyMage mage;
        private float nextAttackTime;
        private bool interrupted;
        private bool fired;
        private Vector3 lastTargetPos;
        private Vector3 targetVel;

        protected override string info { get { return "Cast at " + target; } }

        protected override string OnInit()
        {
            mage = agent.GetComponent<EnemyMage>();
            if(mage == null) { mage = agent.GetComponentInParent<EnemyMage>(); }
            if(mage == null) { mage = agent.GetComponentInChildren<EnemyMage>(); }
            return mage == null ? "MageAttack needs an EnemyMage component on the agent." : null;
        }

        protected override void OnExecute()
        {
            interrupted = false;
            fired = false;
            if(target.value == null) { EndAction(false); return; }
            if(Time.time < nextAttackTime || !mage.CanCastDamage) { EndAction(false); return; }

            MeleeUtil.Halt(agent);
            lastTargetPos = target.value.transform.position;
            targetVel = Vector3.zero;
            mage.SetTrigger(attackTrigger.value);
        }

        protected override void OnUpdate()
        {
            if(interrupted) { EndAction(false); return; }
            var t = target.value;

            if(t != null) {
                var tp = t.transform.position;
                if(Time.deltaTime > 0f) { targetVel = Vector3.Lerp(targetVel, (tp - lastTargetPos) / Time.deltaTime, 0.5f); }
                lastTargetPos = tp;
                if(!fired) { MeleeUtil.FaceTarget(agent.transform, tp, turnSpeed.value); }
            }

            if(!fired && elapsedTime >= castDelay.value) {
                if(t == null) { EndAction(false); return; }
                Fire(t);
            }

            if(elapsedTime >= attackDuration.value) { EndAction(fired); }
        }

        private void Fire(GameObject t)
        {
            var aim = t.transform.position + aimOffset.value;
            var speed = mage.ProjectileSpeed;
            if(leadFactor.value > 0f && speed > 0.01f) {
                var travel = Vector3.Distance(mage.FirePoint.position, aim) / speed;
                var v = targetVel; v.y = 0f;
                aim += v * travel * leadFactor.value;
            }
            fired = mage.SpawnSpell(aim);
        }

        protected override void OnPause()
        {
            interrupted = true;
            mage.ResetTrigger(attackTrigger.value);
            MeleeUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            mage.ResetTrigger(attackTrigger.value);

            if(fired) {
                nextAttackTime = Time.time + Mathf.Max(cooldown.value, mage.CastRate) + Random.Range(0f, cooldownVariance.value);
                if(!saveNextAttackTimeAs.isNone) { saveNextAttackTimeAs.value = nextAttackTime; }
            }

            MeleeUtil.Resume(agent);
        }
    }
}