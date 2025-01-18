using UnityEngine;

// DoTween
// using DG.Tweening;

public class HealthPickupController : ParallaxObject
{
    [Header("Pickup Properties")]
    [SerializeField]
    private int healthToRestore;

    [Header("Animation Properties")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float speed;


    // Private internal vars
    private bool isUsed;

    protected override void OnStart()
    {
        // Set isUsed to false (we haven't picked this guy up yet!)
        isUsed = false;

        // TODO: trigger DoTween Animation
    }

    protected void Update()
    {
        // float y = Mathf.PingPong(Time.time * speed, 1) * 6 - 3;
        // transform.position = new Vector3(0, y, 0);
    }

    // Ideally there is a better way to share the action of this pickup
    // (either hardcoded or dynamic)
    public int getHealth()
    {
        return healthToRestore;
    }

    public void die()
    {
        // Picked up and can be removed from scene
        Destroy(gameObject);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        // Only care about colliding with objects IF we haven't used the
        // pickup yet
        if (isUsed)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("PLAYER");

            // Set isUsed to true
            isUsed = true;

            // Update player health
            other.gameObject.GetComponent<PlayerController>().UpdateHealth(healthToRestore);

            // Destroy self
            die();
        }
    }
}
