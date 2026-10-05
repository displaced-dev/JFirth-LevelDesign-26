using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("Searches for a reachable spot higher than the target, inside cast range and ideally with line of sight, then paths there. " +
                 "Fails fast if nothing better is found or while the search is on cooldown, so the tree falls through to normal combat.")]
    public class SeekHighGround : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;

        [Header("Search")]
        public BBParameter<float> searchRadius = 15f;
        public BBParameter<float> minRange = 5f;
        public BBParameter<float> maxRange = 14f;
        public BBParameter<float> minAdvantage = 1.5f;
        public BBParameter<float> minGain = 0.75f;
        public BBParameter<int> samples = 24;
        public BBParameter<int> maxPathChecks = 6;
        public BBParameter<float> maxPathLength = 30f;
        public BBParameter<float> probeHeight = 12f;
        public BBParameter<LayerMask> groundMask = (LayerMask)Physics.DefaultRaycastLayers;
        public BBParameter<LayerMask> obstacleMask = (LayerMask)Physics.DefaultRaycastLayers;
        public BBParameter<float> eyeHeight = 1.5f;
        public BBParameter<Vector3> targetOffset = Vector3.up;
        public BBParameter<string> markerTag = "";
        public BBParameter<float> searchCooldown = 3f;

        [Header("Move")]
        public BBParameter<float> moveSpeed = 3.5f;
        public BBParameter<float> arriveDistance = 0.6f;
        public BBParameter<float> maxTime = 8f;
        public BBParameter<float> turnSpeed = 540f;
        [BlackboardOnly] public BBParameter<Vector3> saveDestinationAs;

        private float nextSearchTime;
        private float originalSpeed;
        private Vector3 destination;

        protected override string info { get { return "Seek High Ground over " + target; } }

        protected override void OnExecute()
        {
            if(target.value == null || !agent.isOnNavMesh) { EndAction(false); return; }
            if(Time.time < nextSearchTime) { EndAction(false); return; }
            nextSearchTime = Time.time + searchCooldown.value;

            var q = new HighGroundQuery {
                searchRadius = searchRadius.value,
                minRange = minRange.value,
                maxRange = maxRange.value,
                minAdvantage = minAdvantage.value,
                minGain = minGain.value,
                samples = Mathf.Max(4, samples.value),
                maxPathChecks = Mathf.Max(1, maxPathChecks.value),
                maxPathLength = maxPathLength.value,
                probeHeight = probeHeight.value,
                groundMask = groundMask.value.value,
                losMask = obstacleMask.value.value,
                eyeHeight = eyeHeight.value,
                targetOffset = targetOffset.value,
                markerTag = markerTag.value
            };

            if(!MageUtil.TryFindHighGround(agent, target.value, q, out destination)) { EndAction(false); return; }

            if(!saveDestinationAs.isNone) { saveDestinationAs.value = destination; }
            originalSpeed = agent.speed;
            agent.speed = moveSpeed.value;
            agent.updateRotation = true;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        protected override void OnUpdate()
        {
            if(elapsedTime >= maxTime.value) { EndAction(false); return; }
            if(agent.pathPending) { return; }

            if(agent.pathStatus == NavMeshPathStatus.PathInvalid) { EndAction(false); return; }

            var remaining = agent.remainingDistance;
            if(remaining <= arriveDistance.value) { EndAction(true); return; }

            if(remaining < 2.5f && target.value != null) {
                agent.updateRotation = false;
                MeleeUtil.FaceTarget(agent.transform, target.value.transform.position, turnSpeed.value);
            }
        }

        protected override void OnPause()
        {
            MeleeUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            agent.updateRotation = true;
            if(originalSpeed > 0f) { agent.speed = originalSpeed; }
            if(agent.isOnNavMesh) { agent.ResetPath(); }
        }
    }
}