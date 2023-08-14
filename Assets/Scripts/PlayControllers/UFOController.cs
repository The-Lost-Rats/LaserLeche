using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOController : ParallaxObject
{
    private enum UFOState
    {
        ENTERING,
        IDLE,
        DEATH,
        DEAD
    }

    [Header("UFO Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float invulnerabilityLength = 0.01f;
    [SerializeField]
    [Range(1, 10)]
    private int ufoMaxHealth = 1;
    
    [Header("Entering State Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float enterLength = 0.01f;

    [Header("Idle State Variables")]
    [SerializeField]
    [Range(0, 5)]
    private int floatAmplitude;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float floatSpeed;

    [Header("Death State Variables")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float deathLength;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float deathSpeed;
    [SerializeField]
    [Range(1, 10)]
    private int deathFadeFrames = 1;

    private Animator animator;

    private float initYPos;
    private UFOState ufoState;
    private int ufoHealth;
    private bool ufoInvulnerable;

    private float enterStartTime;
    private float idleFloatStartTime;
    private float deathStartTime;
    private int lastDeathFadeFrame;

    protected override void OnStart()
    {
        animator = GetComponent<Animator>();

        initYPos = transform.position.y;
        ufoHealth = ufoMaxHealth;

        ufoState = UFOState.ENTERING;
        SetYPosition(12.25f); // Put UFO just offscreen
        enterStartTime = Time.time;
    }

    // TODO Add in more info on how the UFO works
    public void Init(int startingCell)
    {
        mapCellLocation = startingCell;
    }

    protected void Update()
    {
        switch (ufoState)
        {
            case UFOState.ENTERING:
            {
                if (Time.time - enterStartTime <= enterLength)
                {
                    float newY = ((12.25f - initYPos) * Mathf.Pow(1 - ((Time.time - enterStartTime) / enterLength), 3)) + initYPos;
                    SetYPosition(newY);
                }
                else
                {
                    ufoState = UFOState.IDLE;
                    idleFloatStartTime = Time.time;
                }
                break;
            }
            case UFOState.IDLE:
            {
                float newY = initYPos + (Mathf.Sin((Time.time - idleFloatStartTime) * floatSpeed) * (floatAmplitude / 8.0f)) - (floatAmplitude % 2 == 0 ? 0.0f : 0.125f);
                SetYPosition(newY);
                break;
            }
            case UFOState.DEATH:
            {
                float deltaTime = Time.time - deathStartTime;
                if (deltaTime <= deathLength)
                {
                    float newY = transform.position.y - (deathSpeed * Time.deltaTime);
                    SetYPosition(newY);
                    int fadeFrame = (int)Mathf.Floor(deltaTime / (deathLength / deathFadeFrames));
                    if (fadeFrame > lastDeathFadeFrame)
                    {
                        Color newColor = new Color(1, 1, 1, (deathFadeFrames - fadeFrame) / (float)deathFadeFrames);
                        renderer.color = newColor;
                        lastDeathFadeFrame = fadeFrame;
                    }
                }
                else
                {
                    ufoState = UFOState.DEAD;
                }
                break;
            }
        }
        animator.SetInteger("UFOState", (int)ufoState);
    }

    protected void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Laser"))
        {
            if (!ufoInvulnerable)
            {
                ufoHealth--;
                ufoInvulnerable = true;
                if (ufoHealth > 0)
                {
                    animator.SetBool("UFOHit", true);
                    Invoke("DisableHitAnimation", 0.1f);
                    Invoke("TurnOffInvulnerability", invulnerabilityLength);
                }
                else
                {
                    deathStartTime = Time.time;
                    lastDeathFadeFrame = 0;
                    ufoState = UFOState.DEATH;
                }
            }
        }
    }

    public bool IsDead()
    {
        return ufoState == UFOState.DEAD;
    }

    private void DisableHitAnimation()
    {
        animator.SetBool("UFOHit", false);
    }

    private void TurnOffInvulnerability()
    {
        ufoInvulnerable = false;
    }
}
