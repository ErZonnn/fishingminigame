using System;
using UnityEngine;

namespace FishingMinigame.Core.Character
{
    public enum MovementState
    {
        Default,
        Idle,
        Walk,
        Sprint
    };

    [RequireComponent(typeof(CharacterController))]
    public class MovementSystem : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _sprintSpeed = 5f;
        [SerializeField] private float _moveInertia = 10f;

        [Space]
        [SerializeField] private float _gravity = 15f;
        [SerializeField] private Vector3 _groundCheckerSize;
        [SerializeField] private Vector3 _groundCheckerPosCorrention;
        [SerializeField] private LayerMask _excludeGroundLayers;

        [Header("Debug")]
        [SerializeField] private MovementState _lastMovementState;
        [SerializeField] private MovementState _currentMovementState;

        #region PRIVATE VARIABLES

        private CharacterController _controller;

        private Vector2 _controlInput;
        private Vector3 _moveVel;
        private Vector3 _gravityVal;

        private bool _sprintInput;

        #endregion

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            UpdateState(ref _lastMovementState, ref _currentMovementState, MovementState.Idle);
        }

        private void OnDisable()
        {
            UpdateState(ref _lastMovementState, ref _currentMovementState, MovementState.Default);
        }

        private void UpdateState(ref MovementState lastMovementState, ref MovementState currentMovementState, MovementState newMovementState)
        {
            if (currentMovementState == newMovementState)
                return;

            lastMovementState = currentMovementState;
            currentMovementState = newMovementState;
        }

        private void Update()
        {
            HandleInput();
            ControlMovementState();

            switch (_currentMovementState)
            {
                case MovementState.Default: return;
                case MovementState.Idle: HandleIdleState();  break;
                case MovementState.Walk:
                case MovementState.Sprint: HandleWalkAndSprintState(); break;
            }
        }

        private void ControlMovementState()
        {
            if(_controlInput.magnitude >= 0.1f && _sprintInput)
            {
                UpdateState(ref _lastMovementState, ref _currentMovementState, MovementState.Sprint);
                return;
            }
            else if(_controlInput.magnitude >= 0.1f)
            {
                UpdateState(ref _lastMovementState, ref _currentMovementState, MovementState.Walk);
                return;
            }
            else if(_controlInput.magnitude < 0.1f)
            {
                UpdateState(ref _lastMovementState, ref _currentMovementState, MovementState.Idle);
                return;
            }
        }

        private void HandleIdleState()
        {
            if (IsGrounded())
                _moveVel = Vector3.Lerp(_moveVel, Vector3.zero, _moveInertia * Time.deltaTime);

            _controller.Move(_moveVel * Time.deltaTime);
        }

        private void HandleWalkAndSprintState()
        {
            Vector3 moveDir = transform.forward * _controlInput.y + transform.right * _controlInput.x;
            moveDir.Normalize();

            float speed = _sprintInput ? _sprintSpeed : _moveSpeed;

            if (IsGrounded())
                _moveVel = Vector3.Lerp(_moveVel, moveDir * speed, _moveInertia * Time.deltaTime);

            _controller.Move(_moveVel * Time.deltaTime);
        }

        private bool IsGrounded()
        {
            bool isGrounded = Physics.CheckBox(transform.position + _groundCheckerPosCorrention, _groundCheckerSize / 2f, transform.rotation, ~_excludeGroundLayers);
            return isGrounded;
        }

        private void FixedUpdate()
        {
            HandleGravity();
        }

        private void HandleGravity()
        {
            if (IsGrounded())
                _gravityVal = Vector3.down * 2f;
            else
                _gravityVal += Vector3.down * _gravity;

            _controller.Move(_gravityVal * Time.fixedDeltaTime);
        }

        private void HandleInput()
        {
            _controlInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            _sprintInput = Input.GetKey(KeyCode.LeftShift);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + _groundCheckerPosCorrention, _groundCheckerSize);
        }
#endif
    }
}
