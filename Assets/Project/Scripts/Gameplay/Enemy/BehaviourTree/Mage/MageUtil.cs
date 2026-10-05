using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LevelDesign.Systems.Enemy
{
    internal struct HighGroundQuery
    {
        public float searchRadius;
        public float minRange;
        public float maxRange;
        public float minAdvantage;
        public float minGain;
        public int samples;
        public int maxPathChecks;
        public float maxPathLength;
        public float probeHeight;
        public int groundMask;
        public int losMask;
        public float eyeHeight;
        public Vector3 targetOffset;
        public string markerTag;
    }

    internal static class MageUtil
    {
        private static readonly List<KeyValuePair<float, Vector3>> scored = new List<KeyValuePair<float, Vector3>>(64);
        private static NavMeshPath path;

        public static bool HasLineOfSight(Vector3 from, GameObject target, Vector3 targetOffset, int mask)
        {
            if(target == null) { return false; }
            var to = target.transform.position + targetOffset;
            RaycastHit hit;
            if(!Physics.Linecast(from, to, out hit, mask, QueryTriggerInteraction.Ignore)) { return true; }
            return hit.transform == target.transform || hit.transform.IsChildOf(target.transform);
        }

        public static bool TryStep(NavMeshAgent agent, Vector3 from, Vector3 dest, float maxDrop, out Vector3 safe)
        {
            safe = from;
            NavMeshHit hit;
            if(NavMesh.Raycast(from, dest, out hit, agent.areaMask)) { dest = hit.position; }
            if(MeleeUtil.FlatDistance(from, dest) < 0.3f) { return false; }
            if(!NavMesh.SamplePosition(dest, out hit, 1.5f, agent.areaMask)) { return false; }
            if(hit.position.y < from.y - maxDrop) { return false; }
            safe = hit.position;
            return true;
        }

        public static bool TryFindHighGround(NavMeshAgent agent, GameObject target, HighGroundQuery q, out Vector3 best)
        {
            best = Vector3.zero;
            if(agent == null || target == null || !agent.isOnNavMesh) { return false; }

            var self = agent.transform.position;
            var targetPos = target.transform.position;
            var topY = Mathf.Max(self.y, targetPos.y) + q.probeHeight;
            var requiredY = targetPos.y + Mathf.Max(q.minAdvantage, (self.y - targetPos.y) + q.minGain);

            scored.Clear();

            for(int i = 0; i < q.samples; i++) {
                bool aroundTarget = (i % 2) == 0;
                var center = aroundTarget ? targetPos : self;
                var dir2 = Random.insideUnitCircle.normalized;
                float r = aroundTarget ? Random.Range(q.minRange, q.maxRange) : Random.Range(2f, q.searchRadius);
                var p = center + new Vector3(dir2.x, 0f, dir2.y) * r;
                Vector3 v;
                if(TryProjectToTop(agent, p, topY, q, out v)) { Score(agent, target, v, self, targetPos, requiredY, q); }
            }

            if(!string.IsNullOrEmpty(q.markerTag)) {
                GameObject[] markers = null;
                try { markers = GameObject.FindGameObjectsWithTag(q.markerTag); } catch(UnityException) { }
                if(markers != null) {
                    foreach(var m in markers) {
                        var mp = m.transform.position;
                        if(MeleeUtil.FlatDistance(mp, self) > q.searchRadius && MeleeUtil.FlatDistance(mp, targetPos) > q.maxRange) { continue; }
                        NavMeshHit nh;
                        if(NavMesh.SamplePosition(mp, out nh, 1.5f, agent.areaMask)) {
                            Score(agent, target, nh.position, self, targetPos, requiredY, q, 2f);
                        }
                    }
                }
            }

            if(scored.Count == 0) { return false; }
            scored.Sort((a, b) => b.Key.CompareTo(a.Key));

            if(path == null) { path = new NavMeshPath(); }
            int checks = 0;
            foreach(var kv in scored) {
                if(checks++ >= q.maxPathChecks) { break; }
                if(!agent.CalculatePath(kv.Value, path) || path.status != NavMeshPathStatus.PathComplete) { continue; }
                if(PathLength(path) > q.maxPathLength) { continue; }
                best = kv.Value;
                return true;
            }
            return false;
        }

        private static bool TryProjectToTop(NavMeshAgent agent, Vector3 p, float topY, HighGroundQuery q, out Vector3 v)
        {
            v = p;
            RaycastHit hit;
            var origin = new Vector3(p.x, topY, p.z);
            if(!Physics.Raycast(origin, Vector3.down, out hit, q.probeHeight * 3f, q.groundMask, QueryTriggerInteraction.Ignore)) { return false; }
            NavMeshHit nh;
            if(!NavMesh.SamplePosition(hit.point, out nh, 1f, agent.areaMask)) { return false; }
            if(Mathf.Abs(nh.position.y - hit.point.y) > 0.75f) { return false; }
            v = nh.position;
            return true;
        }

        private static void Score(NavMeshAgent agent, GameObject target, Vector3 c, Vector3 self, Vector3 targetPos, float requiredY, HighGroundQuery q, float bonus = 0f)
        {
            if(c.y < requiredY) { return; }
            var flat = MeleeUtil.FlatDistance(c, targetPos);
            if(flat < q.minRange || flat > q.maxRange) { return; }

            float score = (c.y - targetPos.y) * 2f - MeleeUtil.FlatDistance(self, c) * 0.25f + bonus;
            if(HasLineOfSight(c + Vector3.up * q.eyeHeight, target, q.targetOffset, q.losMask)) { score += 4f; }
            scored.Add(new KeyValuePair<float, Vector3>(score, c));
        }

        private static float PathLength(NavMeshPath p)
        {
            float len = 0f;
            var c = p.corners;
            for(int i = 1; i < c.Length; i++) { len += Vector3.Distance(c[i - 1], c[i]); }
            return len;
        }
    }
}