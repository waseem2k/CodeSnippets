/// <summary>
/// Simple method for toggling between several objects
/// Here I'm using it to toggle between different menu's in a UI
/// SetActive(); Is very efficient as it checks the state first before executing
///     - This way it's not toggling panels for no reason and makes for cleaner code
/// </summary>
private void TogglePanels(GameObject panel)
{
	PauseMenu.TogglePauseMenu(panel == gamePanel);
	gamePanel.SetActive(panel == gamePanel);
	mainMenuPanel.SetActive(panel == mainMenuPanel);
	winScreenPanel.SetActive(panel == winScreenPanel);
	levelSelectPanel.SetActive(panel == levelSelectPanel);
	creditsPanel.SetActive(panel == creditsPanel);
}

/// <summary>
/// Gets the level number value from scene name
/// Eg. We check "Level36" with the value "Level", it returns 36
/// </summary>
public static int SetCurrentScene(Scene scene, string val) // Sets the current scene index
{
    if (scene.name.Contains(val))
    {
        string n = scene.name.Remove(0, val.length);
        int i;
        int.TryParse(n, out i);
        return i;
    }
    return 0; // Defaults to zero if a number hasn't been found
}

/// <summary>
/// Returns a velocity for rigidbody object to be thrown and arrive with a given time
/// </summary>
public static Vector2 CalculateTrajectoryWithTime(Vector2 originPos, Vector2 targetPosition, float timeToTargetPosition)
{
	if (timeToTargetPosition <= 0.0f)
	{
		Debug.LogError("Invalid time set");
	}

	float gravity = Physics2D.gravity.y;

	// calculate forward speed
	Vector2 startToEndFlat = targetPosition - originPos;
	startToEndFlat.y = 0.0f;
	float flatDistance = startToEndFlat.magnitude;
	var forwardSpeed = flatDistance / timeToTargetPosition;

	// calculate vertical speed
	float heightDiff = targetPosition.y - originPos.y;
	float upSpeed = (heightDiff - (0.5f * gravity * timeToTargetPosition * timeToTargetPosition))
			/ timeToTargetPosition;

	// initialize velocity
	Vector2 velocity = startToEndFlat.normalized * forwardSpeed;
	velocity.y = upSpeed;

	return velocity;
}

/// <summary>
/// Sets the velocity of a rigidbody object to hit a target position at a given speed/force
/// </summary>
public static Vector2 CalculateTrajectoryWithSpeed(Vector2 originPos, Vector2 targetPosition, float force, bool highAngle)
{
	float gravity = Physics2D.gravity.y;
	Vector2 startToEndFlat = targetPosition - originPos;
	startToEndFlat.y = 0.0f;
	float flatDistance = startToEndFlat.magnitude;
	float heightDiff = targetPosition.y - originPos.y;

	float toRoot = Mathf.Pow(force, 4.0f);
	toRoot += gravity * (-gravity * flatDistance * flatDistance + 2.0f * heightDiff * force * force);
	if (toRoot < 0.0f)
	{
		toRoot = 0.0f;
	}

	float root = Mathf.Sqrt(toRoot);
	Vector2 startToEndFlatNorm = startToEndFlat.normalized;
	Vector4 horizonAxis = Vector3.Cross(startToEndFlatNorm, Vector3.up);

	if (highAngle)
	{
		float angle1 = ((force * force) + root) / (-gravity * flatDistance);
		angle1 = Mathf.Atan(angle1);
		float angle1Deg = Mathf.Rad2Deg * angle1;

		Vector3 direction = Quaternion.AngleAxis(angle1Deg, horizonAxis) * startToEndFlatNorm;

		return direction * force;
	}
	else
	{
		float angle2 = ((force * force) - root) / (-gravity * flatDistance);
		angle2 = Mathf.Atan(angle2);
		float angle2Deg = Mathf.Rad2Deg * angle2;
		Vector3 direction2 = Quaternion.AngleAxis(angle2Deg, horizonAxis) * startToEndFlatNorm;

		return direction2 * force;
	}
}
