using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    private bool imFiringMahLazer;

    protected void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();
        imFiringMahLazer = false;
    }

    protected void FixedUpdate()
    {
        CheckForLaserBlast();

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(playerController.PlayerVelX));
        animator.SetFloat("PlayerVelY", playerController.PlayerVelY);
        animator.SetBool("LaserBlast", imFiringMahLazer);
    }

    private void CheckForLaserBlast()
    {
        imFiringMahLazer = Input.GetKey(KeyCode.Space);
    }
}
