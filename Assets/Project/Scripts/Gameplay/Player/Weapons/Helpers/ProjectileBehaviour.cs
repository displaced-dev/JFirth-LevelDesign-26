using UnityEngine;

namespace LevelDesign.Systems
{
    [RequireComponent(typeof(SphereCollider))]
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private GameObject impactPrefab;

        [Header("Config")]
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float impactLifetime = 2f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float speed;

        private bool hasHit;
        private Transform owner;
        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];

        public void Init(float projDamage = 0f, float projSpeed = 0f, float projLifetime = 0f, float impactLife = 0, Transform projOwner = null)
        {
            if(projSpeed > 0f) { speed = projSpeed; }
            if(projDamage > 0f) { damage = projDamage; }
            if(projLifetime > 0f) { maxLifetime = projLifetime; }
            if(impactLife > 0f) { impactLifetime = impactLife; }

            owner = projOwner;
        }

        private void Awake()
        {
            if(sphereCollider == null) {
                sphereCollider = GetComponent<SphereCollider>();
            }

            sphereCollider.isTrigger = true;
        }

        private void Start()
        {
            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            if(hasHit) { return; }

            float step = speed * Time.deltaTime;

            if(TryFindHit(Mathf.Max(step, 0.01f), out RaycastHit hit)) {
                OnHit(hit);
                return;
            }

            transform.position += transform.forward * step;
        }

        private bool TryFindHit(float distance, out RaycastHit closest)
        {
            closest = default;

            Vector3 origin = transform.TransformPoint(sphereCollider.center);
            float radius = sphereCollider.radius * MaxAbs(transform.lossyScale);

            int count = Physics.SphereCastNonAlloc(origin, radius, transform.forward, hitBuffer, distance, hitMask, QueryTriggerInteraction.Ignore);

            bool found = false;
            float bestDistance = float.MaxValue;

            for(int i = 0; i < count; i++)
            {
                RaycastHit h = hitBuffer[i];
                if(IsIgnored(h.collider)) { continue; }

                if(h.distance < bestDistance) {
                    bestDistance = h.distance;
                    closest = h;
                    found = true;
                }
            }

            if(found && closest.distance <= 0f) {
                closest.point = origin;
            }

            return found;
        }

        private bool IsIgnored(Collider other)
        {
            if(other == sphereCollider) { return true; }
            if(other.transform.IsChildOf(transform)) { return true; }
            if(owner != null && other.transform.IsChildOf(owner)) { return true; }
            return false;
        }

        private void OnHit(RaycastHit hit)
        {
            hasHit = true;
            transform.position = hit.point;

            if(impactPrefab != null)
            {
                Vector3 normal = hit.normal.sqrMagnitude > 0f ? hit.normal : -transform.forward;
                GameObject impact = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(normal));
                Destroy(impact, impactLifetime);
            }

            Health health = hit.collider.GetComponentInParent<Health>();
            if(health != null) {
                health.TakeDamage(damage);
                Debug.Log($"Hit {health.name} for {damage}");
            }

            Cleanup();
        }

        private void Cleanup()
        {
            Destroy(gameObject);
        }

        private static float MaxAbs(Vector3 v)
        {
            return Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
    }
}