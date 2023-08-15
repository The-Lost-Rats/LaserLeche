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
        FADE_OUT = 4
    }

    [SerializeField] private Animator logoAnimator;
    [SerializeField] private Animator lecheAnimator;
    [SerializeField] private Animator overlayAnimator;

    private bool startedPlayingGameMusic = false;

    private IntroState state;

    protected void Start()
    {
        state = IntroState.LOADING;
        AudioController.Instance.PlayMusic(MusicKeys.IntroMusic);
    }

    protected void Update()
    {
        switch (state)
        {
            case IntroState.LOADING:
                if (logoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Logo_Loop"))
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
                if (logoAnimator.GetCurrentAnimatorStateInfo(1).IsName("Logo_Gone"))
                {
                    Destroy(logoAnimator.gameObject);
                    logoAnimator = null;
                    SwitchMusic();
                    state = IntroState.LECHE_ANIM;
                }
                break;
            case IntroState.LECHE_ANIM:
                if (lecheAnimator.GetCurrentAnimatorStateInfo(0).IsName("Leche_Stands") && !startedPlayingGameMusic)
                {
                    AudioController.Instance.PlayMusic(MusicKeys.GameMusic);
                    startedPlayingGameMusic = true;
                }
                if (lecheAnimator.GetCurrentAnimatorStateInfo(0).IsName("Leche_Gone"))
                {
                    overlayAnimator.gameObject.SetActive(true);
                    state = IntroState.FADE_OUT;
                }
                break;
            case IntroState.FADE_OUT:
                if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Overlay_Done"))
                {
                    GameController.instance.ChangeState(GameState.PLAY);
                }
                break;
        }

        if (logoAnimator) logoAnimator.SetInteger("IntroState", (int)state);
        if (lecheAnimator) lecheAnimator.SetInteger("IntroState", (int)state);
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
