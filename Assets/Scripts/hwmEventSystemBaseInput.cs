using UnityEngine;
using UnityEngine.EventSystems;

public class hwmEventSystemBaseInput : BaseInput
{
	public override Vector2 mousePosition
	{
		get
		{
			return Vector2.zero;
		}
	}
}