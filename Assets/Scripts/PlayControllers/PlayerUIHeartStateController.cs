using UnityEngine;

public class PlayerUIHeartStateController : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state
    // machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("UIHeart_Intro"))
        {
            animator.SetTrigger("PlayerHeartAnimationEntered");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("UIHeart_Exit"))
        {
            animator.SetTrigger("PlayerHeartAnimationExited");
        }
    }
}
