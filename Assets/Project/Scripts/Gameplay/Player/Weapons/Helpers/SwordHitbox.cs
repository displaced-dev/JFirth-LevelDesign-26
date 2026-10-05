using System.Collections.Generic;
using UnityEngine;
using LevelDesign.Systems;

namespace LevelDesign.Systems.Player
{
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField] private Collider hitCollider;
        private Vector3 startingPos = new Vector3(0f,1f,1.58f); // I'm not happy with this. It's a cut corner as the rigidbody would slide around. 

        private void FixedUpdate() {
            hitCollider.gameObject.transform.localPosition = startingPos;
        }
        
        private void OnDisable() => hitCollider.enabled = false;

        public void BeginHit() {
            hitCollider.enabled = true;
        }

        public void EndHit() => hitCollider.enabled = false;
    }
}