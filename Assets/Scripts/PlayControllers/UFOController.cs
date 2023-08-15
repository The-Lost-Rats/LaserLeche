using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UFOController : ParallaxObject
{
    private enum UFOState
    {
        ENTERING = 0,
        IDLE = 1,
        LASER = 2,
        DEATH = 3,
        DEAD = 4
    }

    [Header("References")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private GameObject ufoLaser;

    [Header("UFO Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float invulnerabilityLength = 0.01f;
    [SerializeField]
    [Range(1, 10)]
    private int ufoMaxHealth = 1;
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float shouldDoSomethingTimer = 0.01f;
    
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
    [SerializeField]
    [Range(0.01f, 1.0f)]
    private float movementSpeed = 0.01f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float movementHoverTime;

    [Header("Laser State Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float laserTime = 0.01f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float laserCooldownTime = 0.0f;

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

    private bool canMove;
    private int[] movementBounds;
    private enum UFOMovementState
    {
        MOVING_TO_A,
        MOVING_TO_B,
        HOVERING
    }
    private UFOMovementState movementState;
    private UFOMovementState lastMovementState;
    private float hoverStateStartTime;

    private float lastLaserTime = -1;
    private float stateStartTime;
    private int lastDeathFadeFrame;

    protected override void OnStart()
    {
        animator = GetComponent<Animator>();

        initYPos = transform.position.y;
        ufoHealth = ufoMaxHealth;

        SetState(UFOState.ENTERING);
        SetYPosition(12.25f); // Put UFO just offscreen
    }

    public void Init(int startingCell, int[] movementBounds)
    {
        mapCellLocation = startingCell;
        if (movementBounds != null)
        {
            canMove = true;
            this.movementBounds = movementBounds;
        }
    }

    protected void Update()
    {
        switch (ufoState)
        {
            case UFOState.ENTERING:
            {
                if (Time.time - stateStartTime <= enterLength)
                {
                    float newY = ((12.25f - initYPos) * Mathf.Pow(1 - ((Time.time - stateStartTime) / enterLength), 3)) + initYPos;
                    SetYPosition(newY);
                }
                else
                {
                    SetState(UFOState.IDLE);
                }
                break;
            }
            case UFOState.IDLE:
            {
                float newY = initYPos + (Mathf.Sin((Time.time - stateStartTime) * floatSpeed) * (floatAmplitude / 8.0f)) - (floatAmplitude % 2 == 0 ? 0.0f : 0.125f);
                SetYPosition(newY);
                if (canMove)
                {
                    switch (movementState)
                    {
                        case UFOMovementState.MOVING_TO_A:
                        case UFOMovementState.MOVING_TO_B:
                            if (GetCurrMapCell() == movementBounds[movementState == UFOMovementState.MOVING_TO_A ? 0 : 1])
                            {
                                UpdateMovementState(false);
                            }
                            break;
                        case UFOMovementState.HOVERING:
                            if (Time.time - hoverStateStartTime > movementHoverTime)
                            {
                                UpdateMovementState(true);
                            }
                            break;
                    }
                }
                CheckShouldDoSomething();
                break;
            }
            case UFOState.LASER:
            {
                // TODO Move to center of cell
                if (Time.time - stateStartTime > laserTime)
                {
                    SetState(UFOState.IDLE);
                }
                break;
            }
            case UFOState.DEATH:
            {
                float deltaTime = Time.time - stateStartTime;
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
                    SetState(UFOState.DEAD);
                }
                break;
            }
        }
        animator.SetInteger("UFOState", (int)ufoState);
    }

    private bool SetState(UFOState newState)
    {
        // Check if can enter into new state
        if (ufoState == newState) return false;
        switch (newState)
        {
            case UFOState.LASER:
                if (lastLaserTime >= 0 && Time.time - lastLaserTime <= laserCooldownTime) return false;
                break;
        }

        // Handle leaving a state
        switch (ufoState)
        {
            case UFOState.ENTERING:
                lastLaserTime = Time.time; // This will ensure we cool down the laser
                break;
            case UFOState.IDLE:
                if (canMove && movementState != UFOMovementState.HOVERING)
                {
                    UpdateMovementState(false);
                }
                break;
            case UFOState.LASER:
                ufoLaser.SetActive(false);
                lastLaserTime = Time.time;
                break;
        }

        // Enter new state
        stateStartTime = Time.time;
        ufoState = newState;

        // Handle entering a state
        switch (ufoState)
        {
            case UFOState.IDLE:
                if (canMove)
                    UpdateMovementState(true);
                break;
            case UFOState.LASER:
                ufoLaser.SetActive(true);
                break;
            case UFOState.DEATH:
                lastDeathFadeFrame = 0;
                break;
        }
        return true;
    }

    private void UpdateMovementState(bool move)
    {
        if (!canMove) return;

        if (move)
        {
            // TODO Maybe try to move away from the player?
            if (lastMovementState == UFOMovementState.MOVING_TO_A)
                movementState = GetCurrMapCell() != movementBounds[0] ? UFOMovementState.MOVING_TO_A : UFOMovementState.MOVING_TO_B;
            else
                movementState = GetCurrMapCell() != movementBounds[1] ? UFOMovementState.MOVING_TO_B : UFOMovementState.MOVING_TO_A;
            relativeSpeed = movementState == UFOMovementState.MOVING_TO_A ? -movementSpeed : movementSpeed;
            lastMovementState = UFOMovementState.HOVERING;
        }
        else
        {
            relativeSpeed = 0;
            hoverStateStartTime = Time.time;
            lastMovementState = movementState;
            movementState = UFOMovementState.HOVERING;
        }
    }

    private void CheckShouldDoSomething()
    {
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, Vector2.down, 20.0f, playerLayer);
        if (playerHit.collider != null)
        {
            if (SetState(UFOState.LASER)) return;
        }

        if (Time.time - stateStartTime > shouldDoSomethingTimer)
        {
            // Do something
            if (SetState(UFOState.LASER)) return;
        }
    }

    protected void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Laser"))
        {
            if (!ufoInvulnerable)
            {
                TakeHit();
            }
        }
    }

    private void TakeHit()
    {
        ufoHealth--;
        ufoInvulnerable = true;
        if (ufoHealth > 0)
        {
            animator.SetBool("UFOHit", true);
            Invoke("DisableHitAnimation", 0.1f);
            Invoke("TurnOffInvulnerability", invulnerabilityLength);
            SetState(UFOState.IDLE);
        }
        else
        {
            SetState(UFOState.DEATH);
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
