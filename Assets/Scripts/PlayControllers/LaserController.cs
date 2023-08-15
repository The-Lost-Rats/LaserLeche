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
        if (!PlayController.instance.IsGameOver())
        {
            CheckForLaserBlast();
        }
        else
        {
            ImFiringMahLazer = false;
        }

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(playerController.PlayerVelX));
        animator.SetFloat("PlayerVelY", playerController.PlayerVelY);
        animator.SetBool("LaserBlast", ImFiringMahLazer);
    }

    private void CheckForLaserBlast()
    {
        if (Input.GetKey(KeyCode.Space) && !ImFiringMahLazer)
            ImFiringMahLazer = true;
        else if (!Input.GetKey(KeyCode.Space) && ImFiringMahLazer)
            ImFiringMahLazer = false;
    }
}
