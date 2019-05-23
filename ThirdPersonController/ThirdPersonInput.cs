using UnityEngine;

public class ThirdPersonInput : MonoBehaviour
{
	[Header("Default Inputs")]
	public string horizontalInput = "Horizontal";
	public string verticalInput = "Vertical";
	public KeyCode jumpInputKey = KeyCode.Space;
	public KeyCode runInputKey = KeyCode.LeftShift;

	[Header("Camera Settings")]
	public string rotateCameraXInput = "Mouse X";
	public string rotateCameraYInput = "Mouse Y";

	private static ThirdPersonCamera Cam {get {return ThirdPersonCamera.CamInstance;}}
	protected ThirdPersonController cc;

	private void Start()
	{
		CharacterInit();
		InitializeCamera();
	}

	private void CharacterInit()
	{
		cc = GetComponent<ThirdPersonController>();
		if (cc != null) cc.Init();

		if (Cam != null) Cam.SetTarget(this.transform);

		//Cursor.visible = false;
		//Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		cc.UpdateMotor(); // Update motor
		cc.UpdateAnimator();// Update animations
	}

	private void LateUpdate()
	{
		if (cc == null) return; // Returns if we didn't find the controller
		InputHandle(); // update input methods
	}

	private void FixedUpdate()
	{
		cc.AirControl();
		CameraInput();
	}

	private void InputHandle()
	{
		CameraInput();

		if (!cc.LockMovement)
		{
			MoveCharacter();
			RunInput();
			JumpInput();
		}
	}

	private void MoveCharacter()
	{
		Vector2 input = new Vector2(Input.GetAxis(horizontalInput), Input.GetAxis(verticalInput));
		cc.Input = input;
		cc.UpdateLerpState();
	}

	private void RunInput()
	{
		if (Input.GetKeyDown(runInputKey))
			cc.Run(true);
		else if (Input.GetKeyUp(runInputKey))
			cc.Run(false);
	}

	private void JumpInput()
	{
		if (Input.GetKeyDown(jumpInputKey))
			cc.StartJump();
			//cc.Jump();
	}

	private void CameraInput()
	{
		if (Cam == null)
			return;
		float Y = Input.GetAxis(rotateCameraYInput);
		float X = Input.GetAxis(rotateCameraXInput);

		Cam.RotateCamera(X, Y);

		cc.UpdateTargetDirection(Cam != null ? Cam.transform : null);
		// Rotate the character with the camera
		RotateWithCamera(Cam != null ? Cam.transform : null);
	}

	private void InitializeCamera()
	{
		if (Cam == null) return;
		Cam.SetTarget(transform);
		Cam.Init();
	}

	private void RotateWithCamera(Transform cameraTransform)
	{
		if (!cc.LockMovement)
		{
			cc.RotateWithAnotherTransform(cameraTransform);
		}
	}
}
