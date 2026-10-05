using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Melee")]
    [Description("Chases target, then strafes around it at attack range before succeeding.")]
    public class EngageTarget : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> chaseSpeed = 4f;
        public BBParameter<float> attackRange = 2f;
        public BBParameter<float> strafeSpeed = 1.5f;
        public BBParameter<float> strafeTime = 1.5f;
        public BBParameter<float> turnSpeed = 540f;

        private float originalSpeed;
        private bool strafing;
        private float strafeStart;
        private float strafeSign;

        protected override string info { get { return "Engage " + target; } }

        protected override void OnExecute()
        {
            if(target.value == null) { EndAction(false); return; }
            originalSpeed = agent.speed;
            strafing = false;
        }

        protected override void OnUpdate()
        {
            var t = target.value;
            if(t == null) { EndAction(false); return; }

            var selfPos = agent.transform.position;
            var targetPos = t.transform.position;
            var dist = MeleeUtil.FlatDistance(selfPos, targetPos);

            if(dist > attackRange.value) {
                strafing = false;
                agent.updateRotation = true;
                agent.speed = chaseSpeed.value;
                MeleeUtil.MoveTo(agent, targetPos);
                return;
            }

            if(!strafing) {
                strafing = true;
                strafeStart = elapsedTime;
                strafeSign = Random.value < 0.5f ? -1f : 1f;
            }

            agent.updateRotation = false;
            agent.speed = strafeSpeed.value;
            MeleeUtil.FaceTarget(agent.transform, targetPos, turnSpeed.value);

            var toTarget = targetPos - selfPos; toTarget.y = 0f; toTarget.Normalize();
            var side = Vector3.Cross(Vector3.up, toTarget) * strafeSign;
            var desired = selfPos + side * 1.5f;
            if(dist < attackRange.value * 0.6f) { desired -= toTarget * 1f; }
            MeleeUtil.MoveTo(agent, desired);

            if(elapsedTime - strafeStart >= strafeTime.value) { EndAction(true); }
        }

        protected override void OnPause()
        {
            MeleeUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            agent.updateRotation = true;
            agent.speed = originalSpeed;
            if(agent.isOnNavMesh) { agent.ResetPath(); }
        }
    }
}