using System;
using UnityEngine;

namespace LevelDesign.Data
{
    [CreateAssetMenu(fileName = "EventCompleteLevel", menuName = "ScriptableObjects/Events/Gameplay/LevelComplete", order = 1)]
    public class LevelCompleteEventChannelSO : ScriptableObject
    {
        public event Action OnLevelComplete;

        public void RaiseEvent()
        {
            OnLevelComplete?.Invoke();
        }
    }
}
