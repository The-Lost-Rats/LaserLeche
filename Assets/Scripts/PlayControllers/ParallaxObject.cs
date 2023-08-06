using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    public float relativeSpeed; // TODO Add in relative speed logic
    [Range(0, 15)]
    public int parallaxDistanceFromForeground;
    public bool looping;

    protected void SetYPosition(float y)
    {
        Vector2 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}
