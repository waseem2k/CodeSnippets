using FMODUnity;
using UnityEngine;

/// <summary>
/// Toggle between different tracks for Menu, Game and GameOver screen
/// We made this a singleton so the music can play seamlessly between scenes
/// </summary>
public class MusicManager : MonoBehaviour
{
	private static MusicManager instance;
	public string menuTrack;
	public string gameTrack;
	public string gameOverTrack;
	private static StudioEventEmitter menuEmitter, gameEmitter, gameOverEmitter;
	private static bool menuMusicPlaying, gameMusicPlaying, gameOverMusicPlaying;

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
		Init();
	}

    // Set up sound emiiters
	private void Init()
	{
		menuEmitter = gameObject.AddComponent<StudioEventEmitter>();
		menuEmitter.Event = menuTrack;
		gameEmitter = gameObject.AddComponent<StudioEventEmitter>();
		gameEmitter.Event = gameTrack;
		gameOverEmitter = gameObject.AddComponent<StudioEventEmitter>();
		gameOverEmitter.Event = gameOverTrack;
	}

	// Stops all music
	public static void StopAll()
	{
		menuEmitter.Stop();
		menuMusicPlaying = false;
		gameEmitter.Stop();
		gameMusicPlaying = false;
		gameOverEmitter.Stop();
		gameOverMusicPlaying = false;
	}

	// Play the menu track
	public static void PlayMenuTrack()
	{
		if (menuMusicPlaying) return;
		StopAll();
		menuEmitter.Play();
		menuMusicPlaying = true;
	}

	// Play the Game Track
	public static void PlayGameTrack()
	{
		if (gameMusicPlaying) return;
		StopAll();
		gameEmitter.Play();
		gameMusicPlaying = true;
	}

	// Play GameOver Track
	public static void PlayGameOverTrack()
	{
		if (gameOverMusicPlaying) return;
		StopAll();
		gameOverEmitter.Play();
		gameOverMusicPlaying = true;
	}
}
