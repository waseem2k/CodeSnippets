using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor to show position and rotation of the scene view camera
/// </summary>
public class SceneCameraStats : EditorWindow
{
	[MenuItem("Window/Romp/SceneViewStats")]
	public static void ShowWindow()
	{
		GetWindow<SceneCameraStats>("Scene Stats");
	}
	private static Camera Camera;
	private static Vector3 CamPos;
	private static Vector3 CamRot;


	private void OnEnable()
	{
		SceneView.onSceneGUIDelegate += DrawStats;
	}

	private void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= DrawStats;
	}

	private void DrawStats(SceneView scene)
	{
		Camera = scene.camera;
		Repaint();
	}

	private void OnGUI()
	{
		if (Camera == null) return;

		CamPos = new Vector3(Camera.transform.position.x, Camera.transform.position.y, Camera.transform.position.z);
		CamRot = new Vector3(Camera.transform.eulerAngles.x, Camera.transform.eulerAngles.y, Camera.transform.eulerAngles.z);

		string PositionResult = "Position - X: " + CamPos.x + " Y: " + CamPos.y + " Z: " + CamPos.z;
		string RotationResult = "Rotation - X: " + CamRot.x + " Y: " + CamRot.y + " Z: " + CamRot.z;
		GUILayout.Label(PositionResult);
		GUILayout.Label(RotationResult);
	}
}
