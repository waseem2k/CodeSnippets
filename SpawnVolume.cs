using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Unreal Engine style Spawn Volume. It generates positions inside a bounding box
/// Positions are spaced out depending on the size of the box
/// The number of positions are also determined by the size of the box
/// </summary>
public class EnemySpawnVolume : MonoBehaviour
{
	private List<Vector3> spawnPoints = new List<Vector3>();
	public List<Vector3> SpawnPoints { get { return spawnPoints; } }

	[Header("Bounding Box Dimensions")]
	public float boxLength = 10f;
	public float boxWidth = 10f;
	public float boxHeight = 5f;
	private float HalfHeight { get { return boxHeight * 0.5f; } }

	[Header("Spawner Dimensions")]
	public float spawnerRadius = 0.5f;
	public float padding = 0.5f;
	private float HalfPadding { get { return padding * 0.5f; } }
	private float SpawnerDiameter { get { return spawnerRadius * 2; } }

	[Header("Gizmos")] // Colour of the drawn Gizmos
	public Color boundingBoxColour = Color.magenta;
	public Color spawnerColour = Color.green;

	private int xCount = 1; // The number of objects we can place along the length of the box
	private int zCount = 1; // The number of objects we can place along the Width of the box

	private float xOffset; // Offset position takes the Length into consideration and spaces out the objects evenly
	private float zOffset; // Same as xOffset, just works with the Width

	private Vector3 backLeftCorner; // The backleft corner is where the points start, this is to make it easier to do calculations

	private void Start()
	{
		UpdateSpawnPositions();
	}

	private void UpdateSpawnPositions()
	{
        // Get the average number of objects in the x/length direction
		float lengthDivide = boxLength / (SpawnerDiameter + padding);
		xCount = (int)Mathf.Abs(lengthDivide);

        // Get the average number of objects in the z/Width direction
		float widthDivide = boxWidth / (SpawnerDiameter + padding);
		zCount = (int)Mathf.Abs(widthDivide);

        // Calculate offsets so we can space evenly
		xOffset = boxLength / (xCount + HalfPadding);
		zOffset = boxWidth / (zCount + HalfPadding);

        // Find the back left corner, keeping in mind the spawner radius and padding
		backLeftCorner = new Vector3((transform.position.x - boxLength * 0.5f) + spawnerRadius + padding, transform.position.y + HalfHeight, (transform.position.z - boxWidth * 0.5f) + spawnerRadius + padding);

		spawnPoints = new List<Vector3>(); // Clear the list so we can fill it with new data

        // Loop for finding the positions, this should only need to be called once
		for (int xIndex = 0; xIndex < xCount; xIndex++)
		{
			for (int zIndex = 0; zIndex < zCount; zIndex++)
			{
				Vector3 newPos = new Vector3(backLeftCorner.x + (xOffset * xIndex), backLeftCorner.y, backLeftCorner.z + (zOffset * zIndex));
				spawnPoints.Add(newPos);
			}
		}
	}

    // Updates the points list if any variable changes
	private void OnValidate()
	{
		UpdateSpawnPositions();
	}

    // Draw Gizmos so we can visualize what we are doing
	private void OnDrawGizmos()
	{
		Gizmos.color = boundingBoxColour;
        Vector3 center = new Vector3(transform.position.x, transform.position.y + HalfHeight, transform.position.z); // Offset the center so the gameobjects pivot is on the bottom
		Vector3 size = new Vector3(boxLength, boxHeight, boxWidth); // Create size for bounding box

		Gizmos.DrawWireCube(center, size); // Draw the bounding box

		foreach (Vector3 point in spawnPoints)
		{
            // Custom Gizmos, allows us to draw a capsule
			DebugExtension.DrawCapsule(point, spawnerRadius, boxHeight, spawnerColour);
		}
	}
}
