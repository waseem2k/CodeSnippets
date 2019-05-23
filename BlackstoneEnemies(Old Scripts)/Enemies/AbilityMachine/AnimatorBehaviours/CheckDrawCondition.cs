using UnityEngine;

public class CheckDrawCondition : StateMachineBehaviour
{
/*	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("DrawComplete", false);
	}*/

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("DrawComplete", true);
	}
}
