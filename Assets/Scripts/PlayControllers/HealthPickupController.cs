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


    /** Initialize values and animation
     */
    protected override void OnStart()
    {
        // Set isUsed to false (we haven't picked this guy up yet!)
        isUsed = false;

        // TODO: trigger DoTween Animation
    }

    // TODO: delete this when dotween comes in
    protected void Update()
    {
        float y = Mathf.PingPong(Time.time * speed, 1) * 2 - 6;
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
        // If player - update health and die
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player picked up health pickup!");

            // Set isUsed to true
            isUsed = true;

            // Update player health
            other.gameObject.GetComponent<PlayerController>().UpdateHealth(healthToRestore);

            // Destroy self
            Die();
        }

    }
}
