using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class ExtensionMethods
{

	public static bool IsInRangeIntB(this int target, int start, int end) {

		return target > start && target <= end;

	}

	public static bool IsInRangeIntA(this int target, int start, int end) {

		return target >= start && target < end;

	}

	public static bool IsInRangeFloatB(this float target, float start, float end) {

		return target > start && target <= end;

	}

	public static bool IsInRangeFloatA(this float target, float start, float end) {

		return target >= start && target < end;

	}

	public static T[] Append<T>(this T[] arrayInitial, T[] arrayToAppend)
	{
		if (arrayToAppend == null)
		{
			throw new ArgumentNullException("arrayToAppend");
		}
		if (arrayInitial is string || arrayToAppend is string)
		{
			throw new ArgumentException("The argument must be an enumerable");
		}
		T[] ret = new T[arrayInitial.Length + arrayToAppend.Length];
		arrayInitial.CopyTo(ret, 0);
		arrayToAppend.CopyTo(ret, arrayInitial.Length);

		return ret;
	}

	/// <summary>
	/// Normalized the angle. between -180 and 180 degrees
	/// </summary>
	/// <param Name="eulerAngle">Euler angle.</param>
	public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
	{
		Vector3 delta = eulerAngle;

		if (delta.x > 180) delta.x -= 360;
		else if (delta.x < -180) delta.x += 360;

		if (delta.y > 180) delta.y -= 360;
		else if (delta.y < -180) delta.y += 360;

		if (delta.z > 180) delta.z -= 360;
		else if (delta.z < -180) delta.z += 360;

		return new Vector3(delta.x, delta.y, delta.z);//round values to angle;
	}

	public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
	{
		return otherVector - vector;
	}
	public static void SetActiveChildren(this GameObject gameObjet, bool value)
	{
		foreach (Transform child in gameObjet.transform)
			child.gameObject.SetActive(value);
	}

	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;

		foreach (Transform child in obj.transform)
		{
			child.gameObject.SetLayerRecursively(layer);
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		do
		{
			if (angle < -360)
				angle += 360;
			if (angle > 360)
				angle -= 360;
		} while (angle < -360 || angle > 360);

		return Mathf.Clamp(angle, min, max);
	}

	public static ClipPlanePoints NearClipPlanePoints(this Camera camera, Vector3 pos, float clipPlaneMargin)
	{
		ClipPlanePoints clipPlanePoints = new ClipPlanePoints();

		Transform transform = camera.transform;
		float halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
		float aspect = camera.aspect;
		float distance = camera.nearClipPlane;
		float height = distance * Mathf.Tan(halfFOV);
		float width = height * aspect;
		height *= 1 + clipPlaneMargin;
		width *= 1 + clipPlaneMargin;
		clipPlanePoints.LowerRight = pos + transform.right * width;
		clipPlanePoints.LowerRight -= transform.up * height;
		clipPlanePoints.LowerRight += transform.forward * distance;

		clipPlanePoints.LowerLeft = pos - transform.right * width;
		clipPlanePoints.LowerLeft -= transform.up * height;
		clipPlanePoints.LowerLeft += transform.forward * distance;

		clipPlanePoints.UpperRight = pos + transform.right * width;
		clipPlanePoints.UpperRight += transform.up * height;
		clipPlanePoints.UpperRight += transform.forward * distance;

		clipPlanePoints.UpperLeft = pos - transform.right * width;
		clipPlanePoints.UpperLeft += transform.up * height;
		clipPlanePoints.UpperLeft += transform.forward * distance;

		return clipPlanePoints;
	}
	public static HitBarPoints GetBoundPoint(this BoxCollider boxCollider, Transform torso, LayerMask mask)
	{
		HitBarPoints bp = new HitBarPoints();
		BoxPoint boxPoint = boxCollider.GetBoxPoint();
		Ray toTop = new Ray(boxPoint.top, boxPoint.top - torso.position);
		Ray toCenter = new Ray(torso.position, boxPoint.center - torso.position);
		Ray toBottom = new Ray(torso.position, boxPoint.bottom - torso.position);
		Debug.DrawRay(toTop.origin, toTop.direction, Color.red, 2);
		Debug.DrawRay(toCenter.origin, toCenter.direction, Color.green, 2);
		Debug.DrawRay(toBottom.origin, toBottom.direction, Color.blue, 2);
		RaycastHit hit;
		float dist = Vector3.Distance(torso.position, boxPoint.top);
		if (Physics.Raycast(toTop, out hit, dist, mask))
		{
			bp |= HitBarPoints.Top;
			Debug.Log(hit.transform.name);
		}
		dist = Vector3.Distance(torso.position, boxPoint.center);
		if (Physics.Raycast(toCenter, out hit, dist, mask))
		{
			bp |= HitBarPoints.Center;
			Debug.Log(hit.transform.name);
		}
		dist = Vector3.Distance(torso.position, boxPoint.bottom);
		if (Physics.Raycast(toBottom, out hit, dist, mask))
		{
			bp |= HitBarPoints.Bottom;
			Debug.Log(hit.transform.name);
		}

		return bp;
	}
	public static BoxPoint GetBoxPoint(this BoxCollider boxCollider)
	{
		BoxPoint bp = new BoxPoint();
		bp.center = boxCollider.transform.TransformPoint(boxCollider.center);
		float height = boxCollider.transform.lossyScale.y * boxCollider.size.y;
		Ray ray = new Ray(bp.center, boxCollider.transform.up);

		bp.top = ray.GetPoint((height * 0.5f));
		bp.bottom = ray.GetPoint(-(height * 0.5f));

		return bp;
	}
	public static Vector3 BoxSize(this BoxCollider boxCollider)
	{
		float length = boxCollider.transform.lossyScale.x * boxCollider.size.x;
		float width = boxCollider.transform.lossyScale.z * boxCollider.size.z;
		float height = boxCollider.transform.lossyScale.y * boxCollider.size.y;
		return new Vector3(length, height, width);
	}
	public static bool Contains(this Enum keys, Enum flag)
	{
		if (keys.GetType() != flag.GetType())
			throw new ArgumentException("Type Mismatch");
		return (Convert.ToUInt64(keys) & Convert.ToUInt64(flag)) != 0;
	}

	public static bool Contains(this List<GeneratorUsage> usage, PowerGenerator generator)
	{
		return usage.Any(us => us.generator == generator);
	}

	public static void RemoveAllUsage(this List<GeneratorUsage> usage, PowerGenerator generator)
	{
		List<GeneratorUsage> newUsage = usage.Where(gu => gu.generator == generator).ToList();

		if (newUsage.Count > 0)
		{
			foreach (GeneratorUsage nu in newUsage)
			{
				usage.Remove(nu);
			}
		}
	}
}

public struct BoxPoint
{
	public Vector3 top;
	public Vector3 center;
	public Vector3 bottom;
}

public struct ClipPlanePoints
{
	public Vector3 UpperLeft;
	public Vector3 UpperRight;
	public Vector3 LowerLeft;
	public Vector3 LowerRight;
}

[Flags]
public enum HitBarPoints
{
	None = 0,
	Top = 1,
	Center = 2,
	Bottom = 4
}
