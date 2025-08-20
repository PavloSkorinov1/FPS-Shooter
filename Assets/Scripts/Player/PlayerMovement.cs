using Input;
using UnityEngine;

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
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 920f;
        [SerializeField] private float jumpCooldown = 0.25f;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 15f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;
        
        [Header("Effects")]
        [SerializeField] private GameObject landingParticles;
        [SerializeField] private float minFallVelocity = -5f;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip[] walkSfx;
        [SerializeField] private AudioClip[] runSfx;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;
        

        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        private PlayerInputActions _playerInputActions;
        private Transform _mainCameraTransform;

        private Vector2 _currentMovementInput;
        private Vector2 _currentLookInput;
        private bool _isSprinting;
        private bool _wasSprinting;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _jumpRequested;
        private bool _justJumped;
        private float _jumpCooldownTimer;
        private float _xRotation = 0f;
        private float _stepTimer;
        
        private Vector3 _groundNormal;

        private void Awake()
        {
            Initialize();
            SetupCamera();
            BindInputActions();
        }

        private void Start()
        {
            SetupCursor();
        }

        private void OnEnable() => _playerInputActions.Player.Enable();
        private void OnDisable() => _playerInputActions.Player.Disable();
        
        private void Update()
        {
            HandleJumpCooldown();
            HandleFootstepSounds();
        }

        private void LateUpdate()
        {
            HandleLook();
        }

        private void FixedUpdate()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleGravity();
            HandleJump();
            HandleLandingEffects();
        }
        
        private void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _playerInputActions = new PlayerInputActions();
            _mainCameraTransform = GetComponentInChildren<Camera>().transform;
        }
        
        private void SetupCamera()
        {
            if (_mainCameraTransform == null)
            {
                Debug.LogError("PlayerMovement: No child camera found");
            }
        }

        private void BindInputActions()
        {
            _playerInputActions.Player.Move.performed += ctx => _currentMovementInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Move.canceled += ctx => _currentMovementInput = Vector2.zero;

            _playerInputActions.Player.Look.performed += ctx => _currentLookInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Look.canceled += ctx => _currentLookInput = Vector2.zero;
            
            _playerInputActions.Player.Sprint.performed += ctx => _isSprinting = true;
            _playerInputActions.Player.Sprint.canceled += ctx => _isSprinting = false;

            _playerInputActions.Player.Jump.performed += ctx => RequestJump();
        }

        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void HandleJumpCooldown()
        {
            if (_jumpCooldownTimer > 0)
            {
                _jumpCooldownTimer -= Time.deltaTime;
            }
        }
        private void RequestJump()
        {
            if (_isGrounded && _jumpCooldownTimer <= 0)
            {
                _jumpRequested = true;
                _justJumped = true;
                
                if (jumpSfx != null && audioSource != null)
                {
                    audioSource.PlayOneShot(jumpSfx);
                }
            }
        }
        
        private void HandleGroundCheck()
        {
            _wasGrounded = _isGrounded;
            if (Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out RaycastHit hit, _capsuleCollider.height / 2f - groundCheckRadius + 0.1f, groundMask))
            {
                _isGrounded = true;
                _groundNormal = hit.normal;
            }
            else
            {
                _isGrounded = false;
                _groundNormal = Vector3.up;
            }
        }

        private void HandleMovement()
        {
            if (_justJumped)
            {
                _justJumped = false;
                return;
            }

            float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;
            Vector3 targetVelocity = new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y);
            targetVelocity = _mainCameraTransform.TransformDirection(targetVelocity) * currentSpeed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(targetVelocity, _groundNormal);
            
            Vector3 newVelocity = new Vector3(projectedVelocity.x, _rigidbody.linearVelocity.y, projectedVelocity.z);
            _rigidbody.linearVelocity = newVelocity;
        }

        private void HandleGravity()
        {
            if (_isGrounded)
            {
                if (_rigidbody.linearVelocity.y > 0)
                {
                    _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
                }
            }
            else
            {
                _rigidbody.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
            }
        }
        
        private void HandleJump()
        {
            if (_jumpRequested)
            {
                _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _jumpCooldownTimer = jumpCooldown;
                _jumpRequested = false;
            }
        }

        private void HandleLandingEffects()
        {
            if (!_wasGrounded && _isGrounded && _rigidbody.linearVelocity.y <= minFallVelocity)
            {
                if (landingParticles != null)
                {
                    GameObject particles = Instantiate(landingParticles, groundCheck.position, landingParticles.transform.rotation);
                    Destroy(particles, 1.5f);
                }
            }
        }
        
        private void HandleFootstepSounds()
        {
            if (!_isGrounded || audioSource == null) return;
            
            if (_currentMovementInput == Vector2.zero)
            {
                _stepTimer = 0f;
                return;
            }

            if (_isSprinting != _wasSprinting)
            {
                _stepTimer = 0f;
            }

            _stepTimer -= Time.deltaTime;

            if (_stepTimer <= 0f)
            {
                AudioClip[] currentClips = _isSprinting ? runSfx : walkSfx;
                float currentInterval = _isSprinting ? runStepInterval : walkStepInterval;
                
                if (currentClips.Length > 0)
                {
                    int index = Random.Range(0, currentClips.Length);
                    audioSource.PlayOneShot(currentClips[index]);
                }
                
                _stepTimer = currentInterval;
            }
            
            _wasSprinting = _isSprinting;
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
    }
}