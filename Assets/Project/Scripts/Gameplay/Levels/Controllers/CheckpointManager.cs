using System.Collections.Generic;
using UnityEngine;
using LevelDesign.Tags;
using LevelDesign.Data;

// Summary: 
// A System devised to start / load the player, and update points in which the player should respawn to

namespace LevelDesign.Gameplay.Levels
{
    public class CheckpointManager : MonoBehaviour
    {
        public Transform SpawnPoint { get; private set; }
        public int CurrentCheckpointID => currentCheckpointID;
        public ProgressionDataSO progression;

        [Header("Debug")]
        [SerializeField] private int currentCheckpointID = -1;
        [SerializeField] private TAG_Checkpoint[] checkpoints;
        [SerializeField] private TAG_CheckpointOverride[] checkpointOverrides;

        private readonly Dictionary<int, TAG_Checkpoint> checkpointLookup = new Dictionary<int, TAG_Checkpoint>();
        private int overrideIndex = -1;

        private void Awake() {
            ScourScene();
        }

        private void ScourScene() {
            checkpoints = FindObjectsByType<TAG_Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            checkpointOverrides = FindObjectsByType<TAG_CheckpointOverride>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            BuildLookup();
            DefineSpawnCheckpoint();
        }

        private void BuildLookup() {
            checkpointLookup.Clear();

            foreach(TAG_Checkpoint checkpoint in checkpoints) {
                checkpoint.OnReached += HandleCheckpointReached;

                if(checkpointLookup.ContainsKey(checkpoint.checkpointID)) {
                    Debug.LogWarning($"[CheckpointManager] Duplicate checkpointID {checkpoint.checkpointID} on '{checkpoint.name}' — ignoring.", checkpoint);
                    continue;
                }

                checkpointLookup.Add(checkpoint.checkpointID, checkpoint);
            }
        }

        private void DefineSpawnCheckpoint() {
            if(checkpointOverrides != null && checkpointOverrides.Length > 0) {
                overrideIndex = 0;
                SetSpawnPoint(checkpointOverrides[overrideIndex].transform);
                return;
            }

            // Default to the checkpoint with the lowest ID
            int lowestID = int.MaxValue;
            foreach(int id in checkpointLookup.Keys) {
                if(id < lowestID) { lowestID = id; }
            }

            if(lowestID != int.MaxValue) {
                SetActiveCheckpoint(lowestID);
            }
        }

        private void HandleCheckpointReached(TAG_Checkpoint checkpoint) {
            SetActiveCheckpoint(checkpoint.checkpointID);
        }

        #region Public Accessors
        public void SetActiveCheckpoint(int checkpointID) {
            if(!checkpointLookup.TryGetValue(checkpointID, out TAG_Checkpoint checkpoint)) {
                Debug.LogWarning($"[CheckpointManager] No checkpoint registered with ID {checkpointID}.");
                return;
            }

            currentCheckpointID = checkpointID;
            progression.checkpoint = checkpointID;
            SetSpawnPoint(checkpoint.transform);
        }

        public void CycleOverrideCheckpoint() {
            if(checkpointOverrides == null || checkpointOverrides.Length == 0) { return; }

            overrideIndex = (overrideIndex + 1) % checkpointOverrides.Length;
            SetSpawnPoint(checkpointOverrides[overrideIndex].transform);
        }
        #endregion

        private void SetSpawnPoint(Transform spawnPoint) {
            if(spawnPoint == null) { return; }

            SpawnPoint = spawnPoint;
        }

        public void OnDestroy() {
            foreach(TAG_Checkpoint checkpoint in checkpoints) {
                if(checkpoint != null) {
                    checkpoint.OnReached -= HandleCheckpointReached;
                }
            }
        }
    }
}