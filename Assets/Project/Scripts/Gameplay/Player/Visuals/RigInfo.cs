using UnityEngine;

// Summary
// Script used for storing and accessing components on the visualized rig

namespace LevelDesign.Systems.Player
{
    public class RigInfo : MonoBehaviour
    {
        [Header("Scene Refs")]
        public Transform followerConstraint;
        public WeaponController handRoot;
        public Animator characterAnimator;
        public _PlayerAnimation playerAnimation;
        [Space]
        public Health health;
    }
}
