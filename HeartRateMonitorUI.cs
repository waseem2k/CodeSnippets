using System.Collections;
using geniikw.DataRenderer2D;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Custom Heart Rate monitor script. Draws a blip based on the heart rate
/// This is made to work with a line renderer that works on the UI.
/// Since the default Line Renderer doesn't work on the UI,
/// We had to use a custom one from the asset store,
/// however it's heavy on the CPU
/// </summary>
public class HeartRateMonitorUI : MonoBehaviour
{
	[Header("Setup")]
	public RectTransform blip;
	public UILine lineOne;
	public UILine lineTwo;
	public Text bpmText;

	[Header("Display")]
	public float monitorWidth = 500f; // The actual width of the entire monitor control.
	public float monitorHeight = 200f; // The actual height of the entire monitor control.
	public float lineSpeed = 0.3f; // Speed the blip moves
	public float lineWidth = 0.5f; // The width of the lines being drawn
	public float blipSize = 10f; // The size of the blip
	private RectTransform MyRect {get { return (RectTransform) transform; }} // The Rect transform of this UI
	private float HalfHeight { get { return MyRect.sizeDelta.y * 0.5f; } } // Half the height of the Rect


	[Header("Warnings")]
	public Image background;
	public float pulseSpeed = 0.5f;
	public float lowDangerLimit = 100f;
	public float highDangerLimit = 160f;
	private float pulseState;
	private int pulseDirection = 1;

	private Color standardColour;
	private Color dangerColour = new Color(1,0,0,0.45f);
	private Color moderateColor = new Color(1,0.5f,0,0.45f);

	private float beatsPerMinute = 90; // Beats per minute.
	private float BeatsPerSecond {get { return 60f / beatsPerMinute; }} // BPM converted to seconds
	private bool flatLine; // Display flatline if true

	private bool lineSwitch; // False is line one, true is line two

	private Vector3 blipPos; // Current blip position
	private float blipLerpPosition; // Position of blip between startX and endX

	private float lastUpdate; // Time since last update
	private float StartX { get { return MyRect.sizeDelta.x * -.5f; }} // Left position side of rect
	private float EndX { get { return MyRect.sizeDelta.x * 0.5f; } } // Right position of rect
	private float StartWorldX { get { return transform.position.x + StartX; } } // World X position of start point
	private Vector3 startWorldPosition; // World position of startX
	private IEnumerator heartbeatRoutine; // Routine to perform heartbeat

	public void SetHeartRate(float amount) // Set the value directly
	{
		beatsPerMinute = amount;
	}

	public void SetFlatLine(bool state)
	{
		flatLine = state;
	}

	private void Start()
	{
		if (bpmText != null) bpmText.text = beatsPerMinute.ToString("F0") + " BPM";
		standardColour = background.color;
		standardColour.a = 0.45f;
		// Reset everything at the start to set it up correctly
		ResetPoint(blip, out blipPos);
		ResetLine(lineOne);
		ResetLine(lineTwo);
	}

	private void Update()
	{

		UpdateLineHorizontalPosition();
		UpdateHeartLine();
		ResetBlipPosition();
		CheckForBlip();
		DangerZone();
	}

	private void DangerZone()
	{
		if (beatsPerMinute < lowDangerLimit)
		{
			if (background.color != standardColour) background.color = standardColour;
			return;
		}

		if (beatsPerMinute < highDangerLimit)
		{
			pulseState += Time.deltaTime * pulseDirection * pulseSpeed;
			background.color = Color.Lerp(standardColour, moderateColor, pulseState);
			//bpmText.color = Color.Lerp(standardColour, moderateColor, pulseState);
		}
		else if (beatsPerMinute > highDangerLimit)
		{
			pulseState += Time.deltaTime * pulseDirection * pulseSpeed * 2;
			background.color = Color.Lerp(standardColour, dangerColour, pulseState);
			//bpmText.color = Color.Lerp(standardColour, dangerColour, pulseState);
		}

		if (pulseState > 1) pulseDirection = -1;
		if (pulseState < 0) pulseDirection = 1;
	}

	// Moves blip horizontally from left to right
	private void UpdateLineHorizontalPosition()
	{
		blipLerpPosition = Mathf.InverseLerp(StartX, EndX, blip.transform.localPosition.x);
		blipPos += new Vector3(MyRect.sizeDelta.x * lineSpeed * Time.deltaTime, 0, 0);
		blip.localPosition = blipPos;

	}

