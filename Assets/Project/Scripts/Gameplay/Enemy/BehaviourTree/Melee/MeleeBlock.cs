using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    [Category("Melee")]
    [Description("Holds a block for blockDuration while facing the target.")]
    public class MeleeBlock : ActionTask<NavMeshAgent>
    {
        [RequiredField] public BBParameter<GameObject> target;
        public BBParameter<string> blockBool = "AltAttack";
        public BBParameter<float> blockDuration = 0.8f;
        public BBParameter<float> turnSpeed = 540f;
        [BlackboardOnly] public BBParameter<bool> isBlocking;

        private Animator anim;
        private bool paused;

        protected override string info { get { return "Block for " + blockDuration + "s"; } }

        protected override string OnInit()
        {
            anim = agent.GetComponentInChildren<Animator>();
            return null;
        }

        protected override void OnExecute()
        {
            paused = false;
            EnemyUtil.Halt(agent);
            SetBlocking(true);
        }

        protected override void OnUpdate()
        {
            if(paused) { paused = false; SetBlocking(true); }

            if(target.value != null) {
                EnemyUtil.FaceTarget(agent.transform, target.value.transform.position, turnSpeed.value);
            }
            if(elapsedTime >= blockDuration.value) { EndAction(true); }
        }

        protected override void OnPause()
        {
            paused = true;
            SetBlocking(false);
            EnemyUtil.Halt(agent);
        }

        protected override void OnStop()
        {
            SetBlocking(false);
            EnemyUtil.Resume(agent);
        }

        private void SetBlocking(bool on)
        {
            if(anim != null && !string.IsNullOrEmpty(blockBool.value)) { anim.SetBool(blockBool.value, on); }
            if(!isBlocking.isNone) { isBlocking.value = on; }
        }
    }
}