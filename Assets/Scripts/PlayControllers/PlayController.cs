using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayController : MonoBehaviour
{
    public static PlayController instance = null;

    private enum PlayState
    {
        START = 0,
        WAVE_TEXT = 1,
        BACKGROUND_UFO_SPAWN = 2,
        BEGIN_TEXT = 3,
        GAMEPLAY = 4,
        WAVE_COMPLETE = 5,
        GAME_WON_A = 6,
        GAME_WON_B = 7,
        GAME_OVER = 8
    }

    [Header("Game Data")]
    [SerializeField] private List<LevelData> levelDatas;

    [Header("References")]
    [SerializeField] private Transform uiParent;
    [SerializeField] private Transform backgroundObjectsParent;
    [SerializeField] private Transform sceneObjectsParent;
    [SerializeField] private GameObject overlay;
    [SerializeField] private SpriteRenderer tapToContinueRenderer;

    [Header("Prefabs")]
    [SerializeField] private GameObject basicUIText;
    [SerializeField] private GameObject backgroundUFOObject;
    [SerializeField] private GameObject ufoPrefab;
    [SerializeField] private GameObject healthPickupPrefab;

    [Header("State Variables")]
    [SerializeField] [Range(0.1f, 4.0f)] private float startLength = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float backgroundUFOSpawnTime = 0.0f;
    [SerializeField] private Sprite beginTextSprite;
    [SerializeField] private Sprite waveCompleteTextSprite;
    [SerializeField] private Sprite gameWonTextSprite;

    [Header("UFO Arrow")]
    [SerializeField] private SpriteRenderer leftUFOArrow;
    [SerializeField] private SpriteRenderer rightUFOArrow;
    [SerializeField] private List<Sprite> ufoArrowSprites;
    [SerializeField] [Range(0.1f, 2.0f)] private float ufoArrowBlinkRate = 0.1f;

    private PlayState state;

    private int currLevel;
    private float stateTimer;
    private int numUFOsDestroyed;
    
    private enum ArrowState
    {
        HIDDEN,
        LEFT_UFO,
        RIGHT_UFO
    }
    private ArrowState currArrowState;
    private float lastArrowUpdateTime;

    private Animator currAnimator;
    private BackgroundUFOController[] backgroundUFOs;
    private UFOController[] spawnedUFOs;

    public void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    protected void Start()
    {
        state = PlayState.START;
        stateTimer = Time.fixedTime;
        currLevel = GameController.instance.level;
        currArrowState = ArrowState.HIDDEN;

        if (!AudioController.Instance.IsMusicPlaying())
        {
            AudioController.Instance.PlayMusic(MusicKeys.GameMusic);
        }

        // Invoke("TestUFO", 0.5f);
    }

    private void TestUFO()
    {
        GameObject ufo = Instantiate(ufoPrefab, sceneObjectsParent);
        UFOController ufoController = ufo.GetComponent<UFOController>();
        ufoController.Init(0, new int[2]{95, 5});
        ufo.transform.position = new Vector2(ParallaxController.instance.GetInitXPos(ufoController.mapCellLocation, ufoController.parallaxDistance), ufo.transform.position.y);
        ParallaxController.instance.RegisterNewParallaxObj(ufoController);
    }

    /** Test function to spawn a health pick in bottom right of screen
     */
    [ContextMenu("Test Health Pickup")]
    private void TestHealthPickup()
    {
        Debug.Log("Creating health pickup!");

        // Instantiate game object and get health pickup controller script
        GameObject healthPickup = Instantiate(healthPickupPrefab, sceneObjectsParent);
        HealthPickupController healthPickupController = healthPickup.GetComponent<HealthPickupController>();

        // Update transform to be bottom right of screen
        healthPickup.transform.position = new Vector2(6, -6);

        // Add controller to list of parallax objects (so it will move in
        // the world)
        ParallaxController.instance.RegisterNewParallaxObj(healthPickupController);
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
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.NextWave);
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
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.NextWave);
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
                        if (currLevel < (levelDatas.Count - 1))
                        {
                            AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.NextWave);
                            GameObject waveCompleteText = Instantiate(basicUIText, uiParent);
                            SpriteRenderer sr = waveCompleteText.GetComponent<SpriteRenderer>();
                            sr.sprite = waveCompleteTextSprite;
                            currAnimator = waveCompleteText.GetComponent<Animator>();
                            state = PlayState.WAVE_COMPLETE;
                        }
                        else
                        {
                            FadeOutMusic();
                            AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.GameWin);
                            GameObject gameWonText = Instantiate(basicUIText, uiParent);
                            SpriteRenderer sr = gameWonText.GetComponent<SpriteRenderer>();
                            sr.sprite = gameWonTextSprite;
                            currAnimator = gameWonText.GetComponent<Animator>();
                            state = PlayState.GAME_WON_A;
                        }
                    }
                    break;
                }
            case PlayState.WAVE_COMPLETE:
                {
                    if (currAnimator && currAnimator.GetCurrentAnimatorStateInfo(0).IsName("TextGone"))
                    {
                        Destroy(currAnimator.gameObject);
                        currAnimator = null;
                        currLevel++;
                        stateTimer = Time.fixedTime;
                        state = PlayState.START;
                    }
                    break;
                }
            case PlayState.GAME_WON_A:
                {
                    if (!AudioController.Instance.OneShotAudioPlaying(SoundEffectKeys.GameWin))
                    {
                        overlay.SetActive(true);
                        currAnimator = overlay.GetComponent<Animator>();
                        currAnimator.SetBool("OverlayIn", true);
                        state = PlayState.GAME_WON_B;
                    }
                    break;
                }
            case PlayState.GAME_WON_B:
                {
                    if (currAnimator && currAnimator.GetCurrentAnimatorStateInfo(0).IsName("Overlay_In"))
                    {
                        GameController.instance.level = 0;
                        GameController.instance.gameWon = true;
                        GameController.instance.ChangeState(GameState.INTRO);
                    }
                    break;
                }
            case PlayState.GAME_OVER:
                {
                    if (tapToContinueRenderer.gameObject.activeSelf && tapToContinueRenderer.color.a >= 0.99f && Input.anyKey)
                    {
                        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Button);
                        GameController.instance.level = currLevel;
                        GameController.instance.ChangeState(GameState.PLAY);
                    }
                    break;
                }
        }

        UpdateArrow();
    }

    private void UpdateArrow()
    {
        ArrowState nextArrowState = state == PlayState.GAMEPLAY ? GetClosestUFO() : ArrowState.HIDDEN;

        if (currArrowState != nextArrowState)
        {
            if (currArrowState == ArrowState.LEFT_UFO)
                leftUFOArrow.gameObject.SetActive(false);
            else
                rightUFOArrow.gameObject.SetActive(false);
            
            if (nextArrowState == ArrowState.LEFT_UFO)
                leftUFOArrow.gameObject.SetActive(true);
            else if (nextArrowState == ArrowState.RIGHT_UFO)
                rightUFOArrow.gameObject.SetActive(true);
            currArrowState = nextArrowState;
        }

        if (currArrowState != ArrowState.HIDDEN && Time.fixedTime - lastArrowUpdateTime > ufoArrowBlinkRate)
        {
            SpriteRenderer arrowSpriteRenderer = currArrowState == ArrowState.LEFT_UFO ? leftUFOArrow : rightUFOArrow;
            arrowSpriteRenderer.sprite = ufoArrowSprites[arrowSpriteRenderer.sprite.name == ufoArrowSprites[0].name ? 1 : 0];
            lastArrowUpdateTime = Time.fixedTime;
        }
    }

    private ArrowState GetClosestUFO()
    {
        bool noUFOsSpawned = true;
        float closestUFOX = 0;
        foreach (UFOController ufoController in spawnedUFOs)
        {
            if (ufoController)
            {
                float ufoX = ufoController.gameObject.transform.position.x;
                if (noUFOsSpawned || Mathf.Abs(ufoX) < Mathf.Abs(closestUFOX))
                    closestUFOX = ufoX;
                noUFOsSpawned = false;
            }
        }
        if (noUFOsSpawned)
            return ArrowState.HIDDEN;
        else
        {
            if (Mathf.Abs(closestUFOX) <= Constants.SCREEN_BOUNDS)
                return ArrowState.HIDDEN;
            return closestUFOX < 0 ? ArrowState.LEFT_UFO : ArrowState.RIGHT_UFO;
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
        int[] movementBounds = ufoData.moving ? new int[2]{ufoData.movementCellA, ufoData.movementCellB} : null;
        ufoController.Init(ufoData.startingCell, movementBounds);
        ufo.transform.position = new Vector2(ParallaxController.instance.GetInitXPos(ufoController.mapCellLocation, ufoController.parallaxDistance), ufo.transform.position.y);
        ParallaxController.instance.RegisterNewParallaxObj(ufoController);
        spawnedUFOs[ufoNum] = ufoController;

        Destroy(backgroundUFOs[ufoNum].gameObject);
        backgroundUFOs[ufoNum] = null;
    }

    public bool IsGameOver()
    {
        return state == PlayState.GAME_OVER;
    }

    public bool IsGameWon()
    {
        return state == PlayState.GAME_WON_A || state == PlayState.GAME_WON_B;
    }

    public void GameOver()
    {
        state = PlayState.GAME_OVER;
        AudioController.Instance.StopAllOneShotAudio();
        AudioController.Instance.StopMusic();
        overlay.SetActive(true);
        overlay.GetComponent<Animator>().SetBool("OverlayIn", true);
        Invoke("FadeInTapToContinue", 2.5f);
    }

    private void FadeInTapToContinue()
    {
        if (!tapToContinueRenderer.gameObject.activeSelf)
        {
            tapToContinueRenderer.gameObject.SetActive(true);
            Color startingColor = tapToContinueRenderer.color;
            startingColor.a = 0;
            tapToContinueRenderer.color = startingColor;
        }
        Color c = tapToContinueRenderer.color;
        c.a += 0.33f;
        tapToContinueRenderer.color = c;
        if (c.a < 1)
        {
            Invoke("FadeInTapToContinue", 0.33f);
        }
    }

    private void FadeOutMusic()
    {
        float newVolume = AudioController.Instance.AdjustMusicVolume(-0.05f);
        if (newVolume > 0.01f)
        {
            Invoke("FadeOutMusic", 0.1f);
        }
        else
        {
            AudioController.Instance.StopMusic();
        }
    }
}
