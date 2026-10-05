using UnityEngine;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy {
    public class EnemyBaseAnimator : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private NavMeshAgent agent;
        public Animator animator;

        [Header("Config")]
        [SerializeField] private float dampTime = 0.1f;
        [SerializeField] private float animForwardSpeed = 3.5f;

        private static readonly int XVal = Animator.StringToHash("xVal");
        private static readonly int YVal = Animator.StringToHash("yVal");

        private void Update()
        {
            Vector3 worldVel = agent.desiredVelocity;
            Vector3 localVel = transform.InverseTransformDirection(worldVel);

            float x = localVel.x / animForwardSpeed;
            float y = localVel.z / animForwardSpeed;

            animator.SetFloat(XVal, x, dampTime, Time.deltaTime);
            animator.SetFloat(YVal, y, dampTime, Time.deltaTime);
        }

        public void Die()    => animator.SetBool("Death", true);
        public void Revive() => animator.SetTrigger("Revive");
    }
}