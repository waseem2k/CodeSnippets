using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	public static ThirdPersonCamera CamInstance;
	public static Camera Cam;

	private Transform target;
	public float smoothCameraRotation = 12f;
	public LayerMask cullingLayer = 1 << 0; // Layers to cull

	public bool lockCamera; // Locks camera behind player

	public float rightOffset;
	public float defaultDistance = 2.5f;
	public float height = 1.4f;
	public float smoothFollow = 10f;
	public float xMouseSensitivity = 3f;
	public float yMouseSensitivity = 3f;
	public float yMinLimit = -40f;
	public float yMaxLimit = 80f;


	[HideInInspector] public int indexList, indexLookPoint;
	[HideInInspector] public float offSetPlayerPivot;
	[HideInInspector] public string currentStateName;
	[HideInInspector] public Transform currentTarget;
	[HideInInspector] public Vector2 movementSpeed;

	private Transform targetLookAt;
	private Vector3 currentTargetPos;
	private Vector3 current_cPos;
	private Vector3 desired_cPos;

	private float distance = 5f;
	private float mouseY;
	private float mouseX;
	private float currentHeight;
	private float cullingDistance;
	private const float checkHeightRadius = 0.4f;
	private const float clipPlaneMargin = 0f;
	private const float forward = -1f;
	private const float xMinLimit = -360f;
	private const float xMaxLimit = 360f;
	private float cullingHeight = 0.2f;
	private const float cullingMinDist = 0.1f;

	private void Awake()
	{
		CamInstance = this;
		Cam = GetComponent<Camera>();
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		if (target == null)
			return;


		currentTarget = target;
		currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

		targetLookAt = new GameObject("targetLookAt").transform;
		targetLookAt.position = currentTarget.position;
		targetLookAt.hideFlags = HideFlags.HideInHierarchy;
		targetLookAt.rotation = currentTarget.rotation;

		mouseY = currentTarget.eulerAngles.x;
		mouseX = currentTarget.eulerAngles.y;

		distance = defaultDistance;
		currentHeight = height;
	}

	private void FixedUpdate()
	{
		if (target == null || targetLookAt == null) return;

		CameraMovement();
	}

	// Set the target for this camera
	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
		currentTarget = newTarget;
		mouseY = currentTarget.rotation.eulerAngles.x;
		mouseX = currentTarget.rotation.eulerAngles.y;
		Init();
	}

	public Ray ScreenPointToRay(Vector3 Point)
	{
		return Cam.ScreenPointToRay(Point);
	}

	public void RotateCamera(float x, float y)
	{
		mouseX += x * xMouseSensitivity;
		mouseY -= y * yMouseSensitivity;

		movementSpeed.x = x;
		movementSpeed.y = -y;
		mouseY = !lockCamera ? ExtensionMethods.ClampAngle(mouseY, yMinLimit, yMaxLimit) : currentTarget.root.localEulerAngles.x;
		mouseX = ExtensionMethods.ClampAngle(mouseX, xMinLimit, xMaxLimit);
	}

	private void CameraMovement()
	{
		if (currentTarget == null)
			return;

		distance = Mathf.Lerp(distance, defaultDistance, smoothFollow * Time.deltaTime);

		cullingDistance = Mathf.Lerp(cullingDistance, distance, Time.deltaTime);
		Vector3 camDir = (forward * targetLookAt.forward) + (rightOffset * targetLookAt.right);

		camDir = camDir.normalized;

		Vector3 targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
		currentTargetPos = targetPos;
		desired_cPos = targetPos + new Vector3(0, height, 0);
		current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
		RaycastHit hitInfo;

		ClipPlanePoints planePoints = Cam.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
		ClipPlanePoints oldPoints = Cam.NearClipPlanePoints(desired_cPos + (camDir * distance), clipPlaneMargin);

		//Check if Height is not blocked
		if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer))
		{
			float t = hitInfo.distance - 0.2f;
			t -= height;
			t /= (cullingHeight - height);
			cullingHeight = Mathf.Lerp(height, cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
		}

		//Check if desired target position is not blocked
		if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, distance + 0.2f, cullingLayer))
		{
			distance = hitInfo.distance - 0.2f;
			if (distance < defaultDistance)
			{
				float t = hitInfo.distance;
				t -= cullingMinDist;
				t /= cullingMinDist;
				currentHeight = Mathf.Lerp(cullingHeight, height, Mathf.Clamp(t, 0.0f, 1.0f));
				current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
			}
		}
		else
		{
			currentHeight = height;
		}

		//Check if target position with culling height applied is not blocked
		if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer)) distance = Mathf.Clamp(cullingDistance, 0.0f, defaultDistance);
		Vector3 lookPoint = current_cPos + targetLookAt.forward * 2f;
		lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
		targetLookAt.position = current_cPos;

		Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
		targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);
		transform.position = current_cPos + (camDir * (distance));
		Quaternion rotation = Quaternion.LookRotation((lookPoint) - transform.position);

		transform.rotation = rotation;
		movementSpeed = Vector2.zero;
	}


	private bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float dist, LayerMask cullLayer)
	{
		bool value = false;

		if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, dist, cullLayer))
		{
			value = true;
			cullingDistance = hitInfo.distance;
		}

		if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, dist, cullLayer))
		{
			value = true;
			if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
		}

		if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, dist, cullLayer))
		{
			value = true;
			if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
		}

		if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, dist, cullLayer))
		{
			value = true;
			if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
		}

		return value;
	}
}
