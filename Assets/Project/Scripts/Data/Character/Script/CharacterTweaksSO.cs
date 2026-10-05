using UnityEngine;
using LevelDesign.Systems.Player;

namespace LevelDesign.Data
{
    [CreateAssetMenu(fileName = "CharacterTweaks", menuName = "ScriptableObjects/Character/CharacterTweaksSO", order = 2)]
    public class CharacterTweaksSO : ScriptableObject
    {
        [Header("Mage - Options")]
        public bool canSwingSword;
        public bool canShieldBlock;

        [Header("SandS - Damage")]
        public float meleeDamage;

        [Header("Mage - Options")]
        public bool canCastDamage;
        public bool canCastHealing;

        [Header("Mage - Healing")]
        public float healAmount;
        public float healRate;

        [Header("Mage - Damage")]
        public float castDamage;
        public float castRate;
        public float castLinger;
        public float castVelocity;
    }
}
