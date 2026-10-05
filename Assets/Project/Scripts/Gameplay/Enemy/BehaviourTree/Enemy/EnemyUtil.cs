using UnityEngine;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    internal static class EnemyUtil
    {
        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }

        public static void FaceTarget(Transform self, Vector3 targetPos, float degreesPerSecond)
        {
            var dir = targetPos - self.position;
            dir.y = 0f;
            if(dir.sqrMagnitude < 0.0001f) { return; }
            var look = Quaternion.LookRotation(dir);
            self.rotation = Quaternion.RotateTowards(self.rotation, look, degreesPerSecond * Time.deltaTime);
        }

        public static bool HasLineOfSight(Vector3 from, GameObject target, Vector3 targetOffset, int mask)
        {
            if(target == null) { return false; }
            var to = target.transform.position + targetOffset;
            RaycastHit hit;
            if(!Physics.Linecast(from, to, out hit, mask, QueryTriggerInteraction.Ignore)) { return true; }
            return hit.transform == target.transform || hit.transform.IsChildOf(target.transform);
        }

        public static T Find<T>(Component agent) where T : Component
        {
            if(agent == null) { return null; }
            var c = agent.GetComponent<T>();
            if(c == null) { c = agent.GetComponentInParent<T>(); }
            if(c == null) { c = agent.GetComponentInChildren<T>(); }
            return c;
        }

        public static void Halt(NavMeshAgent agent)
        {
            if(agent != null && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); }
        }

        public static void Resume(NavMeshAgent agent)
        {
            if(agent != null && agent.isOnNavMesh) { agent.isStopped = false; }
        }

        public static void MoveTo(NavMeshAgent agent, Vector3 desired)
        {
            if(agent == null || !agent.isOnNavMesh) { return; }
            NavMeshHit hit;
            if(NavMesh.SamplePosition(desired, out hit, 2f, agent.areaMask)) {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }
}
