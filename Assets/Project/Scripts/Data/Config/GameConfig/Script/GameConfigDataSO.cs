using UnityEngine;
using UnityEngine.Serialization;
using System;
using LevelDesign.Systems.Player;

namespace LevelDesign.Data
{
    [CreateAssetMenu(fileName = "GameConfigData", menuName = "ScriptableObjects/Config/GameConfigData", order = 2)]
    public class GameConfigDataSO : ScriptableObject
    {
        [Header("Enemies")]
        public bool enemiesRespawn;

        [Header("Enemy Mage - Damage")]
        public bool canCastDamage = true;
        public float castDamage;
        public float castRate;
        public float castVelocity;
        public float castLinger;

        [Header("Enemy Mage - Healing")]
        public bool canCastHealing = true;
        public float healAmount;
        public float healRate;
    }
}