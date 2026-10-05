using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LevelDesign.Data;
using NodeCanvas.BehaviourTrees;

namespace LevelDesign.Systems.Enemy
{
    public enum EnemyType{
        Melee,
        Mage
    }

    public class EnemyController : MonoBehaviour
    {
        [Header("Config")]
        public EnemyType enemyType;
        [SerializeField] private int associatedCheckpoint;

        [Header("Scene Refs")]
        [SerializeField] private EnemyBaseAnimator enemyAnimator;
        [SerializeField] private Collider enemyCollider;
        [SerializeField] private UnityEngine.AI.NavMeshAgent enemyAgent;
        [SerializeField] private BehaviourTreeOwner behaviourTree;
        [SerializeField] private Health healthComponent;
        [SerializeField] private List<GameObject> meleeObjects;
        [SerializeField] private List<GameObject> mageObjects; 

        [Header("Data")]
        [SerializeField] private ProgressionDataSO progression;
        [SerializeField] private GameConfigDataSO gameConfig;
        [SerializeField] private BehaviourTree meleeBehaviour;
        [SerializeField] private BehaviourTree mageBehaviour;

        [Header("Events")]
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;

        private Vector3 startPos;

        public void OnEnable() {
            SetupCharacter();

            startPos = enemyCollider.gameObject.transform.position;
            
            if(gameConfig.enemiesRespawn) {
                e_playerkilled.OnReviveRequested += Revive;
            }
        }
        
        public void OnDisable() {
            if(gameConfig.enemiesRespawn) {
                e_playerkilled.OnReviveRequested -= Revive;
            }
        }

        public void Revive() {
            if(progression.checkpoint > associatedCheckpoint || progression.checkpoint != -1) { return; }

            enemyCollider.gameObject.transform.position = startPos;

            enemyCollider.enabled = true;
            enemyAgent.isStopped = false;
            enemyAnimator.Revive();
            
            SetupCharacter();
        }

        public void SetupCharacter() {
            healthComponent.ResetHealth();
            switch(enemyType) {
                case EnemyType.Melee: 
                    ToggleObjects(meleeObjects, true);
                    ToggleObjects(mageObjects, false);

                    behaviourTree.behaviour = meleeBehaviour;
                    enemyAnimator.animator.SetInteger("Weapon", 2);
                    break;
                case EnemyType.Mage:
                    ToggleObjects(meleeObjects, false);
                    ToggleObjects(mageObjects, true);

                    behaviourTree.behaviour = mageBehaviour;
                    enemyAnimator.animator.SetInteger("Weapon", 1);
                    break;
            }

            behaviourTree.enabled = true;
            behaviourTree.RestartBehaviour();
        }

        public void ToggleObjects(List<GameObject> itemsList, bool state) {
            foreach(GameObject item in itemsList) { item.SetActive(state); }
        }

        public void Die() {
            behaviourTree.StopBehaviour();
            behaviourTree.enabled = false;
 
            enemyAnimator.animator.ResetTrigger("Attack");
            enemyAnimator.animator.SetBool("AltAttack", false);
 
            if(enemyAgent.isOnNavMesh) {
                enemyAgent.isStopped = true;
                enemyAgent.ResetPath();
            }
            enemyCollider.enabled = false;
 
            enemyAnimator.Die();
        }
    }
}
