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
    [Range(0.01f, 2.0f)]
    private float explodeWait = 0.01f;
    [SerializeField]
    private List<Material> spriteMaterials;

    [Header("UI Variables")]
    [SerializeField]
    private List<Sprite> heartSprites;

    // Time for UI to be active after updating
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float activeHeartUIDuration;

    // Bool for managing animation states of heart UI
    private bool updateHeartUI;

    public float PlayerVelX { get; private set; }
    public float PlayerVelY { get; private set; }

    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer sr;

    const int MAX_PLAYER_HEALTH = 4;
    private int playerHealth;
    private bool invulnerable;
    private float lastInvulnerabilityBlinkTime;
    private bool invulnerabilityMaterialWhite;
    private float explodeStartTime;
    private bool playedExplosionSoundEffect;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        PlayerVelX = 0.0f;
        playerHealth = MAX_PLAYER_HEALTH;

        // Set health UI to disabled on start
        updateHeartUI = false;
        animator.SetBool("PlayerUpdateUI", updateHeartUI);

        playerHeart.SetActive(false);
    }

    protected void FixedUpdate()
    {
        if (PlayController.instance.IsGameOver())
        {
            PlayerVelX = 0;
            if (Time.fixedTime - lastInvulnerabilityBlinkTime > invulnerabilityBlinkLength && !invulnerabilityMaterialWhite)
            {
                SetMaterial(spriteMaterials[0]);
                invulnerabilityMaterialWhite = true;
            }
            if (Time.fixedTime - explodeStartTime > explodeWait)
            {
                laserController.gameObject.SetActive(false);
                animator.SetBool("PlayerExplode", true);
                if (!playedExplosionSoundEffect)
                {
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.LecheExplodes);
                    playedExplosionSoundEffect = true;
                }
            }
        }
        else if (!PlayController.instance.IsGameWon())
        {
            Move();
            CheckForJump();
        }
        PlayerVelY = rb2d.linearVelocity.y;

        DamageUpdate();

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(PlayerVelX));
        animator.SetFloat("PlayerVelY", PlayerVelY);
        animator.SetBool("PlayerInvulnerable", invulnerable);
        animator.SetBool("PlayerDead", playerHealth == 0);
        animator.SetBool("PlayerUpdateUI", updateHeartUI);
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
                rb2d.linearVelocity = new Vector2(0, jumpForce);
                if (!AudioController.Instance.OneShotAudioPlaying(SoundEffectKeys.Jump))
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Jump);
            }
        }
    }

    private bool IsOnGround() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2.8f, groundLayer);
        return hit.collider != null;
    }

    private void DamageUpdate()
    {
        // If player is alive
        if (playerHealth > 0)
        {
            //***********************
            // UI Update
            //***********************

            // TODO: think about invokes and if we can cancel any running
            // ones before starting new one

            // Only update UI if we are in idle state - if we aren't it
            // means the heart animations are playing :-)
            if (updateHeartUI && animator.GetCurrentAnimatorStateInfo(1).IsName("UIHeart_Idle"))
            {
                // If we need to update the heart UI, update it
                playerHeart.GetComponent<SpriteRenderer>().sprite = heartSprites[MAX_PLAYER_HEALTH - playerHealth];
            
                // Otherwise if we have updated the UI and it is back in Idle
                // state, stop showing the heart
            }

            // TODO: exit stuff
            else if (animator.GetBool("PlayerHeartAnimationExited"))
            {
                playerHeart.SetActive(false);
                animator.ResetTrigger("PlayerHeartAnimationExited");
            }

            //************************
            // Invulnerability logic
            //************************

            // If we are invulnerable and it is time to flash, flash
            if (invulnerable && Time.fixedTime - lastInvulnerabilityBlinkTime > invulnerabilityBlinkLength)
            {
                SetMaterial(spriteMaterials[invulnerabilityMaterialWhite ? 1 : 0]);
                invulnerabilityMaterialWhite = !invulnerabilityMaterialWhite;
                lastInvulnerabilityBlinkTime = Time.fixedTime;
            }

            // If we are not invulnerable, set player sprite to default
            else if (!invulnerable && invulnerabilityMaterialWhite)
            {
                SetMaterial(spriteMaterials[0]);
            }
        }

        // Player is not alive and heart UI is showing - lets hide it
        else if (playerHeart.activeSelf)
        {
            playerHeart.SetActive(false);

            // Also make sure the player sprite it correct
            SetMaterial(spriteMaterials[0]);
        }
    }

    private void SetMaterial(Material material)
    {
        sr.material = material;
        laserController.spriteRenderer.material = material;
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
        // Decrement health
        UpdateHealth(-1);

        // Stop firing laser
        laserController.StopFiringLaser();

        // If player is still alive, play damange sound effect
        if (playerHealth > 0)
        {
            AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.LecheHurt);
        }

        // Otherwise put up game over screen
        else
        {
            sr.sortingLayerName = "Overlay";
            sr.sortingOrder = 1;
            laserController.spriteRenderer.sortingLayerName = "Overlay";
            laserController.spriteRenderer.sortingOrder = 2;
            explodeStartTime = Time.fixedTime;
            PlayController.instance.GameOver();
        }
    }

    public void UpdateHealth(int healthValue, bool setInvulnerable = true)
    {
        // Update player health
        // TODO: one day we could make this iterate and do increment or
        // decrement one at a time - eh not sure how i feel about it
        playerHealth = Mathf.Clamp(playerHealth + healthValue, 0, MAX_PLAYER_HEALTH);

        // Optionally set player invincible for a short time
        invulnerable = setInvulnerable;
        if (setInvulnerable)
        {
            lastInvulnerabilityBlinkTime = Time.fixedTime;
            invulnerabilityMaterialWhite = false;

            // Set initial player sprite to flash white
            SetMaterial(spriteMaterials[1]);

            // Turn off invulnerability at some point
            Invoke("TurnOffInvulnerability", invulnerabilityLength);
        }

        // We need to update heart UI
        updateHeartUI = true;

        // Show player heart UI
        playerHeart.SetActive(true);

        // Set timer to start for UI (now)
        Invoke("TurnOffHealthUI", activeHeartUIDuration);
    }

    private void TurnOffInvulnerability()
    {
        invulnerable = false;
    }

    /** Disable Health UI
    */
    private void TurnOffHealthUI()
    {
        updateHeartUI = false;
    }

    /** Return true if player health is full
     * (e.g. equal to max health)
     */
    public bool IsPlayerFullHealth()
    {
        return (playerHealth == MAX_PLAYER_HEALTH);
    }
}
