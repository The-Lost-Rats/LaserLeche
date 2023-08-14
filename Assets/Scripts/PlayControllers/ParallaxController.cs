using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public static ParallaxController instance = null;

    [Header("References")]
    [SerializeField] private Transform backgroundObjectsParent;
    [SerializeField] private Transform grassObjectsParent;
    [SerializeField] private PlayerController playerController;

    [Header("Prefabs")]
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private List<GameObject> backgroundObjPrefabs;

    [Header("Game Settings")]
    [SerializeField]
    [Range(50, 300)]
    private int mapSize = 50;
    [SerializeField]
    [Range(4, 32)]
    private int mapCellSize = 4; // Defined in pixels

    private List<ParallaxObject> parallaxObjects; // Parallax objects will contain all objects to move across the screen
    private float playerPosX; // Keeps track of where the player would be if they actually moved

    public void Awake() {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    protected void Start()
    {
        // Enforce even map size cause I don't want to deal with odd numbers
        if (mapSize % 2 != 0) mapSize--;

        playerPosX = 0;

        // Create a grass object for each tile
        parallaxObjects = new List<ParallaxObject>();
        for (int i = 0; i < mapSize; i++)
        {
            GameObject grassObj = Instantiate(grassPrefab, new Vector2(GetInitXPos(i, 0), 0), Quaternion.identity, grassObjectsParent);
            GrassController grassController = grassObj.GetComponent<GrassController>();
            grassController.Init(i, mapCellSize);
            parallaxObjects.Add(grassController);
        }

        // Go through all existing background objects (these are the actual looping backgrounds) and add them to the reference list
        foreach (ParallaxObject existingBackgroundObj in backgroundObjectsParent.GetComponentsInChildren<ParallaxObject>())
        {
            parallaxObjects.Add(existingBackgroundObj);
        }
        // Go through all extra background prefabs and add them to the screen
        foreach (GameObject backgroundPrefab in backgroundObjPrefabs)
        {
            GameObject backgroundObj = Instantiate(backgroundPrefab, backgroundObjectsParent);
            ParallaxObject parallaxObj = backgroundObj.GetComponent<ParallaxObject>();
            backgroundObj.transform.position = new Vector2(GetInitXPos(parallaxObj.mapCellLocation, parallaxObj.parallaxDistance), backgroundObj.transform.position.y);
            parallaxObjects.Add(parallaxObj);
        }
    }

    public void RegisterNewParallaxObj(ParallaxObject parallaxObj)
    {
        parallaxObjects.Add(parallaxObj);
    }

    public float GetInitXPos(int cellLoc, int parallaxDist)
    {
        if (cellLoc >= (mapSize / 2)) cellLoc -= mapSize;
        float startingRawXPos = (Constants.PIXELS_PER_UNIT * cellLoc) + (Constants.PIXELS_PER_UNIT / 2) - playerPosX;
        return startingRawXPos / Mathf.Pow(2, parallaxDist);
    }

	protected void FixedUpdate()
    {
        playerPosX += playerController.PlayerVelX;
        if (playerPosX > Constants.PIXELS_PER_UNIT * (mapSize / 2)) playerPosX -= Constants.PIXELS_PER_UNIT * mapSize;
        if (playerPosX < -Constants.PIXELS_PER_UNIT * (mapSize / 2)) playerPosX += Constants.PIXELS_PER_UNIT * mapSize;

        for (int i = parallaxObjects.Count - 1; i >= 0; i--)
        {
            if (parallaxObjects[i] != null)
            {
                parallaxObjects[i].Move(playerController.PlayerVelX);
            }
            else
            {
                parallaxObjects.RemoveAt(i);
            }
        }
	}

    // Not sure if we'll ever need this, but leaving it here just in case
    private int GetCurrMapCell()
    {
        return (int)Mathf.Floor(playerPosX * Constants.PIXELS_PER_UNIT / mapCellSize);
    }

    public int GetScreenBoundsForDistance(int parallaxDist)
    {
        return Mathf.CeilToInt((Constants.PIXELS_PER_UNIT * (mapSize / 2)) / Mathf.Pow(2, parallaxDist));
    }

    public int GetMapSize()
    {
        return mapSize;
    }
}
