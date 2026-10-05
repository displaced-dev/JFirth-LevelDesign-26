using UnityEngine;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public class CharacterDataManager : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private Player player;
        [SerializeField] private Transform CharacterSpawnRoot;

        [Header("Config")]
        public CharacterDataSO fallbackCharacterData;

        [Header("Debug")]
        public CharacterDataSO currentCharacterData;
        public GameObject currentCharacterObject;
        public _MovementController currentMovementController;
        public RigInfo currentRigInfo;

        public void Initialize() {
            // TODO: Strip this code out if there ends up being a character select system
            // or need for no starting
            if(currentCharacterObject == null) {
                LoadCharacterData(fallbackCharacterData);
            }
        }

        public void LoadCharacterData(CharacterDataSO newCharacter){
            if(newCharacter == null || newCharacter.GameplayController == null) {
                Debug.LogError("Could Not Process Character Data Swap");
                return;
            }

            if(currentCharacterObject != null) {
                ClearCharacter();
            }

            currentCharacterData = newCharacter;
            GameObject characterObject = Instantiate(newCharacter.GameplayController, CharacterSpawnRoot);
            if(characterObject != null) {
                currentCharacterObject = characterObject;
            }

            LinkCharacterController();
        }

        private void LinkCharacterController() {
            if(currentCharacterObject != null && currentCharacterObject.TryGetComponent(out _MovementController tempController)) {
                currentMovementController = tempController;
                SpawnVisuals();
            }
        }
        
        private void SpawnVisuals() {
            if(currentRigInfo != null) { Destroy(currentRigInfo); }
            currentRigInfo = Instantiate(currentCharacterData.thirdPersonVisuals, currentMovementController._visualSpawnPoint);
            if(currentRigInfo != null) {
                currentRigInfo.playerAnimation.m_Controller = currentMovementController;
                currentRigInfo.health = currentMovementController.GetComponent<Health>();
            }  
        }

        public void ClearCharacter() {
            currentCharacterData = null;
            
            if(currentCharacterObject == null) {
                return;
            }
            else {
                Destroy(currentCharacterObject);
                currentCharacterObject = null;
            }
        }
    }
}
