using UnityEngine;
using LevelDesign.Gameplay.Levels;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class Player : MonoBehaviour
    {
        [Header("State Machine")]
        [SerializeField] private PlayerStateMachine PSM;
        
        [Header("Controllers")]
        [SerializeField] private PlayerCamera playerCamera;
        [Space]
        [SerializeField] private _MovementController playerCharacter;

        [Header("Managers")]
        [SerializeField] private CharacterDataManager characterDataM;
        [SerializeField] private UIManager uiM;
        [Space]
        [SerializeField] private CheckpointManager checkpointM;

        [Header("Events")]
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;

        private Transform cameraFocalTarget;
        private Transform spectatorCameraTarget;

        #region Unity Calls
        private void Start() {
            playerCamera.Initialize(PSM);
            characterDataM.Initialize();

            ScourScene();
        }

        private void Update() {
            RoutineChecks();
            ProcessControllers();
        }

        private void LateUpdate()
        {
            UpdateCameraTarget();
        }
        #endregion

        #region Controllers
        // Both
        public void ProcessControllers() {
            if(playerCamera != null)
            {
                playerCamera.UpdateCameraInput();
                playerCamera.UpdateRotation();
            }
            if(playerCharacter != null && playerCamera != null)
            {
                playerCharacter._UpdateBody(deltaTime: Time.deltaTime, playerCam: playerCamera.transform);
                playerCharacter._UpdateInput();
            }
        }

        // Camera Controller
        public void UpdateCameraTarget()
        {
            if (playerCharacter == null || playerCamera == null)
                return;

            if (!PSM.isCinematic)
            {
                spectatorCameraTarget = null;
                if(playerCharacter != null)
                {
                    cameraFocalTarget = playerCharacter._GetCameraTarget();
                }
                playerCamera.UpdatePosition(cameraFocalTarget);
            }
            else
            {
                if (spectatorCameraTarget == null)
                {
                    return;
                }

                if (spectatorCameraTarget != null)
                {
                    playerCamera.UpdatePositionSmooth(spectatorCameraTarget);
                    playerCamera.UpdateRotationSmooth(spectatorCameraTarget.transform.eulerAngles);
                }
            }
        }

        // Movement
        public void ClearMovement() { }
        #endregion

        #region Checks
        private void RoutineChecks() {
            // Character Data
            if(characterDataM.currentMovementController != null) {
                if(characterDataM.currentMovementController._isInitialized) { return; }

                playerCharacter = characterDataM.currentMovementController;

                if(playerCharacter._isInitialized) { return; }
                playerCharacter._Initialize(psm: PSM, characterdata: characterDataM.currentCharacterData);

                if(checkpointM != null) {
                    playerCharacter._Teleport(checkpointM.SpawnPoint.position);
                }
            }
        }

        private void ScourScene() {
            // Checkpoint Manager
            if(checkpointM == null) {
                checkpointM = FindFirstObjectByType<CheckpointManager>();

                if(checkpointM == null) {
                    Debug.LogWarning($"[{nameof(Player)}] No CheckpointManager found in scene.", this);
                }
            }
        }
        #endregion

        #region Public Accessors
        // Cinematic Controls
        public void SetAndLoadCinematic(Transform cinematicPosition) {
            spectatorCameraTarget = cinematicPosition;
            PSM.isCinematic = true;
        }

        public void ExitCinematic() {
            PSM.isCinematic = false;
        }

        public void RevivePlayer() {
            characterDataM.ClearCharacter();
            characterDataM.LoadCharacterData(characterDataM.fallbackCharacterData);
            e_playerkilled.RaiseReviveEvent();
        }
        #endregion
    }
}
