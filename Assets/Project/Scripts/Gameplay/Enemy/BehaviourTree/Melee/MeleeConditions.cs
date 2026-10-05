using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Melee")]
    [Description("True if the target is within range (horizontal distance).")]
    public class IsTargetInRange : ConditionTask<Transform>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<float> range = 2f;

        protected override string info { get { return target + " within " + range + "m"; } }

        protected override bool OnCheck()
        {
            if(target.value == null) { return false; }
            return MeleeUtil.FlatDistance(agent.position, target.value.transform.position) <= range.value;
        }
    }

    [Category("Melee")]
    [Description("True when Time.time has passed the stored next-attack time.")]
    public class AttackReady : ConditionTask
    {
        public BBParameter<float> nextAttackTime;

        protected override string info { get { return "Attack Ready (" + nextAttackTime + ")"; } }

        protected override bool OnCheck() { return Time.time >= nextAttackTime.value; }
    }

    [Category("Melee")]
    [Description("Random roll. True with the given probability each time it is checked.")]
    public class Chance : ConditionTask
    {
        [SliderField(0f, 1f)] public BBParameter<float> probability = 0.35f;

        protected override string info { get { return Mathf.RoundToInt(probability.value * 100f) + "% Chance"; } }

        protected override bool OnCheck() { return Random.value <= probability.value; }
    }
}