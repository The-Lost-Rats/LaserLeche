using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundUFOController : ParallaxObject
{
    private enum BackgroundUFOState
    {
        ENTERING,
        IDLE,
        DESCENDING,
        DESCENDED
    }

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

    [Header("Descending State Variables")]
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float descendingSpeed = 0.01f;

    private float initYPos;
    private BackgroundUFOState state;
    private float stateStartTime;

    protected override void OnStart()
    {
        initYPos = transform.position.y;
        SetYPosition(8.75f); // Put UFO just offscreen
        stateStartTime = Time.time;
    }

    public void Descend()
    {
        state = BackgroundUFOState.DESCENDING;
    }

    public bool IsIdling()
    {
        return state == BackgroundUFOState.IDLE;
    }

    public bool IsDescending()
    {
        return state == BackgroundUFOState.DESCENDING;
    }

    public bool IsDescended()
    {
        return state == BackgroundUFOState.DESCENDED;
    }

    protected void Update()
    {
        switch (state)
        {
            case BackgroundUFOState.ENTERING:
            {
                if (Time.time - stateStartTime <= enterLength)
                {
                    float newY = ((12.25f - initYPos) * Mathf.Pow(1 - ((Time.time - stateStartTime) / enterLength), 3)) + initYPos;
                    SetYPosition(newY);
                }
                else
                {
                    state = BackgroundUFOState.IDLE;
                    stateStartTime = Time.time;
                }
                break;
            }
            case BackgroundUFOState.IDLE:
            {
                float newY = initYPos + (Mathf.Sin((Time.time - stateStartTime) * floatSpeed) * (floatAmplitude / 8.0f)) - (floatAmplitude % 2 == 0 ? 0.0f : 0.125f);
                SetYPosition(newY);
                break;
            }
            case BackgroundUFOState.DESCENDING:
            {
                float newYPos = transform.position.y - (Time.deltaTime * descendingSpeed);
                SetYPosition(newYPos);
                if (newYPos <= -4)
                {
                    state = BackgroundUFOState.DESCENDED;
                }
                break;
            }
            case BackgroundUFOState.DESCENDED:
                break;
        }
    }
}
