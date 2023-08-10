using UnityEngine;

public class GrassController : ParallaxObject
{
    private float grassSize;

    public void Init(int mapCellNum, int mapCellSize)
    {
        parallaxDistanceEnum = ParallaxDistance.ZERO;
        mapCellLocation = mapCellNum;
        grassSize = mapCellSize / (float)Constants.PIXELS_PER_UNIT;
    }

    protected override void OnStart()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        renderer.size = new Vector2(grassSize, renderer.size.y);
        collider.size = new Vector2(grassSize, collider.size.y);
        gameObject.name = "Grass [" + mapCellLocation + "]";
    }
}
