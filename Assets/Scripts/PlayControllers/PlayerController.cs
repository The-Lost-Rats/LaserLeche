using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float playerSpeedScalar;
    [Range(1, 20)]
    [SerializeField]
    private float jumpForce;

    public float PlayerVelX { get; private set; }
    public float PlayerVelY { get; private set; }

    private Rigidbody2D rb2d;
    private Animator animator;

    [SerializeField] private LayerMask groundLayer;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        PlayerVelX = 0.0f;
    }

    protected void FixedUpdate()
    {
        Move();
        CheckForJump();
        PlayerVelY = rb2d.velocity.y;

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(PlayerVelX));
        animator.SetFloat("PlayerVelY", PlayerVelY);
    }

    private bool test;
    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.L)) test = !test;
        PlayerVelX = test ? playerSpeedScalar : Input.GetAxisRaw("Horizontal") * playerSpeedScalar;
        if (PlayerVelX > 0 && transform.localScale.x < 0) {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (PlayerVelX < 0 && transform.localScale.x > 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void CheckForJump()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            if (IsOnGround()) {
                rb2d.velocity = new Vector2(0, jumpForce);
            }
        }
    }

    private bool IsOnGround() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2.8f, groundLayer);
        return hit.collider != null;
    }
}
