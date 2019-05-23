using UnityEngine;

public class ThirdPersonController : ThirdPersonMotor
{
	public void Run(bool value)
	{
		IsRunning = value;
	}

	public void StartJump()
	{
		if (!IsJumping && IsGrounded/* && !startJumping*/)
		{
			animator.SetTrigger("StartJumping");
			//startJumping = true;
		}
	}

	public void Jump()
	{
		if (!IsJumping && IsGrounded)
		{
			jumpCounter = jumpTimer;
			IsJumping = true;
		}
	}



	public void RotateWithAnotherTransform(Transform referenceTransform)
	{
		Vector3 newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), rotationSpeed * Time.fixedDeltaTime);
		targetRotation = transform.rotation;
	}

	public void UpdateAnimator()
	{
		if (animator == null || !animator.enabled) return;

		animator.SetBool("IsGrounded", IsGrounded);
		animator.SetBool("IsJumping", IsJumping);

		if (!IsGrounded || IsJumping)
		{
			animator.SetFloat("AirVelocity", verticalVelocity);
		}

		IsMoving = direction > 0.5f || direction < -0.5f || speed > 0.5f || speed < -0.5f;
		animator.SetBool("IsMoving", IsMoving);

		animator.SetFloat("InputHorizontal", direction);

		animator.SetFloat("InputVertical", speed);
		animator.SetFloat("GroundDistance", groundDistance);
	}

	public void OnAnimatorMove()
	{
		// we implement this function to override the default root motion.
		// this allows us to modify the positional speed before it's applied.
		if (IsGrounded)
		{
			transform.rotation = animator.rootRotation;

			float speedDir = Mathf.Abs(direction) + Mathf.Abs(speed);
			speedDir = Mathf.Clamp(speedDir, 0, 1);
			//float movementState = speedDir; // (isSprinting ? 1.5f : 1f) * Mathf.Clamp(speedDir, 0f, 1f);

			float movementState = Mathf.Lerp(0, 0.7f, movementLerpState);

			float moveSpeed = Mathf.Lerp(walkSpeed, runSpeed, movementLerpState);

			animator.SetFloat("MovementState", movementState);
			ControlSpeed(moveSpeed);
		}
	}

}
