using Photon.Pun;
using UnityEngine;

public class PlayerAnimationDriver : MonoBehaviourPun, IPunObservable
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private FirstPersonController fpc;

    [Header("Params")]
    [SerializeField] private float speedLerp = 10f;
    [SerializeField] private float groundCheckDist = 0.25f;
    [SerializeField] private LayerMask groundMask = ~0;
    
    private float netSpeed;
    private bool netGrounded;
    private bool jumpFlagRecv;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        fpc = GetComponent<FirstPersonController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        if (photonView.IsMine)
        {
            var v = rb ? rb.velocity : Vector3.zero;
            float planar = new Vector3(v.x, 0, v.z).magnitude;

            bool grounded = fpc ? fpc.IsGrounded() : DoGroundCheck();

            animator.SetFloat("Speed", planar, 0.1f, Time.deltaTime);
            animator.SetBool("Grounded", grounded);
            
            if (Input.GetButtonDown("Jump") && grounded)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
            }
        }
        else
        {
            float cur = animator.GetFloat("Speed");
            float smoothed = Mathf.Lerp(cur, netSpeed, 1f - Mathf.Exp(-speedLerp * Time.deltaTime));
            animator.SetFloat("Speed", smoothed);
            animator.SetBool("Grounded", netGrounded);

            if (jumpFlagRecv)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
                jumpFlagRecv = false;
            }
        }
    }

    private bool DoGroundCheck()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        return Physics.SphereCast(origin, 0.2f, Vector3.down, out _, groundCheckDist, groundMask, QueryTriggerInteraction.Ignore);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (animator == null) return;

        if (stream.IsWriting)
        {
            float speed = animator.GetFloat("Speed");
            bool grounded = animator.GetBool("Grounded");
            bool jump = Input.GetButtonDown("Jump");

            stream.SendNext(speed);
            stream.SendNext(grounded);
            stream.SendNext(jump);
        }
        else
        {
            netSpeed = (float)stream.ReceiveNext();
            netGrounded = (bool)stream.ReceiveNext();
            bool jump = (bool)stream.ReceiveNext();
            if (jump) jumpFlagRecv = true;
        }
    }
}
