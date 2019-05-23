using UnityEngine;
/// <summary>
/// Used for creating a list of spawnable objects. These are simple prefabs.
/// The level generator generates a texture from the preview.
/// </summary>
[CreateAssetMenu(fileName = "ObstacleList", menuName = "LevelGeneration/ObstacleList")]
public class ObstacleList : ScriptableObject
{
	[Header("Used for consistency")]
	public GameObject Fire;
	public GameObject Dog;
	public GameObject Exit;
	[Space]
	public GameObject[] Obstacles;
}
