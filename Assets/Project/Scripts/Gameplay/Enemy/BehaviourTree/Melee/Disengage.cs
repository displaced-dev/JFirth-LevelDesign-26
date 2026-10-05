using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Melee")]
    [Description("Strafes left or right around the target while backing out to retreatDistance, facing it the whole time. Picks a random side; flips if the NavMesh blocks that way.")]
    public class Disengage : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> retreatDistance = 4f;
        public BBParameter<float> retreatSpeed = 2.5f;
        public BBParameter<float> maxTime = 2f;
        public BBParameter<float> turnSpeed = 540f;

        [Header("Strafe")]
        public BBParameter<float> strafeWeight = 1f;
        public BBParameter<float> minStrafeTime = 1.2f;

        private float originalSpeed;
        private float side;
        private bool flipped;

        protected override string info { get { return "Strafe-Disengage to " + retreatDistance + "m"; } }

        protected override void OnExecute()
        {
            if(target.value == null) { EndAction(false); return; }
            originalSpeed = agent.speed;
            agent.updateRotation = false;
            agent.speed = retreatSpeed.value;
            side = Random.value < 0.5f ? -1f : 1f;
            flipped = false;
        }

        protected override void OnUpdate()
        {
            var t = target.value;
            if(t == null) { EndAction(false); return; }

            var selfPos = agent.transform.position;
            var targetPos = t.transform.position;
            MeleeUtil.FaceTarget(agent.transform, targetPos, turnSpeed.value);

            var toSelf = selfPos - targetPos; toSelf.y = 0f;
            var dist = toSelf.magnitude;
            var away = dist > 0.01f ? toSelf / dist : -agent.transform.forward;

            if(elapsedTime >= maxTime.value || (elapsedTime >= minStrafeTime.value && dist >= retreatDistance.value)) {
                EndAction(true);
                return;
            }

            var tangent = Vector3.Cross(Vector3.up, away) * side;
            var outward = Mathf.Clamp01(retreatDistance.value - dist);
            var dir = (tangent * strafeWeight.value + away * outward).normalized;
            var dest = selfPos + dir * 2f;

            NavMeshHit hit;
            if(NavMesh.Raycast(selfPos, dest, out hit, agent.areaMask)) {
                if(!flipped) { side = -side; flipped = true; return; }
                dest = hit.position;
            }

            MeleeUtil.MoveTo(agent, dest);
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