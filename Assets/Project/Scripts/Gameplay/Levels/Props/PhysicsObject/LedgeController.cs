using UnityEngine;
using LevelDesign.Data;

namespace LevelDesign.Gameplay.Levels
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class LedgeController : MonoBehaviour
    {
        [Header("Ledge Guard")]
        [SerializeField] private bool protectFromledges = true;
        [SerializeField] private float maxDropHeight = 0.6f;
        [SerializeField] private float maxSlopeAngle = 50f;
        [SerializeField] private float pushBack = 0.02f;
        [SerializeField] private float skin = 0.05f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float lowestYPoint = -50f;
 
        [Header("Capsule Probing")]
        [SerializeField] private int capsuleProbeCount = 8;
 
        [Header("Debug")]
        [SerializeField] private bool debugDrawProbes = false;

        [Header("Events")]
        [SerializeField] private KillPlayerEventChannelSO e_playerKilled; 
        
        private Rigidbody rb;
        private Collider col;
        private Vector3 resetPosition;
 
        void OnEnable() {
            if(e_playerKilled != null) {
                e_playerKilled.OnKillRequested += ResetObject;
            }
        }

        void OnDisable() {
            if(e_playerKilled != null) {
                e_playerKilled.OnKillRequested -= ResetObject;
            }
        }
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
 
            resetPosition = transform.position;
        }
 
        void FixedUpdate()
        {
            if(transform.position.y < lowestYPoint) {
                ResetObject();
                return;
            }

            if(!protectFromledges) {
                return;
            }
 
            int mask = groundMask & ~(1 << gameObject.layer);
 
            Bounds b = col.bounds; // world-space AABB, rotation-proof
            Vector3 min = b.min;
            Vector3 center = b.center;
            float tanSlope = Mathf.Tan(maxSlopeAngle * Mathf.Deg2Rad);
 
            Vector3[] probes = BuildProbes(b);
 
            Vector3 correction = Vector3.zero;
            int offenders = 0;
 
            foreach(Vector3 v in probes)
            {
                float rXZ = new Vector2(v.x - center.x, v.z - center.z).magnitude;
                float slopeAllowance = rXZ * tanSlope;
 
                float originY = min.y + slopeAllowance + skin;
                Vector3 origin = new Vector3(v.x, originY, v.z);
 
                float rayLength = (originY - min.y) + slopeAllowance + maxDropHeight;
 
                if(debugDrawProbes) { Debug.DrawRay(origin, Vector3.down * rayLength, Color.red); }
 
                if(Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, mask, QueryTriggerInteraction.Ignore))
                {
                    float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if(groundAngle <= maxSlopeAngle)
                    {
                        continue;
                    }
                }
 
                Vector3 outward = v - rb.position;
                outward.y = 0f;
                if(outward.sqrMagnitude < 0.0001f) { continue; }
                outward.Normalize();
 
                float outwardSpeed = Vector3.Dot(rb.linearVelocity, outward);
                if(outwardSpeed > 0f)
                {
                    rb.linearVelocity -= outward * outwardSpeed;
                }
 
                correction -= outward;
                offenders++;
            }
 
            if(offenders > 0)
            {
                Vector3 delta = Vector3.ClampMagnitude(correction / offenders, 1f) * pushBack;
                rb.MovePosition(rb.position + delta);
            }
        }

        private void ResetObject() {
            rb.position = resetPosition;
        }
 
        private Vector3[] BuildProbes(Bounds b)
        {
            Vector3 min = b.min;
            Vector3 max = b.max;
            Vector3 center = b.center;
 
            if(col is CapsuleCollider)
            {
                float radius = Mathf.Min(b.extents.x, b.extents.z);
 
                int count = Mathf.Max(3, capsuleProbeCount);
                Vector3[] ring = new Vector3[count];
                for(int i = 0; i < count; i++)
                {
                    float ang = (Mathf.PI * 2f / count) * i;
                    ring[i] = new Vector3(
                        center.x + Mathf.Cos(ang) * radius,
                        min.y,
                        center.z + Mathf.Sin(ang) * radius);
                }
                return ring;
            }
 
            return new Vector3[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, max.z),
            };
        }
    }
}