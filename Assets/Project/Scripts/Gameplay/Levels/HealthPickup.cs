using UnityEngine;
using LevelDesign.Data;

namespace LevelDesign.Systems.Level
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private GameObject healthpackObj;

        [Header("Event")]
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;

        private void OnEnable() {
            if(e_playerkilled != null) {
                e_playerkilled.OnReviveRequested += Reset;
            }
        }

        private void OnDisable() {
            if(e_playerkilled != null) {
                e_playerkilled.OnReviveRequested -= Reset;
            }
        }

        private void Reset() {
            healthpackObj.SetActive(true);
        }
    }
}
