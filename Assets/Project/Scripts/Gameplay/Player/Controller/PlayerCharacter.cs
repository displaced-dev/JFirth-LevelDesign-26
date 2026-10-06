using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using LevelDesign.Async.Auth;
using LevelDesign.Data;

namespace LevelDesign.Systems.Player
{
    public struct CharacterState
    {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
    }

    public class PlayerCharacter : _MovementController, ICharacterController
    {
        [Header("Scene Refs")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform cameraTarget;

        [Header("Controller / Height")]
        [SerializeField] private float groundResponse = 25f;
        [SerializeField] private float gravity = -90f;

        [Header("Controller / Height")]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float cameraStandHeight = .9f;
        [SerializeField] private float cameraCrouchHeight = .7f;
        [SerializeField] private float heightResponse = 15f;

        [Header("Movement / Speeds")]
        [SerializeField] private float walkSpeed = 20f;
        [SerializeField] private float crouchSpeed = 10f;
        [SerializeField] private float sprintSpeed = 32f;
        [SerializeField] private float airSpeed = 15f;
        [SerializeField] private float airAcceleration = 70f;

        [Header("Movement / Jumping")]
        [SerializeField] private float jumpSpeed = 20f;
        [SerializeField] private float coyoteTime = .15f;
        
        [Header("Movement / Dash")]
        [SerializeField] private float dashSpeed = 40f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.75f;

        [Header("Data")]
        [SerializeField] private CharacterTweaksSO characterTweaks;

        [Header("Event")]
        [SerializeField] private StaminaEventChannelSO e_stamina;

        [Header("Debug")]
        [SerializeField] private Stance debugStance;
        [SerializeField] private bool debugSprinting;
        [SerializeField] private RigInfo currentRigInfo;

        public CharacterState state;

        private Quaternion cameraYaw = Quaternion.identity;
        private Quaternion requestedRotation = Quaternion.identity;

        private float timeSinceUngrounded;

        private Vector3 requestedMovement;

        private bool requestedJump;
        private bool requestedCrouch;
        private bool requestedSprint;
        private bool requestedDash;

        private bool isCrouched;
        private bool isSprinting;

        private float dashCooldownTimer;
        private float dashTimeElapsed;
        public bool awaitingGrounded;
        private Vector3 dashDirection;

        private Transform playerCamera;

        private PlayerStateMachine psm;

        private const float MinPlanarSqrMagnitude = 0.0001f;

        public override void _Initialize(PlayerStateMachine psm, CharacterDataSO characterdata)
        {
            if(_isInitialized){
                return;
            }

            this.psm = psm;
            motor.CharacterController = this;
            motor.enabled = true;
            state.Stance = Stance.Stand;

            _characterData = characterdata;

            _isInitialized = true;
            aInputInit(true);
            InputAuthManager.Instance.RequestInput(this);
        }

        public override void _RemoteInit() {
            motor.enabled = false;
        }

        public override void _UpdateBody(float deltaTime, Transform playerCam)
        {
            if(!_isInitialized || motor == null || cameraTarget == null){
                return;
            }

            playerCamera = playerCam;

            stanceMirror = state.Stance;

            UpdateCameraYaw();

            var cameraHeight = motor.Capsule.height * (isCrouched ? cameraCrouchHeight : cameraStandHeight);

            cameraTarget.localPosition = Vector3.Lerp(
                cameraTarget.localPosition,
                new Vector3(0f, cameraHeight, 0f),
                1f - Mathf.Exp(-heightResponse * deltaTime));
        }

        private void UpdateCameraYaw()
        {
            if(playerCamera == null){
                return;
            }

            var up = motor.CharacterUp;
            var forward = Vector3.ProjectOnPlane(playerCamera.forward, up);

            if(forward.sqrMagnitude < MinPlanarSqrMagnitude){
                forward = Vector3.ProjectOnPlane(playerCamera.up, up) * -Mathf.Sign(Vector3.Dot(playerCamera.forward, up));
            }

            if(forward.sqrMagnitude > MinPlanarSqrMagnitude){
                cameraYaw = Quaternion.LookRotation(forward.normalized, up);
            }
        }

        public void ClearInput()
        {
            requestedMovement = Vector3.zero;
            requestedJump = false;
            requestedCrouch = false;
            requestedSprint = false;
            requestedDash = false;
        }

        public override void _UpdateInput()
        {
            if(!_isInitialized || !_inputAuthorized) { 
                ClearInput(); 
                return;
            }

            requestedRotation = cameraYaw;

            var move = _input.Move.ReadValue<Vector2>();
            requestedMovement = cameraYaw * Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);

            requestedJump |= _input.Jump.WasPressedThisFrame();
            requestedCrouch = _input.Crouch.IsPressed();
            requestedSprint = _input.Sprint.IsPressed();
            requestedDash |=  _input.Dash.WasPressedThisFrame();
        }

        public override Transform _GetCameraTarget() => cameraTarget;
        
        void FindComponent<T>(ref T field) where T : Component
        {
            if(field != null) { return; }
            field = GetComponent<T>() ?? GetComponentInChildren<T>();
        }

        public override void _Teleport(Vector3 position)
        {
            if(motor == null){
                return;
            }

            motor.BaseVelocity = Vector3.zero;
            motor.SetPosition(position);
        }

        public override void _SetRotation(Quaternion rotation)
        {
            if(motor == null){
                return;
            }

            cameraYaw = rotation;
            requestedRotation = rotation;
            motor.SetRotation(rotation);
        }

