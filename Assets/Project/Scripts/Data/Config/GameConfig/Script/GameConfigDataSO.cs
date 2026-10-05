using UnityEngine;
using System;
using LevelDesign.Systems.Player;

namespace LevelDesign.Data
{
    [CreateAssetMenu(fileName = "GameConfigData", menuName = "ScriptableObjects/Config/GameConfigData", order = 2)]
    public class GameConfigDataSO : ScriptableObject
    {
        public bool enemiesRespawn;
    }
}
