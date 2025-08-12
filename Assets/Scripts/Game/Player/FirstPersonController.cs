using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviourPun
{
    [Header("Camera Setup")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    private Rigidbody _rb;
    private float _xRotation = 0f;
    private bool _grounded = true;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (!photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
            enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        RotateView();

        if (Input.GetKeyDown(KeyCode.Space) && _grounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _grounded = false;
        }
    }

    public bool IsGrounded()
    {
        return _grounded;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = (transform.forward * v + transform.right * h) * moveSpeed;
        Vector3 velocity = new Vector3(move.x, _rb.velocity.y, move.z);
        _rb.velocity = velocity;
    }

    private void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -maxLookAngle, maxLookAngle);

        cameraPivot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 &&
            Vector3.Dot(collision.contacts[0].normal, Vector3.up) > 0.5f)
        {
            _grounded = true;
        }
    }
}
