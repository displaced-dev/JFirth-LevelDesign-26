using UnityEngine;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class UIManager : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private GameObject playerDeadUI;
        [SerializeField] private GameObject levelCompleteUI;

        [Header("Events")]
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;
        [SerializeField] private LevelCompleteEventChannelSO e_gameOver;

        private void Start() {
            e_playerkilled.OnKillRequested += RespawnUI;
            e_gameOver.OnLevelComplete += GameOverUI;
        }

        private void OnDestroy() {
            e_playerkilled.OnKillRequested -= RespawnUI;
            e_gameOver.OnLevelComplete -= GameOverUI;
        }

        private void RespawnUI() {
            playerDeadUI.SetActive(true);
        }

        private void GameOverUI() {
            levelCompleteUI.SetActive(true);
        }
    }
}
