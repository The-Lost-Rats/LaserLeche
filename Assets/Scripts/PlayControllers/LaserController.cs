using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    protected void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    protected void Update()
    {
        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(playerController.PlayerVelX));
        animator.SetFloat("PlayerVelY", playerController.PlayerVelY);
    }
}
