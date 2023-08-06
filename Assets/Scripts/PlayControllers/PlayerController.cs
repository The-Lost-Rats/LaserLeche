using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 0.1f)]
    private float playerSpeedScalar;
    [Range(1, 20)]
    [SerializeField]
    private float jumpForce;

    public float PlayerSpeed { get; private set; }

    private Rigidbody2D rb2d;
    private Animator animator;
    private new SpriteRenderer renderer;

    [SerializeField] private LayerMask groundLayer;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        PlayerSpeed = 0.0f;
    }

    protected void Update()
    {
        Move();
        CheckForJump();

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(PlayerSpeed));
        animator.SetFloat("PlayerVelY", rb2d.velocity.y);
    }

    private void Move()
    {
        PlayerSpeed = Input.GetAxisRaw("Horizontal") * playerSpeedScalar;
        if (PlayerSpeed > 0 && renderer.flipX) {
            renderer.flipX = false;
        }
        else if (PlayerSpeed < 0 && !renderer.flipX) {
            renderer.flipX = true;
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
