using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script for adding volume controls to the menu
/// Using FMOD
/// </summary>
public class AudioSettings : MonoBehaviour
{
	private FMOD.Studio.EventInstance SFXVolumeEvent;
	private FMOD.Studio.EventInstance UIVolumeEvent;

	private FMOD.Studio.Bus Master;
	private FMOD.Studio.Bus Music;
	private FMOD.Studio.Bus SFX;
	private FMOD.Studio.Bus UI;

	public float MasterVolume = 1f;
	public float MusicVolume = 1f;
	public float SFXVolume = 1f;
	public float UIVolume = 1f;

	private void Awake() // Init
	{
		Master = FMODUnity.RuntimeManager.GetBus("bus:/Master");
		Music = FMODUnity.RuntimeManager.GetBus("bus:/Master/MX");
		SFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
		UI = FMODUnity.RuntimeManager.GetBus("bus:/Master/UI");

		SFXVolumeEvent = FMODUnity.RuntimeManager.CreateInstance("event:/DOG/DOG_Jump");
		UIVolumeEvent = FMODUnity.RuntimeManager.CreateInstance("event:/MENU/Hover");
	}

	private void Update()
	{
		Master.setVolume(MasterVolume);
		Music.setVolume(MusicVolume);
		SFX.setVolume(SFXVolume);
		UI.setVolume(UIVolume);
	}

	public void MasterVolumeLevel(Slider slider) // Master slider to control all volume
	{
		MasterVolume = slider.value;
	}

	public void MusicVolumeLevel(Slider slider) // Slider for Music volume
	{
		MusicVolume = slider.value;
	}

	public void SFXVolumeLevel(Slider slider) // Slider for generic SFX
	{
		SFXVolume = slider.value;

		// Play a sound to show the difference
		FMOD.Studio.PLAYBACK_STATE PBState;
		SFXVolumeEvent.getPlaybackState(out PBState);
		if (PBState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
		{
			SFXVolumeEvent.start();
		}
	}

	public void UIVolumeLevel(Slider slider) // Slider for UI SFX
	{
		UIVolume = slider.value;

		// Play a sound to show the difference
		FMOD.Studio.PLAYBACK_STATE PBState;
		UIVolumeEvent.getPlaybackState(out PBState);
		if (PBState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
		{
			UIVolumeEvent.start();
		}
	}
}
