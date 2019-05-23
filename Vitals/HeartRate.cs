using UnityEngine;
using Random = UnityEngine.Random;

public class HeartRate : MonoBehaviour
{
	public float CurrentHeartRate = 60;
	public float heartRateMultiplier = 0.5f;

	private const float relaxedTarget = 60f;
	private const float movingTarget = 90f;
	private const float runningTarget = 120f; // 
	private const float overwhelmedLimit = 160f; // The limit at which the player has trouble moving
	private const float dangerLimit = 200f; // The limit at which the player dies

	private float targetHeartRate;

	private ThirdPersonController character;
	private bool isRelaxed;
	private bool IsMoving { get { return character.IsMoving; } }
	private bool IsRunning { get { return character.IsRunning; } }
	private bool IsJumping { get { return character.IsJumping; } }

	private Oxygen oxygen;
	public float oxygentDecreaseRate = 0.25f;

	public void HeartRateBurst(float amount)
	{
		// On damage taken or other variables
		CurrentHeartRate += amount;
	}

	private void Start()
	{
		character = GetComponent<ThirdPersonController>();
		oxygen = GetComponent<Oxygen>();
		CurrentHeartRate = 60;
		UIManager.HeartRateMonitor.SetHeartRate(CurrentHeartRate);
		targetHeartRate = relaxedTarget;
		isRelaxed = true;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.U))
		{
			CurrentHeartRate += 10f;
		}
		UpdateTarget();
		UpdateMultiplier();
		UpdateUI();
		UpdateOxygenUse();
	}

	private void UpdateOxygenUse()
	{
		if (isRelaxed)
		{
			oxygentDecreaseRate = 0;
		}
		else if (CurrentHeartRate > relaxedTarget && CurrentHeartRate < movingTarget)
		{
			oxygentDecreaseRate = 0.25f;
		}
		else if (CurrentHeartRate > movingTarget && CurrentHeartRate < runningTarget)
		{
			oxygentDecreaseRate = 0.5f;
		}
		else if (CurrentHeartRate > runningTarget && CurrentHeartRate < overwhelmedLimit)
		{
			oxygentDecreaseRate = 1f;
		}
		else if (CurrentHeartRate > overwhelmedLimit && CurrentHeartRate < dangerLimit)
		{
			oxygentDecreaseRate = 2f;
		}
		else if (CurrentHeartRate > dangerLimit)
		{
			oxygentDecreaseRate = 5f;
		}
		oxygen.DecreaseOxygen(oxygentDecreaseRate * Time.deltaTime);
	}

	// Check the players state and set a target for the bpm to reach
	private void UpdateTarget()
	{
		if (!IsMoving)
		{
			targetHeartRate = relaxedTarget;
			isRelaxed = true;
		}
		else if (IsMoving && IsRunning)
		{
			targetHeartRate = runningTarget;
			isRelaxed = false;
		}
		else if (IsMoving && !IsRunning)
		{
			targetHeartRate = movingTarget;
			isRelaxed = false;
		}
		if (IsJumping) isRelaxed = false;

	}

	private void UpdateMultiplier()
	{
		if (isRelaxed)
		{
			heartRateMultiplier = Random.Range(-1, 3);
		}

		if (CurrentHeartRate <= targetHeartRate && IsMoving) // If moving and heart rate is less than target
		{
			heartRateMultiplier = 0.75f;
		}

		else if (CurrentHeartRate > targetHeartRate && IsMoving) // If moving and heart rate is more then target, reduce multiplier
		{
			heartRateMultiplier = 0.15f;
		}

		else if (CurrentHeartRate > targetHeartRate && !IsMoving) // If heart rate is greater than target and we're not moving
		{													// Decrease heart rate at slower rate
			heartRateMultiplier = -0.45f;
		}

		if (IsMoving && IsRunning)
		{
			heartRateMultiplier *= 2;
		}

		if (IsJumping)
		{
			heartRateMultiplier += 4;
		}

		CurrentHeartRate += heartRateMultiplier * Time.deltaTime;
	}

	private void UpdateUI()
	{
		UIManager.HeartRateMonitor.SetHeartRate(CurrentHeartRate);
	}

	// Limits Relaxed 60, Running 90, Slow Movement at 160, Death at 200 
}
