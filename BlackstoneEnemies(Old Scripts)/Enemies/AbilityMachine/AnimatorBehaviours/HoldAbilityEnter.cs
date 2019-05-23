using UnityEngine;

public class HoldAbilityEnter : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("AbilityHold", true);
	}
}
