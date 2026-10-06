using System;
using UnityEngine;

namespace LevelDesign.Data
{
    [CreateAssetMenu(fileName = "EventStamina", menuName = "ScriptableObjects/Events/Stamina", order = 3)]
    public class StaminaEventChannelSO : ScriptableObject
    {
        public Func<float, bool> OnSpendRequested;

        public bool SpendStamina(float amount)
        {
            return OnSpendRequested != null && OnSpendRequested(amount);
        }
    }
}
