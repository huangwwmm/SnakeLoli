using UnityEngine;
using UnityEngine.EventSystems;

public class snEventSystemBaseInput : BaseInput
{
	public override Vector2 mousePosition
	{
		get
		{
			return Vector2.zero;
		}
	}
}