using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("Disengage remade for ranged: strafes around the target while backing out to retreatDistance, facing it the whole time, " +
                 "and refuses steps that drop more than maxHeightDrop (so it won't walk off its high ground). Fails if cornered.")]
    public class MageReposition : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> retreatDistance = 8f;
        public BBParameter<float> retreatSpeed = 3f;
        public BBParameter<float> maxTime = 2f;
        public BBParameter<float> turnSpeed = 540f;

        [Header("Strafe")]
        public BBParameter<float> strafeWeight = 1f;
        public BBParameter<float> minStrafeTime = 1f;

        [Header("High Ground")]
        public BBParameter<float> maxHeightDrop = 0.5f;

        private float originalSpeed;
        private float side;
        private bool flipped;

        protected override string info { get { return "Reposition to " + retreatDistance + "m"; } }

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
            EnemyUtil.FaceTarget(agent.transform, targetPos, turnSpeed.value);

            var toSelf = selfPos - targetPos; toSelf.y = 0f;
            var dist = toSelf.magnitude;
            var away = dist > 0.01f ? toSelf / dist : -agent.transform.forward;

            if(elapsedTime >= maxTime.value || (elapsedTime >= minStrafeTime.value && dist >= retreatDistance.value)) { EndAction(true); return; }

            var tangent = Vector3.Cross(Vector3.up, away) * side;
            var outward = Mathf.Clamp01(retreatDistance.value - dist);
            var dir = (tangent * strafeWeight.value + away * outward).normalized;

            Vector3 safe;
            if(MageUtil.TryStep(agent, selfPos, selfPos + dir * 2f, maxHeightDrop.value, out safe)) { EnemyUtil.MoveTo(agent, safe); return; }

            if(!flipped) { side = -side; flipped = true; return; }

            if(MageUtil.TryStep(agent, selfPos, selfPos + away * 2f, maxHeightDrop.value, out safe)) { EnemyUtil.MoveTo(agent, safe); return; }
            EndAction(false);
        }

        protected override void OnPause()
        {
            EnemyUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            agent.updateRotation = true;
            agent.speed = originalSpeed;
            if(agent.isOnNavMesh) { agent.ResetPath(); }
        }
    }
}