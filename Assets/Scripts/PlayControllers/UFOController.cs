using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOController : ParallaxObject
{
    private enum UFOState
    {
        ENTERING,
        IDLE,
        DEATH
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

    private float initYPos;
    private UFOState ufoState;

    private float enterStartTime;
    private float idleFloatStartTime;

    protected void Start()
    {
        initYPos = transform.position.y;

        ufoState = UFOState.ENTERING;
        SetYPosition(12.25f); // Put UFO just offscreen
        enterStartTime = Time.time;
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
            case UFOState.DEATH:
            {
                break;
            }
            case UFOState.IDLE:
            default:
            {
                float newY = initYPos + (Mathf.Sin((Time.time - idleFloatStartTime) * floatSpeed) * (floatAmplitude / 8.0f)) - (floatAmplitude % 2 == 0 ? 0.0f : 0.125f);
                SetYPosition(newY);
                break;
            }
        }
    }
}
