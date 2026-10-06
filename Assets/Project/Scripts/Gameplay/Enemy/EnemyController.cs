using UnityEngine;
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

        private const string HealthPercentParam = "HealthPercent";

        private Vector3 startPos;
        private bool started;
        private float lastHealthPercent = -1f;

        private void Start() {
            started = true;
            startPos = enemyCollider.gameObject.transform.position;
            SetupCharacter();
        }

        public void OnEnable() {
            if(gameConfig.enemiesRespawn) {
                e_playerkilled.OnReviveRequested += Revive;
            }
        }

        private void Update() {
            PushHealthPercent(false);
        }
        
        public void OnDisable() {
            if(gameConfig.enemiesRespawn) {
                e_playerkilled.OnReviveRequested -= Revive;
            }
        }

        public void Revive() {
            if(progression.checkpoint > associatedCheckpoint || associatedCheckpoint == -1) { return; }

            enemyCollider.gameObject.transform.position = startPos;

            enemyCollider.enabled = true;
            enemyAgent.enabled = true;
            if(enemyAgent.isOnNavMesh) { enemyAgent.isStopped = false; }
            enemyAnimator.Revive();
            
            SetupCharacter();
        }

        public void SetupCharacter() {
            healthComponent.ResetHealth();
            switch(enemyType) {
                case EnemyType.Melee: 
                    ToggleObjects(meleeObjects, true);
                    ToggleObjects(mageObjects, false);

                    SetBehaviour(meleeBehaviour);
                    enemyAnimator.animator.SetInteger("Weapon", 2);
                    break;
                case EnemyType.Mage:
                    ToggleObjects(meleeObjects, false);
                    ToggleObjects(mageObjects, true);

                    SetBehaviour(mageBehaviour);
                    enemyAnimator.animator.SetInteger("Weapon", 1);
                    break;
            }

            behaviourTree.enabled = true;
            behaviourTree.StartBehaviour();
            PushHealthPercent(true);
        }

        private void PushHealthPercent(bool force) {
            if(behaviourTree == null || !behaviourTree.isRunning) { return; }

            float percent = healthComponent.Normalized * 100f;
            if(!force && Mathf.Approximately(percent, lastHealthPercent)) { return; }

            lastHealthPercent = percent;
            behaviourTree.SetExposedParameterValue(HealthPercentParam, percent);
        }

        private void SetBehaviour(BehaviourTree tree) {
            StopTree();
            behaviourTree.behaviour = tree;
        }

        private void StopTree() {
            if(behaviourTree != null && behaviourTree.isRunning) { behaviourTree.StopBehaviour(); }
        }

        public void ToggleObjects(List<GameObject> itemsList, bool state) {
            foreach(GameObject item in itemsList) { item.SetActive(state); }
        }

        public void Die() {
            StopTree();
            behaviourTree.enabled = false;
 
            enemyAnimator.animator.ResetTrigger("Attack");
            enemyAnimator.animator.SetBool("AltAttack", false);
 
            if(enemyAgent.isOnNavMesh) {
                enemyAgent.isStopped = true;
                enemyAgent.ResetPath();
            }
            enemyAgent.enabled = false;
            enemyCollider.enabled = false;
 
            enemyAnimator.Die();
        }
    }
}