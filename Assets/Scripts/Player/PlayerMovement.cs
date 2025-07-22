using Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float sprintSpeed = 12f;
        [SerializeField] private float gravityForce = -9.81f;
        [SerializeField] private float jumpForce = 920f;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 15f;
        [SerializeField] private Transform cameraTarget;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem landingParticles;

        private Rigidbody _rigidbody;
        private PlayerInputActions _playerInputActions;
        private Transform _mainCameraTransform;
        private CapsuleCollider _capsuleCollider;

        private Vector2 _currentMovementInput;
        private Vector2 _currentLookInput;
        private float _xRotation = 0f;
        private bool _isGrounded;
        private bool _isSprinting;
        private RaycastHit _groundHit;
        private bool _wasGrounded;
        private bool _justJumped;

        private void Awake()
        {
            Initialize();
            BindInputActions();
        }

        private void Start()
        {
            SetupCursor();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Disable();
        }

        private void FixedUpdate()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleGravity();
            
            _justJumped = false;
            _wasGrounded = _isGrounded;
        }

        private void LateUpdate()
        {
            HandleLook();
        }

        private void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInputActions = new PlayerInputActions();
            _mainCameraTransform = GetComponentInChildren<Camera>().transform;
            _capsuleCollider = GetComponent<CapsuleCollider>();

			if (_mainCameraTransform == null)
            {
                Debug.LogError("PlayerMovement: No camera found");
            }
        }

        private void BindInputActions()
        {
            _playerInputActions.Player.Move.performed += ctx => _currentMovementInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Move.canceled += ctx => _currentMovementInput = Vector2.zero;

            _playerInputActions.Player.Look.performed += ctx => _currentLookInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Look.canceled += ctx => _currentLookInput = Vector2.zero;
            
            _playerInputActions.Player.Sprint.performed += OnSprint;
            _playerInputActions.Player.Sprint.canceled += OnSprint;
            
            _playerInputActions.Player.Jump.performed += OnJump;
        }

        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            if (_isGrounded)
            {

                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _justJumped = true;
            }
        }

        private void HandleGroundCheck()
        {
            float sphereRadius = _capsuleCollider.radius * 0.9f;
            Vector3 sphereOrigin = groundCheck.position + (Vector3.up * sphereRadius);
            _isGrounded = Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out _groundHit, groundDistance, groundMask);

            if (!_wasGrounded && _isGrounded)
            {
                PlayLandingEffect();
            }
        }

        private void HandleMovement()
        {
            float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;
            Vector3 inputDirection = (transform.right * _currentMovementInput.x) + (transform.forward * _currentMovementInput.y);
            Vector3 targetVelocity = inputDirection * currentSpeed;

            targetVelocity.y = _rigidbody.linearVelocity.y;

            if (_isGrounded && !_justJumped)
            {
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, _groundHit.normal);
            }
            
            _rigidbody.linearVelocity = targetVelocity;
        }

        private void HandleGravity()
        {
            if (!_isGrounded)
            {
                _rigidbody.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
            }
        }

        private void HandleLook()
        {
            float mouseX = _currentLookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = _currentLookInput.y * mouseSensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            _mainCameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
        
        private void PlayLandingEffect()
        {
            if (landingParticles != null)
            {
                ParticleSystem instance = Instantiate(landingParticles, _groundHit.point, Quaternion.LookRotation(_groundHit.normal));
                Destroy(instance.gameObject, instance.main.duration);
            }
        }
    }
}