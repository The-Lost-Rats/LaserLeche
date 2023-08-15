using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject playerHeart;
    [SerializeField]
    private LaserController laserController;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Movement Variables")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float playerSpeedScalar;
    [Range(1, 20)]
    [SerializeField]
    private float jumpForce;

    [Header("Damage Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float invulnerabilityLength = 0.01f;
    [SerializeField]
    [Range(0.01f, 2.0f)]
    private float invulnerabilityBlinkLength = 0.01f;
    [SerializeField]
    private List<Material> spriteMaterials;
    [SerializeField]
    private List<Sprite> heartSprites;

    public float PlayerVelX { get; private set; }
    public float PlayerVelY { get; private set; }

    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer sr;

    const int MAX_PLAYER_HEALTH = 4;
    private int playerHealth;
    private bool invulnerable;
    private bool updatedHeartUI;
    private float lastInvulnerabilityBlinkTime;
    private bool invulnerabilityMaterialWhite;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        PlayerVelX = 0.0f;
        playerHealth = MAX_PLAYER_HEALTH;
    }

    protected void FixedUpdate()
    {
        Move();
        CheckForJump();
        PlayerVelY = rb2d.velocity.y;

        DamageUpdate();

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(PlayerVelX));
        animator.SetFloat("PlayerVelY", PlayerVelY);
        animator.SetBool("PlayerInvulnerable", invulnerable);
        animator.SetBool("PlayerDead", playerHealth == 0);
    }

    private void Move()
    {
        PlayerVelX = Input.GetAxisRaw("Horizontal") * playerSpeedScalar;
        if (PlayerVelX > 0 && transform.localScale.x < 0) {
            transform.localScale = new Vector3(1, 1, 1);
            playerHeart.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (PlayerVelX < 0 && transform.localScale.x > 0) {
            transform.localScale = new Vector3(-1, 1, 1);
            playerHeart.transform.localScale = new Vector3(-1, 1, 1);
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

    private void DamageUpdate()
    {
        if (playerHealth > 0)
        {
            if (invulnerable)
            {
                if (!updatedHeartUI && animator.GetCurrentAnimatorStateInfo(1).IsName("UIHeart_On"))
                {
                    playerHeart.GetComponent<SpriteRenderer>().sprite = heartSprites[MAX_PLAYER_HEALTH - playerHealth];
                    updatedHeartUI = true;
                }
                if (Time.fixedTime - lastInvulnerabilityBlinkTime > invulnerabilityBlinkLength)
                {
                    sr.material = spriteMaterials[invulnerabilityMaterialWhite ? 1 : 0];
                    laserController.spriteRenderer.material = spriteMaterials[invulnerabilityMaterialWhite ? 1 : 0];
                    invulnerabilityMaterialWhite = !invulnerabilityMaterialWhite;
                    lastInvulnerabilityBlinkTime = Time.fixedTime;
                }
            }
            else if (playerHeart.activeSelf)
            {
                if (animator.GetCurrentAnimatorStateInfo(1).IsName("UIHeart_Idle"))
                    playerHeart.SetActive(false);
                sr.material = spriteMaterials[0];
                laserController.spriteRenderer.material = spriteMaterials[0];
            }
        }
        else if (playerHeart.activeSelf)
        {
            playerHeart.SetActive(false);
            sr.material = spriteMaterials[0];
            laserController.spriteRenderer.material = spriteMaterials[0];
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("HurtfulForPlayer"))
        {
            if (!invulnerable)
            {
                TakeDamage();
            }
        }
    }

    public void TakeDamage()
    {
        playerHealth--;
        invulnerable = true;
        updatedHeartUI = false;
        laserController.ImFiringMahLazer = false;
        if (playerHealth > 0)
        {
            invulnerabilityMaterialWhite = true;
            playerHeart.SetActive(true);
            Invoke("TurnOffInvulnerability", invulnerabilityLength);
        }
        else
        {
            // TODO DIE
        }
    }

    private void TurnOffInvulnerability()
    {
        invulnerable = false;
    }
}
