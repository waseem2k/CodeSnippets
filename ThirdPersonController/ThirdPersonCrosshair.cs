using UnityEngine;

public class ThirdPersonCrosshair : MonoBehaviour
{
	/// <summary>
	/// Use this for detecting interactable objects in the world
	/// </summary>
	public Transform crosshair; // The position of the crosshair being set
	public Transform eyes; // The position we start all raycasts
	public Transform Cam { get { return ThirdPersonCamera.Cam.transform; } }
	public float distance = 5; // The max distance from camera
	public CharacterInteraction interactions;
	private Interactable LastInteractable {get { return interactions.Interactable; } set { interactions.Interactable = value; }}
	public static Vector3 RayPoint;
	public LayerMask rayLayers;
	public Transform sphere;

	private void LateUpdate()
	{
		UpdateCrosshair();
	}

	/// <summary>
	/// Set the position of the crosshair by checking for hit
	/// </summary>
	private void UpdateCrosshair()
	{
		Ray camRay = new Ray(eyes.position, Cam.forward);
		RaycastHit hit;

		if (Physics.Raycast(camRay, out hit, distance, rayLayers))
		{
			RayPoint = hit.point;
			Interactable interactable = hit.collider.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.ShowDescription();
				LastInteractable = interactable;
			}
			else
			{
				RemoveLastInteractable();
			}
		}
		else
		{
			RayPoint = camRay.GetPoint(distance);
			RemoveLastInteractable();
		}

		crosshair.position = RayPoint;
		sphere.position = RayPoint;
	}

	// Call this whenever we don't hit anything
	private void RemoveLastInteractable()
	{
		if (LastInteractable != null)
		{
			LastInteractable.HideDescription();
			LastInteractable = null;
		}
	}
}
