using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Planetoria/Resource")]
public class Resource : ScriptableObject
{
	public ResourceType resourceType;

}

public enum ResourceType
{
	Coal,
	Copper,
	Iron
}