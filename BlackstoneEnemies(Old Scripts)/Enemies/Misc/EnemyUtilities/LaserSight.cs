using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour
{
	public float heightAdjustment = 0.6f;
	public LayerMask ignoreLayers;
	private LineRenderer lineRenderer;
	private Transform target; // Set by enemy script
	
	[HideInInspector] public bool drawLine; // Set by animator
	private bool updateLine;

	private bool overrideDefaults;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
	}

	public void SetTarget(Transform _target)
	{
		target = _target;
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, transform.position);
		updateLine = true;
	}

	public void SetTarget(Transform _target, bool overrideAnimator)
	{
		if (overrideAnimator)
			overrideDefaults = true;
			
		SetTarget(_target);
	}

	public void HideLine()
	{
		if(lineRenderer.positionCount > 0) lineRenderer.SetPosition(1, transform.position);
		target = null;
		lineRenderer.positionCount = 0;
		drawLine = false;
		lineRenderer.enabled = false;
		updateLine = false;
		overrideDefaults = false;
	}

	private void Update()
	{
	
		if(!lineRenderer.enabled && drawLine || !lineRenderer.enabled && overrideDefaults) lineRenderer.enabled = true;
		if (!updateLine) return;

		Vector2 tarPos = new Vector2(target.position.x, target.position.y + heightAdjustment);
		//Vector2 dir = target.position - transform.position;
		RaycastHit2D ray = Physics2D.Linecast(transform.position, tarPos, ~ignoreLayers);
		if(lineRenderer.positionCount > 1) lineRenderer.SetPosition(1, ray.point);

		if (!overrideDefaults) return;

		if (lineRenderer.positionCount > 0) lineRenderer.SetPosition(0, transform.position);
	}
}
