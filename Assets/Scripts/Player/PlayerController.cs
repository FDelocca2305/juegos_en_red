using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("Camera")]
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private bool invertLook = false;

    [Header("Player movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravityMod = 2.5f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    
    private float _verticalRotStore;
    private float _activeMoveSpeed;
    private bool _isGrounded;
    private Vector2 _mouseInput;
    private Vector3 _moveDir;
    private Vector3 _movement;
    private CharacterController _characterController;
    private Camera _camera;
    
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        _camera = Camera.main;

        Transform spawnPosition = ServiceLocator.Resolve<ISpawnManager>().GetSpawnPoint();
        transform.position = spawnPosition.position;
        transform.rotation = spawnPosition.rotation;
    }

    private void Update()
    {
        CameraMovement();
        PlayerMovement();
        CheckFreeingMouse();
    }

    private void LateUpdate()
    {
        _camera.transform.position = viewPoint.position;
        _camera.transform.rotation = viewPoint.rotation;
    }

    private void PlayerMovement()
    {
        _moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        _activeMoveSpeed = IsRunning() ? runSpeed : moveSpeed;

        float yVel = _movement.y;
        _movement = ((transform.forward * _moveDir.z) + (transform.right * _moveDir.x)).normalized * _activeMoveSpeed;
        _movement.y = yVel;
        
        if (_characterController.isGrounded)
        {
            _movement.y = 0f;
        }

        JumpCheck();
        
        _movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        _characterController.Move(_movement * Time.deltaTime);
    }

    private void JumpCheck()
    {
        _isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayer);
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _movement.y = jumpForce;
        }
    }

    private static bool IsRunning()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    private void CameraMovement()
    {
        _mouseInput = GetMouseInput();
        HorizontalCameraRotation();
        VerticalCameraRotation();
    }

    private Vector2 GetMouseInput()
    {
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
    }

    private void VerticalCameraRotation()
    {
        _verticalRotStore += _mouseInput.y;
        _verticalRotStore = Mathf.Clamp(_verticalRotStore, -60f, 60f);
        viewPoint.rotation = Quaternion.Euler(invertLook ? _verticalRotStore : -_verticalRotStore, viewPoint.eulerAngles.y, viewPoint.eulerAngles.z);
    }

    private void HorizontalCameraRotation()
    {
        var rotation = transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + _mouseInput.x, rotation.eulerAngles.z);
        transform.rotation = rotation;
    }
    
    private void CheckFreeingMouse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
