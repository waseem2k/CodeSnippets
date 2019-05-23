using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A dot based UI instead of the traditional filled image
///	- Each dot represents a percentage of the total health
/// - The current active dot slowly fades out as health decreases
/// - There is also a warning system to show low health
/// </summary>
public class VitalResourceUI : MonoBehaviour
{
	public Text display;
	public float pulseSpeed = 0.5f;
	[Space]
	public DisplayDot[] resourceDot;

	private bool IsResourceLow;
	private float pulseState;
	private int pulseDirection = 1;

	public void UpdateUI(float currentValue, float maxValue)
	{
		IsResourceLow = currentValue < Mathf.Lerp(0, maxValue, 0.12f);
		float step = maxValue / resourceDot.Length;

		if (!IsResourceLow)
		{
			resourceDot[0].SetState(currentValue, 0, step);
			resourceDot[1].SetState(currentValue, step, step * 2);
			resourceDot[2].SetState(currentValue, step * 2, step * 3);
			resourceDot[3].SetState(currentValue, step * 3, step * 4);
			resourceDot[4].SetState(currentValue, step * 4, step * 5);
			resourceDot[5].SetState(currentValue, step * 5, maxValue);
		}
		else
		{
			resourceDot[0].SetLowestColor(currentValue, 0, step);
		}

		if(display != null) display.text = currentValue.ToString("F0") + " / " + maxValue.ToString("F0");
	}

	private void Update()
	{
		if (IsResourceLow)
		{
			pulseState += Time.deltaTime * pulseDirection * pulseSpeed;
			if (pulseState > 1) pulseDirection = -1;
			if (pulseState < 0) pulseDirection = 1;
			resourceDot[0].SetLowestState(pulseState);
			for (int i = 1; i < resourceDot.Length; i++)
			{
				resourceDot[i].SetDangerState(pulseState);
			}
		}
	}
}

[Serializable]
public class DisplayDot
{
	public Image resourceDot;
	private Color faded = new Color(1,1,1,0);
	private Color active = new Color(1,1,1,1);
	private Color danger = new Color(1,0,0,1);
	private Color lowestColor = new Color(1,1,1,0);

    // Gets a lerp value from the health chunk and lerps the colour based on value
	public void SetLowestColor(float currentValue, float minValue, float maxValue)
	{
		float state = Mathf.InverseLerp(minValue, maxValue, currentValue);
		lowestColor = Color.Lerp(faded, active, state);
	}

    // Set the lowest colour (In our case it's just affecting transparency)
	public void SetLowestState(float state)
	{
		resourceDot.color = Color.Lerp(lowestColor, danger, state);
	}

    // Lerps the colour based on the value between faded and active
	public void SetState(float state)
	{
		resourceDot.color = Color.Lerp(faded, active, state);
	}

    // Lerps the colour based on the value between faded and danger
	public void SetDangerState(float state)
	{
		resourceDot.color = Color.Lerp(faded, danger, state);
	}

    // Sets the colour state
	public void SetState(float currentValue, float minValue, float maxValue)
	{
		if (currentValue > minValue && currentValue < maxValue)
		{
            // Gets lerp value from the chunk
			float state = Mathf.InverseLerp(minValue, maxValue, currentValue);
			SetState(state);
		}
		else if (currentValue > maxValue)
		{
			SetState(1);
		}
		else if (currentValue < maxValue)
		{
			SetState(0);
		}
	}
}
