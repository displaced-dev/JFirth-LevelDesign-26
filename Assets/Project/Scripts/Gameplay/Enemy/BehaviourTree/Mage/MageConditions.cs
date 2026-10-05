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
}
