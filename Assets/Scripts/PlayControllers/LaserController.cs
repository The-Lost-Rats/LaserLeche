using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    public SpriteRenderer spriteRenderer;

    public bool ImFiringMahLazer { get; private set; }
    private int laserSoundEffectId = -1;

    protected void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ImFiringMahLazer = false;
    }

    protected void FixedUpdate()
    {
        if (!PlayController.instance.IsGameOver() && !PlayController.instance.IsGameWon())
        {
            CheckForLaserBlast();
        }
        else
        {
            StopFiringLaser();
        }

        animator.SetFloat("PlayerVelXAbs", Mathf.Abs(playerController.PlayerVelX));
        animator.SetFloat("PlayerVelY", playerController.PlayerVelY);
        animator.SetBool("LaserBlast", ImFiringMahLazer);
    }

    private void CheckForLaserBlast()
    {
        if (Input.GetKey(KeyCode.Space) && !ImFiringMahLazer)
            StartFiringLaser();
        else if (!Input.GetKey(KeyCode.Space) && ImFiringMahLazer)
            StopFiringLaser();
    }

    private void StartFiringLaser()
    {
        ImFiringMahLazer = true;
        Invoke("PlayLaserSoundEffect", 0.4f);
    }

    public void StopFiringLaser()
    {
        CancelInvoke("PlayLaserSoundEffect");
        ImFiringMahLazer = false;
        if (laserSoundEffectId >= 0)
        {
            AudioController.Instance.StopOneShotAudio(laserSoundEffectId);
            laserSoundEffectId = -1;
        }
    }

    private void PlayLaserSoundEffect()
    {
        laserSoundEffectId = AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.LecheLaser, true);
    }
}
