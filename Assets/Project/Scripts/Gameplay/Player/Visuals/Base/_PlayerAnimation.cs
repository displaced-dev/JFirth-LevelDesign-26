using UnityEngine;
using LevelDesign.Async.Auth;
using LevelDesign.Data;
// Summary
// Base class responsible for managing the base movement states such as walk, run, jump, crouch.
// Will need to be extended when elements such as abilities modify the animations.

namespace LevelDesign.Systems.Player
{
    public abstract class _PlayerAnimation : _InputAuth
    {
        [Header("Scene Refs")]
        [SerializeField] protected RigInfo rigInfo;
        public Animator characterAnimator;
        [Space]
        public _MovementController m_Controller;

        [Header("Event")]
        [SerializeField] private KillPlayerEventChannelSO e_playerkilled;

        [Header("Debug")]
        public float deltaTime;
        public float animationReactionModifier = 1f;

        private float currentXVal;
        private float currentYVal;

        private bool isGrounded = true;
        private bool isCrouching = false;
        private bool isMoving = false;
        private bool isDashing = false;

        private Stance characterStance;

        protected virtual void Awake()
        {
            _FindComponent(ref rigInfo);
            e_playerkilled.OnKillRequested += KillPlayerAnimation;
        }

        protected virtual void OnEnable()
        {
            aInputInit(true);
            InputAuthManager.Instance.RequestInput(this);
        }

        void _FindComponent<T>(ref T field) where T : Component
        {
            if(field != null) { return; }
            field = GetComponent<T>() ?? GetComponentInChildren<T>();
        }
        
        void KillPlayerAnimation() {
            characterAnimator.SetBool("Death", true);
        }
        
        void RevivePlayerAnimation() {
            characterAnimator.SetBool("Death", false);
        }

        protected virtual void Update()
        {
            deltaTime = Time.deltaTime;

            if(characterAnimator == null) { return; }

            bool sprinting = false;
            Vector2 requestedMovement = new Vector2(0,0);
            if(_inputAuthorized) { 
                requestedMovement = _input.Move.ReadValue<Vector2>();
                sprinting = _input.Sprint.IsPressed();
            }

            float maxMovementValue = sprinting ? 1f : 0.5f;
            float targetX = Mathf.Clamp(requestedMovement.x, -maxMovementValue, maxMovementValue);
            float targetY = Mathf.Clamp(requestedMovement.y, -maxMovementValue, maxMovementValue);

            currentXVal = Mathf.Lerp(currentXVal, targetX, deltaTime * animationReactionModifier);
            currentYVal = Mathf.Lerp(currentYVal, targetY, deltaTime * animationReactionModifier);

            isMoving = Mathf.Abs(currentXVal) >= .01f || Mathf.Abs(currentYVal) >= .01f;

            _UpdateAnimationStateFromStance();
            _UpdateAnimatorValues();
        }

        protected virtual void _UpdateAnimationStateFromStance()
        {
            if (m_Controller != null)
            {
                characterStance = m_Controller.stanceMirror;
            }

            switch (characterStance)
            {
                case Stance.Air:
                    isGrounded = false; isCrouching = false; isDashing = false; break;
                case Stance.Stand:
                    isGrounded = true; isCrouching = false; isDashing = false; break;
                case Stance.Crouch:
                    isGrounded = true; isCrouching = true; isDashing = false; break;
                case Stance.Dash:
                    characterAnimator.SetTrigger("Dash"); break;
            }
        }

        public void _SetWeapon(int weaponValue) {
            characterAnimator.SetInteger("Weapon", weaponValue);
        }

        protected virtual void _UpdateAnimatorValues()
        {
            if(characterAnimator == null) { return; }

            characterAnimator.SetFloat("xVal", currentXVal);
            characterAnimator.SetFloat("yVal", currentYVal);
            characterAnimator.SetBool("Grounded", isGrounded);
            characterAnimator.SetBool("Crouch", isCrouching);
            characterAnimator.SetBool("Moving", isMoving);
        }

        protected virtual void OnDestroy()
        {
            e_playerkilled.OnKillRequested -= KillPlayerAnimation;

            if(InputAuthManager.Instance != null)
            {
                InputAuthManager.Instance.RelinquishRequest(this);
            }
        }
    }
}