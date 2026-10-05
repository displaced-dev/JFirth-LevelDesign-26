using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    internal static class MeleeUtil
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

        public static bool IsInTaggedState(Animator anim, string tag, out int stateHash)
        {
            stateHash = 0;
            if(anim == null || string.IsNullOrEmpty(tag)) { return false; }
            var cur = anim.GetCurrentAnimatorStateInfo(0);
            if(cur.IsTag(tag)) { stateHash = cur.fullPathHash; return true; }
            if(anim.IsInTransition(0)) {
                var next = anim.GetNextAnimatorStateInfo(0);
                if(next.IsTag(tag)) { stateHash = next.fullPathHash; return true; }
            }
            return false;
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
            if(!agent.isOnNavMesh) { return; }
            NavMeshHit hit;
            if(NavMesh.SamplePosition(desired, out hit, 2f, NavMesh.AllAreas)) {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }
}