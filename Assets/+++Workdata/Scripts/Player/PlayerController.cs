using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Inspector
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float acceleration = 10;
    [SerializeField] private float deceleration = 12f;

    [Header("Gravity Settings")] 
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundStickForce = 2f;

    [Header("Height Settings")] 
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float cameraStandingHeight = 1.6f;
    [SerializeField] private float cameraHeightSmoothing = 10f;

    [Header("Ground Settings")] 
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Look Settings")] 
    [SerializeField] private float lookHorizontalSensitivity = 0.1f;
    [SerializeField] private float lookVerticalSensitivity = 0.1f;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Vector2 minMaxRotation = new Vector2(-85, 85);
    [SerializeField] private bool invertY;

    #endregion

    #region private Variables

    private PlayerControlInput _playerControlInput;
    private CharacterController _characterController;
    private PlayerState _playerState;

    private float _groundCheckRadius = 0.9f;
    
    private float _currentSpeed;
    private float _verticalVelocity;
    private float _xRotation;
    private float _cameraBaseHeight;
    
    private bool _isGrounded;
    
    private Vector3 _moveVelocity;
    private Vector2 _moveInput;
    private Vector2 _lookInput;

    #endregion

    #region Unity Event Methods
    private void Awake()
    {
        //TODO: Delete in some time
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        _playerControlInput = GetComponent<PlayerControlInput>();
        _characterController = GetComponent<CharacterController>();
        _playerState = GetComponent<PlayerState>();
        
        ResetHeight();
        ResetMoveSpeed();

        ResetToBaseHeight();
    }

    private void OnEnable()
    {
        _playerState.StateChanged += OnStateChanged;
    }

    private void Update()
    {
        ReadInputs();
        
        HandleLook();

        CheckGround();
        HandleMovement();
        HandleGravity();
        
        ApplyMovement();
        
        HandleCameraHeight();
    }

    private void OnDisable()
    {
        _playerState.StateChanged -= OnStateChanged;
    }

    #endregion
    
    #region Movement Methods

    private void ReadInputs()
    {
        _moveInput = _playerControlInput.MoveInput;
        _lookInput = _playerControlInput.LookInput;
    }

    private void HandleLook()
    {
        float mouseX = _lookInput.x * lookHorizontalSensitivity;
        float mouseY = (invertY ? -1f : 1f) * _lookInput.y * lookVerticalSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minMaxRotation.x, minMaxRotation.y);

        cameraPivot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        Vector3 desiredVelocity = inputDirection * _currentSpeed;
        float rate = inputDirection.sqrMagnitude > 0.01f ? acceleration : deceleration;

        _moveVelocity = Vector3.Lerp(_moveVelocity, desiredVelocity, rate * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        Vector3 finalMovement = _moveVelocity;
        finalMovement.y = _verticalVelocity;
        _characterController.Move(finalMovement * Time.deltaTime);
    }
    
    #endregion

    #region Gravity Methods

    private void CheckGround()
    {
        float controllerRadius = _characterController.radius;
        float checkRadius = controllerRadius * _groundCheckRadius;
        Vector3 origin = transform.position + Vector3.up * controllerRadius;
        float castDistance = controllerRadius - checkRadius + groundCheckDistance;

        bool hitGround = Physics.SphereCast(origin, checkRadius, Vector3.down, out RaycastHit groundHit,
            castDistance, groundMask, QueryTriggerInteraction.Ignore);

        _isGrounded = hitGround && _verticalVelocity <= 0f;
    }
    
    private void HandleGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -groundStickForce;
        }

        _verticalVelocity += gravity * Time.deltaTime;
    }    

    #endregion

    #region Utility Methods
    
    public void SetMoveSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    public void ResetMoveSpeed()
    {
        SetMoveSpeed(walkingSpeed);
    }

    public void SetHeight(float height, float cameraHeight)
    {
        _characterController.height = height;
        _characterController.center = new Vector3(0f, height / 2f, 0f);

        _cameraBaseHeight = cameraHeight;
    }

    public void ResetHeight()
    {
        SetHeight(standingHeight, cameraStandingHeight);
    }

    public bool CanStandUp(LayerMask ceilingMask)
    {
        float distance = standingHeight - _characterController.height;
        if (distance <= 0f) return true;

        float radius = _characterController.radius * _groundCheckRadius;
        Vector3 origin = transform.position + Vector3.up * (_characterController.height - _characterController.radius);

        return !Physics.SphereCast(origin, radius, Vector3.up, out _, distance, ceilingMask, QueryTriggerInteraction.Ignore);
    }

    private void HandleCameraHeight()
    {
        Vector3 pivotPosition = cameraPivot.localPosition;
        pivotPosition.y = Mathf.Lerp(pivotPosition.y, _cameraBaseHeight, cameraHeightSmoothing * Time.deltaTime);
        cameraPivot.localPosition = pivotPosition;
    }
    
    private void ResetToBaseHeight()
    {
        Vector3 pivotPosition = cameraPivot.localPosition;
        pivotPosition.y = _cameraBaseHeight;
        cameraPivot.localPosition = pivotPosition;
        ResetHeight();
    }

    private void OnStateChanged(MovementState oldState, MovementState newState)
    {
        if (newState == MovementState.Walking)
        {
            ResetMoveSpeed();
        }
    }

    #endregion
    
    
}