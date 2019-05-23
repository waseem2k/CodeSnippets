using UnityEngine;
using Pathfinding;
using System;
using VikingCrewTools;
using Random = UnityEngine.Random;

[Serializable]
public class PathfindingAgent
{

/// <summary>
/// The responsibility of this class is to handle all pathfinding for a character. This class will be used by
/// AIControl which will be making the actual decisions of where to go and what to do
/// </summary>

	//TODO: Encapsulate these so that only those that need to are public
	private int currentWaypoint = 0;
	private Path path = null;
	private Seeker pathSeeker;

	#region Settings
	[Header("Settings"), SerializeField, Tooltip("If the characters devoid more than this then path will be replanned")]
	private float maxDevoidFromPath = 5;
	[Tooltip("This is the distance in meters the AI will try to get within of its next waypoint position to consider having reached it and proceeding to the next one")]
	public float nextWaypointDistance = 1;
	#endregion

	#region Astar node settings
	public int walkableNodes = 1;
	public int jumpableNodes = 2;
	#endregion

	#region Movement variables
	[Header("Movement variables")]
	public Vector2 lastPos;
	[SerializeField]
	public float movedDistanceTowardWaypoint = 0;
	public bool hasRequestedPath = false;
	public Vector3 targetPosWhenRequesting;
	#endregion

	private Rigidbody2D body;
	private Transform transform;
	private GenericEnemy ai;

	#region Properties
	protected Vector2 position { get { return transform.position; } }
	protected Vector2 velocity { get { return body.velocity; } }
	#endregion

	public void Setup(GenericEnemy controls)
	{
		this.ai = controls;
		this.pathSeeker = controls.GetComponent<Seeker>();
		this.transform = controls.transform;
		this.body = controls.GetComponent<Rigidbody2D>();
	}

	public bool HasPath()
	{
		return path != null;
	}

	/// <summary>
	/// Call this whenever the path we are on is leading to the wrong place or is in any other way
	/// considered not valid anymore. A new path will be requested at a later time
	/// </summary>
	public void SetPathUnvalid()
	{
		path = null;
	}

	/// <summary>
	/// Determines if we are on the correct path towards our next waypoint by checking 
	/// if we are getting closer to it. If we are consistently failing to do so it will request a new path
	/// as we may have fallen into a hole or something
	/// 
	/// </summary>
	public bool IsOnPath()
	{
		if (path == null) return false;
		float oldDist = (GetCurrentWaypoint() - lastPos).magnitude;
		float newDist = (GetCurrentWaypoint() - position).magnitude;
		float improvement = oldDist - newDist;
		movedDistanceTowardWaypoint += improvement;
		movedDistanceTowardWaypoint -= ai.minGetCloserPerSecond / ai.checksPerSecond;
		if (!hasRequestedPath && movedDistanceTowardWaypoint < -maxDevoidFromPath)
		{
			if (ai.debugPathfinding)
				Debug.Log(ai.name + ": Progress was too slow so I requested a new path");
			return false;
		}
		lastPos = position;
		return true;
	}

	public bool IsWaypointWalkable()
	{
		if (path == null || currentWaypoint >= path.path.Count) return false;
		return path.path[currentWaypoint].Tag == walkableNodes;
	}

	public bool IsWaypointJumpable()
	{
		if (path == null || currentWaypoint >= path.path.Count) return false;
		return path.path[currentWaypoint].Tag == jumpableNodes;
	}

	/// <summary>
	/// Requests a path to the closest node to the target position
	/// </summary>
	/// <param name="enemy"></param>
	public void RequestPath(Transform target)
	{
		targetPosWhenRequesting = target.position;
		RequestPath(targetPosWhenRequesting);
	}

	/// <summary>
	/// Requests a path to the closest node to the enemy position
	/// </summary>
	/// <param name="enemy"></param>
	public void RequestPath(Rigidbody2D enemy)
	{
		targetPosWhenRequesting = enemy.position;
		RequestPath(targetPosWhenRequesting);
	}

