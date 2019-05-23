using UnityEngine;

public class AtackingResetExit : StateMachineBehaviour
{
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("IsAttacking", false);	
	}
}
