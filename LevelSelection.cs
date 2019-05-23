using UnityEngine;
/// <summary>
/// Was used to set up the level selection menu.
/// Since we had so many levels, we had to split the menu into separate pages
/// This script helps set up each button and animates the pages so the slide on and off the screen
/// </summary>
public class LevelSelection : MonoBehaviour
{
	// Set up singleton behaviour
	private static LevelSelection Instance;

    private void Awake()
	{
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
	}

	public LevelFrame[] frames; // List of all the level frames, contains a list of level buttons

	[Header("Frame Movement")]
	public Transform leftPos; // The left off screen position
	public Transform centerPos; // The center of the screen
	public Transform rightPos; // The right off screen position

	public float frameMoveSpeed; // The speed at which the frames move when changing frames

	[Header("Selection Arrows")]
	public NavigationButton leftArrow; // Left nav arrow
	public NavigationButton rightArrow; // Right nav arrow

	private int frameIndex; // The index of the active frame
	public static int UnlockedLevelsCountCount; // The number of levels unlocked, get from save manager and save to manager when updated

	private Vector3 fp;   //First touch position
	private Vector3 lp;   //Last touch position
	private float dragDistance;  //minimum distance for a swipe to be registered

	public static bool LevelSelectActive = false; // If the level select scene and panel is active, so we're not calling events for no reason

    private void Start() // Init
	{
		dragDistance = Screen.height * 15f / 100f; //dragDistance is 15% height of the screen
		frameIndex = 0;
		SaveManager.LoadSaveData();

		SetupFrames();
		SetupButtons();
		UpdateArrows();
    }

	private void Update()
	{
		if (!LevelSelectActive) return;

		// Get swipe direction to switch frames
		if(GetSwipeDirection() == Vector3.left) MoveFramesLeft();
		if (GetSwipeDirection() == Vector3.right) MoveFramesRight();
	}

	public void MoveFramesLeft() // Starts moving the frames, move last frame off and new from onto the screen
	{
		frames[frameIndex].MoveToPosition(leftPos.position, frameMoveSpeed, false);
		frameIndex++;
		frames[frameIndex].MoveToPosition(centerPos.position, frameMoveSpeed, true);
		UpdateArrows();
	}

	public void MoveFramesRight() // Starts moving the frames in the other direction
	{
		frames[frameIndex].MoveToPosition(rightPos.position, frameMoveSpeed, false);
		frameIndex--;
		frames[frameIndex].MoveToPosition(centerPos.position, frameMoveSpeed, true);
		UpdateArrows();
	}

	//Initializes buttons, check how many levels are unlocked and toggle the level buttons on or off
	private void SetupButtons()
	{
		int index = 1;
		int activeFrame = 0;
		bool encounteredFirstLock = false;

		foreach (LevelFrame f in frames)
		{
			foreach (LevelButton t in f.levels)
			{
				t.SetLevelIndex(index);
				if (index == UnlockedLevelsCountCount)
				{
					encounteredFirstLock = true;
				}
				if (index <= UnlockedLevelsCountCount)
				{
					t.UnlockLevel();
				}

				index++;
			}
			if(!encounteredFirstLock) activeFrame++;
		}
		GameMenu.SetMaxLevels(index - 1);

		SetActiveFrame(activeFrame);
	}

	// Sets up frame positions
	private void SetupFrames()
	{
		for (int i = 0; i < frames.Length; i++)
		{
			frames[i].SetPosition(i == 0 ? centerPos.position : rightPos.position, false);
		}
	}

	// Set which frame is active
	private void SetActiveFrame(int activeFrame)
	{
		for (int i = 0; i < activeFrame; i++)
		{
			MoveFramesLeft();
		}
		if (activeFrame == 0) frames[0].ToggleButtons(true);
	}

	// Checks if we have met the requirement to unlock a new level
	public static void CheckForLevelUnlock(int index)
	{
		if (index == UnlockedLevelsCountCount)
		{
			Instance.UnlockNextLevel();
			SaveManager.SaveData();
		}
	}

	// Unlocks the next level
	private void UnlockNextLevel()
	{
		UnlockedLevelsCountCount++;

		int index = 1;
		foreach (LevelFrame f in frames)
		{
			foreach (LevelButton t in f.levels)
			{
				if (index > UnlockedLevelsCountCount) return;
				t.UnlockLevel();
				SetActiveFrame(t.FrameIndex);
				index++;
			}
		}
	}

	// Toggles navigation arrows based on which frame we are looking at, Left most frame will disable the left arrow, right most for right
	private void UpdateArrows()
	{
		leftArrow.SetEnabled(frameIndex != 0);
		rightArrow.SetEnabled(frameIndex != frames.Length - 1);
	}

	// Detects a mobile swipe for level select frames
	// Also checks which frame we are on so we aren't able to swipe the frames if far left or far right frame is active
	private Vector3 GetSwipeDirection()
	{
		if (Input.touchCount == 1) // user is touching the screen with a single touch
		{
			Touch touch = Input.GetTouch(0); // get the touch
			if (touch.phase == TouchPhase.Began) //check for the first touch
			{
				fp = touch.position;
				lp = touch.position;
			}
			else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
			{
				lp = touch.position;

				//Check if drag distance is greater than a percentage of the screen size
				if (Mathf.Abs(lp.x - fp.x) > dragDistance )
				{
					if (lp.x > fp.x && frameIndex != 0) return Vector3.right; // Checking to make sure we are not on the left most frame
					if (lp.x < fp.x && frameIndex != frames.Length - 1) return Vector3.left; // Check to make sure we are not on the right most frame

					return Vector3.zero;
				}
			}
		}
		return Vector3.zero;
	}
}
