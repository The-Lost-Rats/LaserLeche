using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    public enum ParallaxDistance
    {
        ZERO = 0,
        ONE = 1,
        TWO = 2,
        THREE = 3,
        SEVEN = 7,
        FIFTEEN = 15,
    }

    public ParallaxDistance parallaxDistanceEnum;
    public int parallaxDistance
    {
        get { return (int)parallaxDistanceEnum; }
    }
    public int mapCellLocation;
    public bool looping;

    [HideInInspector]
    public float relativeSpeed; // TODO Add in relative speed logic

    private int objBounds;
    private bool realLoop; // If true, loop by subtracting/adding the loop amount rather than setting it

    protected new SpriteRenderer renderer;

    protected bool initialized { get; private set; } = false;

    protected void Start()
    {
        renderer = GetComponent<SpriteRenderer>();

        if (looping)
        {
            objBounds = Constants.SCREEN_BOUNDS * 2;
            realLoop = true;
        }
        else
        {
            int parallaxBounds = ParallaxController.instance.GetScreenBoundsForDistance(
                parallaxDistance
            );
            int screenBounds = Constants.SCREEN_BOUNDS + (int)Mathf.Ceil(renderer.size.x / 2);
            realLoop = parallaxBounds > screenBounds;
            objBounds = realLoop ? parallaxBounds : screenBounds;
        }

        OnStart();

        initialized = true;
    }

    protected virtual void OnStart() { }

    protected void SetYPosition(float y)
    {
        Vector2 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    public void Move(float xDiff)
    {
        if (!initialized)
            return;

        Vector2 pos = transform.position;
        pos.x -= (xDiff - relativeSpeed) / Mathf.Pow(2, parallaxDistance);
        // Handle bounds
        if (pos.x > objBounds)
        {
            if (realLoop)
                pos.x -= 2 * objBounds;
            else
                pos.x = -objBounds;
        }
        else if (pos.x < -objBounds)
        {
            if (realLoop)
                pos.x += 2 * objBounds;
            else
                pos.x = objBounds;
        }
        transform.position = pos;
    }

    protected int GetCurrMapCell()
    {
        return ParallaxController.instance.GetMapCellFromPos(transform.position.x);
    }
}
