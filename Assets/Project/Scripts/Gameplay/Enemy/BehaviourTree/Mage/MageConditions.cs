using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace LevelDesign.Systems.Enemy
{
    [Category("Mage")]
    [Description("True if this agent stands at least minAdvantage meters above the target. Tick 'Invert' to mean 'needs high ground'.")]
    public class HasHighGround : ConditionTask<Transform>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> minAdvantage = 1.5f;

        protected override string info { get { return "Above " + target + " by " + minAdvantage + "m"; } }

        protected override bool OnCheck()
        {
            if(target.value == null) { return false; }
            return agent.position.y - target.value.transform.position.y >= minAdvantage.value;
        }
    }

    [Category("Mage")]
    [Description("Linecast from eye height to the target. True if nothing on the obstacle mask is in the way.")]
    public class HasLineOfSight : ConditionTask<Transform>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> eyeHeight = 1.5f;
        public BBParameter<Vector3> targetOffset = Vector3.up;
        public BBParameter<LayerMask> obstacleMask = (LayerMask)Physics.DefaultRaycastLayers;

        protected override string info { get { return "LOS to " + target; } }

        protected override bool OnCheck()
        {
            return MageUtil.HasLineOfSight(agent.position + Vector3.up * eyeHeight.value, target.value, targetOffset.value, obstacleMask.value.value);
        }
    }
}
