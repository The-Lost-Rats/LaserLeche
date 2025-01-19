using UnityEngine;

// DoTween
// using DG.Tweening;

// Ideally there is a better way to share the action of this pickup
// (either hardcoded or dynamic)

/** Health pickup controller script
 *
 * Manages pickup lifecycle and collisions
 */
public class HealthPickupController : ParallaxObject
{
    /** Pickup related properties
     */
    [Header("Pickup Properties")]

    // Amount of health to give to player
    [SerializeField]
    private int healthToRestore;

    /** Properties related to animation of pickup
     */
    [Header("Animation Properties")]

    // Speed of animation
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float speed;


    /** Private internal vars
     */

    // If pickup has been picked up
    private bool isUsed;

    // Starting y position of pickup
    private float startingYPos;


    /** Initialize values and animation
     */
    protected override void OnStart()
    {
        // Set isUsed to false (we haven't picked this guy up yet!)
        isUsed = false;

        // Set starting y position
        startingYPos = transform.position.y;

        // TODO: trigger DoTween Animation
    }

    // TODO: delete this when dotween comes in
    protected void Update()
    {
        // Get value between 0 and 1 over time (follows triangle wave)
        // Multiply value by 2 so range is 0 to 2
        // Add starting y poition so it is offset by start position
        // E.g. the pickup will ping pong from starting y position to 2 +
        // starting y position
        float y = Mathf.PingPong(Time.time * speed, 1) * 2 + startingYPos;
        transform.position = new Vector2(transform.position.x, y);
    }

    /** Destroy self and any other clean up
     */
    public void Die()
    {
        // I have been picked up and can be removed from scene
        Destroy(gameObject);
    }

    /** Collision has occurred
     */
    protected void OnTriggerEnter2D(Collider2D other)
    {
        // Only care about colliding with objects IF we haven't used the
        // pickup yet
        if (!isUsed)
        {
            CheckCollision(other);
        }
    }

    /** Check collision
     *
     * If against player, use pick up
     */
    private void CheckCollision(Collider2D other)
    {
        // If player touched pickup
        // AND player is not at full health
        // Update player health and destroy self
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched health pickup!");

            // Get player controller
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

            // Check if player is not at full health
            if (!playerController.IsPlayerFullHealth())
            {
                Debug.Log("Player picked up health pickup!");

                // Set isUsed to true
                isUsed = true;

                // Update player health
                playerController.UpdateHealth(healthToRestore, false);

                // Destroy self
                Die();
            }
        }

    }
}
