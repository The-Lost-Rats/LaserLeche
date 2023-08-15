using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : MonoBehaviour
{
    private enum PlayState
    {
        START = 0,
        WAVE_TEXT = 1,
        BACKGROUND_UFO_SPAWN = 2,
        BEGIN_TEXT = 3,
        GAMEPLAY = 4,
        WAVE_COMPLETE = 5
    }

    [Header("Game Data")]
    [SerializeField] private List<LevelData> levelDatas;

    [Header("References")]
    [SerializeField] private Transform uiParent;
    [SerializeField] private Transform backgroundObjectsParent;
    [SerializeField] private Transform sceneObjectsParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject basicUIText;
    [SerializeField] private GameObject backgroundUFOObject;
    [SerializeField] private GameObject ufoPrefab;

    [Header("State Variables")]
    [SerializeField] [Range(0.1f, 1.0f)] private float startLength = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float backgroundUFOSpawnTime = 0.0f;
    [SerializeField] private Sprite beginTextSprite;
    [SerializeField] private Sprite waveCompleteTextSprite;

    private PlayState state;

    private int currLevel;
    private float stateTimer;
    private int numUFOsDestroyed;

    private Animator currAnimator;
    private BackgroundUFOController[] backgroundUFOs;
    private UFOController[] spawnedUFOs;

    protected void Start()
    {
        state = PlayState.START;
        stateTimer = Time.fixedTime;
        currLevel = 0;

        // Invoke("TestUFO", 0.5f);
    }

    private void TestUFO()
    {
        GameObject ufo = Instantiate(ufoPrefab, sceneObjectsParent);
        UFOController ufoController = ufo.GetComponent<UFOController>();
        ufoController.Init(0);
        ufo.transform.position = new Vector2(ParallaxController.instance.GetInitXPos(ufoController.mapCellLocation, ufoController.parallaxDistance), ufo.transform.position.y);
        ParallaxController.instance.RegisterNewParallaxObj(ufoController);
    }

    protected void FixedUpdate()
    {
        switch (state)
        {
            case PlayState.START:
                if (Time.fixedTime - stateTimer >= startLength)
                {
                    GameObject waveText = Instantiate(basicUIText, uiParent);
                    SpriteRenderer sr = waveText.GetComponent<SpriteRenderer>();
                    sr.sprite = levelDatas[currLevel].waveNumImage;
                    currAnimator = waveText.GetComponent<Animator>();
                    state = PlayState.WAVE_TEXT;
                }
                break;
            case PlayState.WAVE_TEXT:
                if (currAnimator && currAnimator.GetCurrentAnimatorStateInfo(0).IsName("TextGone"))
                {
                    Destroy(currAnimator.gameObject);

                    int numUFOs = levelDatas[currLevel].ufoList.Count;
                    backgroundUFOs = new BackgroundUFOController[numUFOs];
                    spawnedUFOs = new UFOController[numUFOs];
                    numUFOsDestroyed = 0;
                    Invoke("SpawnBackgroundUFO", backgroundUFOSpawnTime);
                    state = PlayState.BACKGROUND_UFO_SPAWN;
                }
                break;
            case PlayState.BACKGROUND_UFO_SPAWN:
                if (backgroundUFOs[^1] != null && backgroundUFOs[^1].IsIdling())
                {
                    GameObject beginText = Instantiate(basicUIText, uiParent);
                    SpriteRenderer sr = beginText.GetComponent<SpriteRenderer>();
                    sr.sprite = beginTextSprite;
                    currAnimator = beginText.GetComponent<Animator>();
                    state = PlayState.BEGIN_TEXT;
                }
                break;
            case PlayState.BEGIN_TEXT:
                if (currAnimator && currAnimator.GetCurrentAnimatorStateInfo(0).IsName("TextGone"))
                {
                    Destroy(currAnimator.gameObject);
                    currAnimator = null;
                    DescendNextBackgroundUFO(); // Descend the first UFO
                    state = PlayState.GAMEPLAY;
                }
                break;
            case PlayState.GAMEPLAY:
                {
                    int numUFOs = levelDatas[currLevel].ufoList.Count;
                    for (int i = 0; i < numUFOs; i++)
                    {
                        if (backgroundUFOs[i] != null && backgroundUFOs[i].IsDescended())
                        {
                            SpawnUFO(i);
                        }
                    }
                    for (int i = 0; i < numUFOs; i++)
                    {
                        if (spawnedUFOs[i] != null && spawnedUFOs[i].IsDead())
                        {
                            Destroy(spawnedUFOs[i].gameObject);
                            spawnedUFOs[i] = null;
                            numUFOsDestroyed++;
                        }
                    }
                    if (numUFOsDestroyed >= numUFOs)
                    {
                        GameObject waveCompleteText = Instantiate(basicUIText, uiParent);
                        SpriteRenderer sr = waveCompleteText.GetComponent<SpriteRenderer>();
                        sr.sprite = waveCompleteTextSprite;
                        currAnimator = waveCompleteText.GetComponent<Animator>();
                        state = PlayState.WAVE_COMPLETE;
                    }
                    break;
                }
            case PlayState.WAVE_COMPLETE:
                {
                    if (currAnimator && currAnimator.GetCurrentAnimatorStateInfo(0).IsName("TextGone"))
                    {
                        Destroy(currAnimator.gameObject);
                        currAnimator = null;
                        if (currLevel < (levelDatas.Count - 1))
                        {
                            currLevel++;
                            stateTimer = Time.fixedTime;
                            state = PlayState.START;
                        }
                        else
                        {
                            // TODO
                            Debug.Log("GAME WIN!");
                        }
                    }
                    break;
                }
        }
    }

    private void SpawnBackgroundUFO()
    {
        int ufoNum = 0;
        for (int i = 0; i < backgroundUFOs.Length; i++)
        {
            if (backgroundUFOs[i] == null)
            {
                ufoNum = i;
                break;
            }
        }
        LevelData.UFOData ufoData = levelDatas[currLevel].ufoList[ufoNum];

        GameObject backgroundUFO = Instantiate(backgroundUFOObject, new Vector3(0, ufoData.startingBackgroundYPos), Quaternion.identity, backgroundObjectsParent);
        BackgroundUFOController backgroundUFOController = backgroundUFO.GetComponent<BackgroundUFOController>();
        backgroundUFOController.mapCellLocation = ufoData.startingCell;

        float initXPos = ParallaxController.instance.GetInitXPos(backgroundUFOController.mapCellLocation, backgroundUFOController.parallaxDistance);
        // Want to make sure all UFOs are on screen, so readjust the bounds
        float parallaxDistBounds = (Constants.PIXELS_PER_UNIT * (ParallaxController.instance.GetMapSize() / 2)) / Mathf.Pow(2, backgroundUFOController.parallaxDistance);
        while (initXPos > parallaxDistBounds) initXPos -= parallaxDistBounds * 2;
        while (initXPos < -parallaxDistBounds) initXPos += parallaxDistBounds * 2;
        initXPos *= (Constants.SCREEN_BOUNDS + 0.75f) / parallaxDistBounds;
        backgroundUFO.transform.position = new Vector2(initXPos, backgroundUFO.transform.position.y);

        backgroundUFOs[ufoNum] = backgroundUFOController;
        ParallaxController.instance.RegisterNewParallaxObj(backgroundUFOController);

        if (ufoNum < (levelDatas[currLevel].ufoList.Count - 1))
        {
            Invoke("SpawnBackgroundUFO", backgroundUFOSpawnTime);
        }
    }

    private void DescendNextBackgroundUFO()
    {
        for (int i = 0; i < backgroundUFOs.Length; i++)
        {
            if (backgroundUFOs[i] != null && backgroundUFOs[i].IsIdling())
            {
                backgroundUFOs[i].Descend();
                if (i < (backgroundUFOs.Length - 1))
                {
                    Invoke("DescendNextBackgroundUFO", levelDatas[currLevel].timeToNextUFO);
                }
                return;
            }
        }
    }

    private void SpawnUFO(int ufoNum)
    {
        LevelData.UFOData ufoData = levelDatas[currLevel].ufoList[ufoNum];

        GameObject ufo = Instantiate(ufoPrefab, sceneObjectsParent);
        UFOController ufoController = ufo.GetComponent<UFOController>();
        ufoController.Init(ufoData.startingCell);
        ufo.transform.position = new Vector2(ParallaxController.instance.GetInitXPos(ufoController.mapCellLocation, ufoController.parallaxDistance), ufo.transform.position.y);
        ParallaxController.instance.RegisterNewParallaxObj(ufoController);
        spawnedUFOs[ufoNum] = ufoController;

        Destroy(backgroundUFOs[ufoNum].gameObject);
        backgroundUFOs[ufoNum] = null;
    }
}
