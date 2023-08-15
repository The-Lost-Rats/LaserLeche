using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    public SpriteRenderer spriteRenderer;

    public bool ImFiringMahLazer;

    protected void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ImFiringMahLazer = false;
    }

    protected void FixedUpdate()
    {
        CheckForLaserBlast();

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(playerController.PlayerVelX));
        animator.SetFloat("PlayerVelY", playerController.PlayerVelY);
        animator.SetBool("LaserBlast", ImFiringMahLazer);
    }

    private void CheckForLaserBlast()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ImFiringMahLazer = true;
        else if (Input.GetKeyUp(KeyCode.Space))
            ImFiringMahLazer = false;
    }
}
