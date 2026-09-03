using UnityEngine;

namespace LevelDesign.Systems
{
    [RequireComponent(typeof(SphereCollider))]
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Movement")]
        public float speed;

        [Header("Hit")]
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private float damage = 10f;

        [Header("Effects")]
        [SerializeField] private GameObject impactPrefab;
        [SerializeField] private float impactLifetime = 2f;

        [Header("Lifetime")]
        [SerializeField] private float maxLifetime = 5f;

        private bool hasHit;
        private readonly Collider[] hitBuffer = new Collider[8];

        public void Init(float projDamage = 0f, float projSpeed = 0f, float projLifetime = 0f)
        {
            if(projSpeed > 0f) { speed = projSpeed; }
            if(projDamage > 0f) { damage = projDamage; }
            if(projLifetime > 0f) { maxLifetime = projLifetime; }
        }

        private void Awake()
        {
            if (sphereCollider == null){
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

            if(speed != 0f) {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }

            CheckForHit();
        }

        private void CheckForHit()
        {
            Vector3 center = transform.TransformPoint(sphereCollider.center);
            float radius = sphereCollider.radius * MaxAbs(transform.lossyScale);

            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer, hitMask, QueryTriggerInteraction.Ignore);

            for(int i = 0; i < count; i++)
            {
                Collider other = hitBuffer[i];
                if (other == sphereCollider || other.transform.IsChildOf(transform)) continue;

                OnHit(other, center);
                return;
            }
        }

        private void OnHit(Collider other, Vector3 hitPoint)
        {
            hasHit = true;

            if(impactPrefab != null)
            {
                GameObject impact = Instantiate(impactPrefab, hitPoint, Quaternion.LookRotation(-transform.forward));
                Destroy(impact, impactLifetime);
            }

            Health health = other.GetComponent<Health>();
            if (health != null) {
                health.TakeDamage(damage);
                Debug.Log("Hit An Object With Health");
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