	// Updates UI Line to follow blip position
	private void UpdateHeartLine()
	{
		if (lineSwitch)
		{
			if (lineTwo.line.Count >= 2)
			{
				lineTwo.line.EditPoint(lineTwo.line.Count - 1, blip.position, lineWidth);
				lineOne.line.EditPoint(0, startWorldPosition, lineWidth);
				float t = blipLerpPosition + 0.1f;
				lineOne.line.option.startRatio = t;
				if (t > 0.95f)
				{
					ResetLine(lineOne);
					lineOne.line.option.startRatio = 0.05f;
				}
			}
		}
		else
		{
			if (lineOne.line.Count >= 2)
			{
				lineOne.line.EditPoint(lineOne.line.Count - 1, blip.position, lineWidth);
				lineTwo.line.EditPoint(0, startWorldPosition, lineWidth);
				float t = blipLerpPosition + 0.1f;
				lineTwo.line.option.startRatio = t;
				if (t > 0.95f)
				{
					ResetLine(lineTwo);
					lineTwo.line.option.startRatio = 0.05f;
				}
			}
		}
	}

	// Reset position of blip when it reaches the right limit
	private void ResetBlipPosition()
	{
		if (blip.localPosition.x > EndX)
		{
			// Stop routine so it doesn't update while resetting
			StopCoroutine(heartbeatRoutine);
			lineSwitch = !lineSwitch;
			ResetPoint(blip, out blipPos);
		}
	}

	// Perform blip to represent heart beat
	private void CheckForBlip() // Check to see if blip needs to be performed
	{
		if (beatsPerMinute <= 0 || flatLine)
		{
			lastUpdate = Time.time;
		}
		else if (Time.time - lastUpdate >= BeatsPerSecond)
		{
			lastUpdate = Time.time;
			heartbeatRoutine = PerformHeartbeat(lineSwitch ? lineTwo : lineOne);
			StartCoroutine(heartbeatRoutine);
		}
	}

	// Moves blip up and down in succession to represent a heartbeat
	private IEnumerator PerformHeartbeat(UILine line)
	{
		AddPoint(line, blip.position);
		yield return new WaitForSeconds(0.01f);
		float max = Random.Range(HalfHeight * 0.8f, HalfHeight);
		blip.localPosition = new Vector3(blip.localPosition.x, max, 0); // Top point
		AddPoint(line, blip.position);
		yield return new WaitForSeconds(0.03f);
		blip.localPosition = new Vector3(blip.localPosition.x, -max, 0); // Bottom point opposite of top point
		AddPoint(line, blip.position);
		yield return new WaitForSeconds(0.02f);

		float secondPulse = max * Random.Range(0.4f, 0.7f);
		blip.localPosition = new Vector3(blip.localPosition.x, secondPulse, 0);
		AddPoint(line, blip.position);
		yield return new WaitForSeconds(0.02f);
		blip.localPosition = new Vector3(blip.localPosition.x, secondPulse * 0.8f, 0);
		AddPoint(line, blip.position);
		yield return new WaitForSeconds(0.03f);

		blip.localPosition = new Vector3(blip.localPosition.x, 0, 0);
		AddPoint(line, blip.position);
		if (bpmText != null) bpmText.text = beatsPerMinute.ToString("F0") + " BPM";
	}

	// Adds a point where the blip currently is
	private void AddPoint(UILine line, Vector3 pos)
	{
		Vector3 myPos = new Vector3(pos.x, pos.y, pos.z);
		line.line.Push(myPos, myPos, Vector3.zero, lineWidth);
		line.line.EditPoint(line.line.Count-2, myPos, lineWidth);
	}

	// Reset the position of the blip
	private void ResetPoint(Transform point, out Vector3 pos)
	{
		pos = new Vector3(StartX, 0, 0);
		point.localPosition = pos;
		startWorldPosition = point.position;
	}

	// Resets a line so it's ready for the next pass
	private void ResetLine(UILine line)
	{
		line.line.Clear();
		Vector3 pos = new Vector3(StartWorldX, transform.position.y, 0);
		line.line.Push(pos, lineWidth);
		line.line.Push(pos, lineWidth);
	}

	// Update variables if the variables have changed
	private void OnValidate()
	{
		MyRect.sizeDelta = new Vector2(monitorWidth, monitorHeight);
		if(blip != null) blip.sizeDelta = new Vector2(blipSize, blipSize);
	}
}