        public void ResetMovementStates()
        {
            if(!_isInitialized || motor == null){
                return;
            }

            ClearInput();
            state = default;
            state.Stance = Stance.Stand;
            motor.BaseVelocity = Vector3.zero;
            isSprinting = false;
            SetCrouched(false);
        }

        public CharacterState GetState() => state;
        public Stance GetStance() => state.Stance;
        public bool GetIsSprinting() => isSprinting;

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var forward = Vector3.ProjectOnPlane(requestedRotation * Vector3.forward, motor.CharacterUp);
            if(forward != Vector3.zero){
                currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            SetCrouched(requestedCrouch);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var grounded = motor.GroundingStatus.IsStableOnGround;
            
            // Perform a dash and ignore all else until completed
            if(dashCooldownTimer > 0f){
                dashCooldownTimer -= deltaTime;
            }

            if(requestedDash && CanDash() && SpendStamina(characterTweaks.dashCost)){
                StartDash();
            }

            requestedDash = false;

            if(state.Stance == Stance.Dash)
            {
                UpdateDash(ref currentVelocity, deltaTime);
                return;
            }

            if(grounded)
            {
                awaitingGrounded = false;
                state.Stance = isCrouched ? Stance.Crouch : Stance.Stand;
                timeSinceUngrounded = 0f;

                isSprinting = !isCrouched && requestedSprint && requestedMovement.sqrMagnitude > MinPlanarSqrMagnitude;

                var groundedMovement = motor.GetDirectionTangentToSurface(requestedMovement, motor.GroundingStatus.GroundNormal) * requestedMovement.magnitude;
                var speed = isCrouched ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

                currentVelocity = Vector3.Lerp(currentVelocity, groundedMovement * speed, 1f - Mathf.Exp(-groundResponse * deltaTime));
            }
            else
            {
                if(state.Stance != Stance.Dash) {
                    state.Stance = Stance.Air;
                }
                
                isSprinting = false;
                timeSinceUngrounded += Time.deltaTime;

                if(requestedMovement.sqrMagnitude > 0f)
                {
                    var planarMovement = Vector3.ProjectOnPlane(requestedMovement, motor.CharacterUp).normalized * requestedMovement.magnitude;
                    var planarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                    var movementForce = planarMovement * airAcceleration * deltaTime;

                    if(planarVelocity.magnitude < airSpeed)
                    {
                        var target = Vector3.ClampMagnitude(planarVelocity + movementForce, airSpeed);
                        movementForce = target - planarVelocity;
                    }
                    else if(Vector3.Dot(planarVelocity, movementForce) > 0f)
                    {
                        movementForce = Vector3.ProjectOnPlane(movementForce, planarVelocity.normalized);
                    }

                    currentVelocity += movementForce;
                }

                currentVelocity += motor.CharacterUp * gravity * deltaTime;
            }
        
            if(requestedJump)
            {
                requestedJump = false;

                if(grounded || timeSinceUngrounded <= coyoteTime && !awaitingGrounded)
                {
                    motor.ForceUnground();

                    var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                    currentVelocity += motor.CharacterUp * (Mathf.Max(jumpSpeed, verticalSpeed) - verticalSpeed);

                    awaitingGrounded = true;
                }
            }
        }

        private bool CanDash()
        {
            if(dashCooldownTimer > 0f || state.Stance == Stance.Dash || !state.Grounded){
                return false;
            }
            return true;
        }

        private void StartDash()
        {
            state.Stance = Stance.Dash;
            dashTimeElapsed = 0f;
            dashCooldownTimer = dashCooldown;
            awaitingGrounded = true;

            var planarInput = Vector3.ProjectOnPlane(requestedMovement, motor.CharacterUp);
            dashDirection = planarInput.sqrMagnitude > MinPlanarSqrMagnitude ? planarInput.normalized : Vector3.ProjectOnPlane(motor.CharacterForward, motor.CharacterUp).normalized;

            requestedJump = false;
        }

        private void UpdateDash(ref Vector3 currentVelocity, float deltaTime)
        {
            dashTimeElapsed += deltaTime;
            currentVelocity = dashDirection * dashSpeed;

            if(dashTimeElapsed >= dashDuration)
            {
                if(state.Grounded) {
                    state.Stance = Stance.Stand;
                }
                else {
                    state.Stance = Stance.Air;
                }
                currentVelocity = dashDirection * dashSpeed * .5f;
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            state.Grounded = motor.GroundingStatus.IsStableOnGround;
            state.Velocity = motor.Velocity;
            debugStance = state.Stance;
            debugSprinting = isSprinting;
        }

        public void PostGroundingUpdate(float deltaTime) { }
        public bool IsColliderValidForCollisions(Collider coll) => true;
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }


        public override RigInfo _GetCurrentRigInfo()
        {
            if(currentRigInfo == null) {
                FindComponent(ref currentRigInfo);
            }

            return currentRigInfo;
        }

        private void SetCrouched(bool crouch)
        {
            if(crouch == isCrouched){
                return;
            }
            isCrouched = crouch;

            var height = crouch ? crouchHeight : standHeight;
            motor.SetCapsuleDimensions(motor.Capsule.radius, height, height * .5f);
        }

        private bool SpendStamina(float amount)
        {
            return e_stamina == null || e_stamina.SpendStamina(amount);
        }
    }
}