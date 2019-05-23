using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class Timer : MonoBehaviour
{
	public Image timerUI;
	public float pulseSpeed = 4f;
	public float pulseThreshold = 15f;
	public string timeOutEvent = "event:/CLOCK/15SecRemain";

	private static Image TimerUI;
	private static float TimeLeft;
	private static float MaxTime;
	private static bool TimerStarted;
	private static float pulseState; // Lerp state of timer so it's not instantly switching between two colours
	private int pulseDir = 1; // The direction of the pulse state so it goes up and down as needed
	private static StudioEventEmitter timeOutEmitter;
	private static bool eventIsPlaying;

	private void Awake()
	{
		// Make timer static so it can easily be accessed by static methods
		TimerUI = timerUI;

		// Set up the FMOD sound emitter
		timeOutEmitter = gameObject.AddComponent<StudioEventEmitter>();
		timeOutEmitter.Event = timeOutEvent;
		timeOutEmitter.StopEvent = EmitterGameEvent.ObjectDestroy;
	}

	// Use for initializing
	public static void SetTime(float time)
	{
		TimeLeft = time; // Set time left
		MaxTime = time; // Set total time
		TimerUI.color = Color.white;
		TimerUI.fillAmount = 1;
		eventIsPlaying = false;
	}

	// Update the timer and UI
	private void Update()
	{
		if (!TimerStarted) return;
	if (TimeLeft > 0)
	{
	    TimeLeft -= Time.deltaTime; // Reduce time

			// Update UI
			if(timerUI != null)
				timerUI.fillAmount = TimeLeft / MaxTime;

			// If time is less than the required threshold we start pulsing red
			if (TimeLeft < pulseThreshold)
			{
		pulseState += Time.deltaTime * pulseDir * pulseSpeed;

		// Simple switch to change the direction of lerp
		if (pulseState > 1) pulseDir = -1;
				if (pulseState < 0) pulseDir = 1;
				TimerUI.color = Color.Lerp(Color.white, Color.red, pulseState); // Lerp between white and red
				if (!eventIsPlaying)
				{
					timeOutEmitter.Play();
					eventIsPlaying = true;
				}
			}
	}
		else // Start game over scene
		{
			TimerStarted = false;
	    GameMenu.LoadGameOverScene();
			MusicManager.PlayGameOverTrack();
			timeOutEmitter.Stop();
		}
	}

	// Pauses the timer
	public static void Pause()
	{
		TimerStarted = false;
		timeOutEmitter.Stop();
	}

	// Resumes the timer
	public static void Resume()
	{
	TimerStarted = true;
	if(eventIsPlaying) timeOutEmitter.Play();
    }
}