	/// <summary>
	/// Requests a path to the node closest to the target position
	/// </summary>
	/// <param name="position"></param>
	public void RequestPath(Vector3 position)
	{
		hasRequestedPath = true;
		//NNInfo nearest = AstarPath.active.GetNearest(position);
		//Vector3 closest = nearest.clampedPosition;
		pathSeeker.StartPath(transform.position, position, PathCalculatedCallback);
	}

	public void RequestRandomPath(Rect insideArea)
	{
		Vector2 pos = new Vector2(Random.Range(insideArea.xMin, insideArea.xMax), Random.Range(insideArea.yMin, insideArea.yMax));
		targetPosWhenRequesting = AstarPath.active.GetNearest(pos).clampedPosition;
		RequestPath(targetPosWhenRequesting);
	}

	public Vector2 GetCurrentWaypoint()
	{
		return path.vectorPath[currentWaypoint];
	}

	public bool IsOnLastWaypointInPath()
	{
		return currentWaypoint >= path.vectorPath.Count - 1;
	}

	/// <summary>
	/// Checks if the next waypoint is positioned on the ground and can thus be walked on
	/// </summary>
	/// <param name="terrainLayers"></param>
	/// <returns></returns>
	public bool IsWaypointOnGround(LayerMask terrainLayers)
	{
		bool isOnGround;


		isOnGround = Physics2D.Raycast(path.vectorPath[currentWaypoint], Vector3.down, 1.5f, terrainLayers).collider != null;


		if (ai.showDetectors)
		{
			DebugDrawPhysics.DebugDrawCircle(path.vectorPath[currentWaypoint], 0.25f, isOnGround ? Color.green : Color.red, 1f / ai.checksPerSecond);
		}

		return isOnGround;
	}

	void PathCalculatedCallback(Path p)
	{
		hasRequestedPath = false;
		movedDistanceTowardWaypoint = 0;
		if (!p.error)
		{
			if (ai.debugPathfinding)
				Debug.Log(ai.name + " found path");
			path = p;
			//Reset the waypoint counter
			currentWaypoint = 0;
		}
		else
		{
			path = null;
			//If we could not find path directly to enemy then we will settle for the closest
			if (ai.debugPathfinding)
				Debug.Log(ai.name + " could not find path " + p.ToString());
		}
	}

	/// <summary>
	/// If close enough to current waypoint then we select the next waypoint
	/// 
	/// Returns true if we now reached the end of the path
	/// </summary>
	public bool SelectNextWaypointIfCloseEnough()
	{
		if (GetDistanceToCurrentWaypoint() < nextWaypointDistance)
		{

			if (currentWaypoint >= path.vectorPath.Count - 1)
			{
				//The path has been used up as index has now been increased too higgh
				SetPathUnvalid();
				return true;
			}
			else
			{//Proceed to next waypoint
				currentWaypoint++;
				movedDistanceTowardWaypoint = 0;
			}
		}
		return false;
	}

	/// <summary>
	/// Will find next waypoint that is on the ground. If the last one is in the air then we should jump to it
	/// so in that case it will keep the last one.
	/// </summary>
	public void FindNextVisibleGroundedWaypoint(LayerMask terrainLayers)
	{
		while (currentWaypoint < path.vectorPath.Count - 1 && //Only step past if it is not the last waypoint
			IsWaypointJumpable() && //Only step past if waypoint is jumpable, because these are sometimes easy to miss, resulting in the character backtracking
			GetDistanceToCurrentWaypoint() < 5 && // Is within reasonable distance
			CanSeeNextWaypoint()) //AND if the waypoint after that is visible. 
		{
			currentWaypoint++;
			movedDistanceTowardWaypoint = 0;
		}
	}

	public bool CanSeeWaypoint(int index)
	{
		if (path == null) return false;
		return ai.CheckLineOfSight(ai.eyes.position, path.vectorPath[index]);
	}

	public bool CanSeeWaypoint()
	{
		return CanSeeWaypoint(currentWaypoint);
	}

	public bool CanSeeNextWaypoint()
	{
		return CanSeeWaypoint(currentWaypoint + 1);
	}

	public float GetDistanceToCurrentWaypoint()
	{
		if (HasPath())
		{
			return (GetCurrentWaypoint() - position).magnitude;
		}
		else
		{
			return float.PositiveInfinity;
		}
	}
}

