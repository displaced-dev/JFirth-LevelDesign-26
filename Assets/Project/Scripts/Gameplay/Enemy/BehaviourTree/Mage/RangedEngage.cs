using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("EngageTarget remade for ranged: chases until within castRange with line of sight, then strafes at preferredRange " +
                 "(without dropping off its current height) before succeeding.")]
    public class RangedEngage : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> chaseSpeed = 3.5f;
        public BBParameter<float> castRange = 14f;
        public BBParameter<float> preferredRange = 10f;
        public BBParameter<float> strafeSpeed = 2f;
        public BBParameter<float> strafeTime = 1.5f;
        public BBParameter<float> turnSpeed = 540f;

        [Header("Line Of Sight")]
        public BBParameter<bool> requireLineOfSight = true;
        public BBParameter<float> eyeHeight = 1.5f;
        public BBParameter<Vector3> targetOffset = Vector3.up;
        public BBParameter<LayerMask> obstacleMask = (LayerMask)Physics.DefaultRaycastLayers;

        [Header("High Ground")]
        public BBParameter<float> maxHeightDrop = 0.5f;

        private float originalSpeed;
        private bool strafing;
        private float strafeStart;
        private float strafeSign;
        private bool flipped;

        protected override string info { get { return "Ranged Engage " + target; } }

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
            var los = !requireLineOfSight.value ||
                      MageUtil.HasLineOfSight(selfPos + Vector3.up * eyeHeight.value, t, targetOffset.value, obstacleMask.value.value);

            if(dist > castRange.value || !los) {
                strafing = false;
                agent.updateRotation = true;
                agent.speed = chaseSpeed.value;
                MeleeUtil.MoveTo(agent, targetPos);
                return;
            }

            if(!strafing) {
                strafing = true;
                flipped = false;
                strafeStart = elapsedTime;
                strafeSign = Random.value < 0.5f ? -1f : 1f;
            }

            agent.updateRotation = false;
            agent.speed = strafeSpeed.value;
            MeleeUtil.FaceTarget(agent.transform, targetPos, turnSpeed.value);

            var toTarget = targetPos - selfPos; toTarget.y = 0f; toTarget.Normalize();
            var dir = Vector3.Cross(Vector3.up, toTarget) * strafeSign;
            if(dist < preferredRange.value * 0.75f) { dir -= toTarget * 0.75f; }
            else if(dist > preferredRange.value * 1.25f) { dir += toTarget * 0.5f; }

            Vector3 safe;
            if(MageUtil.TryStep(agent, selfPos, selfPos + dir.normalized * 1.5f, maxHeightDrop.value, out safe)) {
                MeleeUtil.MoveTo(agent, safe);
            }
            else if(!flipped) { strafeSign = -strafeSign; flipped = true; }
            else if(agent.isOnNavMesh) { agent.ResetPath(); }

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