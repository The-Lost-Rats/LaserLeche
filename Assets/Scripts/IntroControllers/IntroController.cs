using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroController : MonoBehaviour
{
    private enum IntroState
    {
        LOADING = 0,
        WAITING_FOR_INPUT = 1,
        FADE_OUT_LOGO = 2,
        LECHE_ANIM = 3,
        FADE_OUT = 4,
    }

    [SerializeField]
    private Animator logoAnimator;

    [SerializeField]
    private Animator creditsAnimator;

    [SerializeField]
    private Animator lecheAnimator;

    [SerializeField]
    private Animator overlayAnimator;

    private bool startedPlayingGameMusic = false;

    private IntroState state;

    private bool credits;

    protected void Start()
    {
        state = IntroState.LOADING;
        AudioController.Instance.PlayMusic(MusicKeys.IntroMusic);
        credits = GameController.instance.gameWon;
        overlayAnimator.gameObject.SetActive(credits);
        if (credits)
        {
            overlayAnimator.SetBool("OverlayOut", true);
            Invoke("HackyTurnOffOverlayOut", 0.1f);
        }
        logoAnimator.gameObject.SetActive(!credits);
        creditsAnimator.gameObject.SetActive(credits);
        GameController.instance.gameWon = false;
    }

    protected void Update()
    {
        switch (state)
        {
            case IntroState.LOADING:
                if (
                    (!credits && logoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Logo_Loop"))
                    || (
                        credits
                        && !overlayAnimator.GetBool("OverlayOut")
                        && overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Overlay_Out")
                    )
                )
                {
                    state = IntroState.WAITING_FOR_INPUT;
                }
                break;
            case IntroState.WAITING_FOR_INPUT:
                if (Input.anyKey)
                {
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Button);
                    state = IntroState.FADE_OUT_LOGO;
                }
                break;
            case IntroState.FADE_OUT_LOGO:
                if (
                    (!credits && logoAnimator.GetCurrentAnimatorStateInfo(1).IsName("Logo_Gone"))
                    || (
                        credits
                        && creditsAnimator
                            .GetCurrentAnimatorStateInfo(1)
                            .IsName("Credits_Fade_Done")
                    )
                )
                {
                    if (!credits)
                    {
                        Destroy(logoAnimator.gameObject);
                        logoAnimator = null;
                    }
                    else
                    {
                        Destroy(creditsAnimator.gameObject);
                        creditsAnimator = null;
                    }
                    SwitchMusic();
                    state = IntroState.LECHE_ANIM;
                }
                break;
            case IntroState.LECHE_ANIM:
                if (
                    lecheAnimator.GetCurrentAnimatorStateInfo(0).IsName("Leche_Stands")
                    && !startedPlayingGameMusic
                )
                {
                    AudioController.Instance.PlayMusic(MusicKeys.GameMusic);
                    startedPlayingGameMusic = true;
                }
                if (lecheAnimator.GetCurrentAnimatorStateInfo(0).IsName("Leche_Gone"))
                {
                    overlayAnimator.gameObject.SetActive(true);
                    overlayAnimator.SetBool("OverlayIn", true);
                    state = IntroState.FADE_OUT;
                }
                break;
            case IntroState.FADE_OUT:
                if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Overlay_In"))
                {
                    GameController.instance.ChangeState(GameState.PLAY);
                }
                break;
        }

        if (logoAnimator)
            logoAnimator.SetInteger("IntroState", (int)state);
        if (lecheAnimator)
            lecheAnimator.SetInteger("IntroState", (int)state);
        if (creditsAnimator)
            creditsAnimator.SetInteger("IntroState", (int)state);
    }

    private void HackyTurnOffOverlayOut()
    {
        overlayAnimator.SetBool("OverlayOut", false);
    }

    private void SwitchMusic()
    {
        float newVolume = AudioController.Instance.AdjustMusicVolume(-0.005f);
        if (newVolume > 0.01f)
        {
            Invoke("SwitchMusic", 0.1f);
        }
    }
}
