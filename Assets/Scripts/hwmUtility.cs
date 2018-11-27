using UnityEngine;

public static class hwmUtility
{
	public static T GetComponentByPath<T>(Transform parent, string path, bool ignoreLog = false) where T : Component
	{
		Transform child = parent.Find(path);
		if (child == null)
		{
			if (!ignoreLog)
			{
				Debug.LogError(string.Format("Fail to locate child by path: ({0})", path));
			}
			return null;
		}
		else
		{
			return child.GetComponent<T>();
		}
	}

	public static Vector2 CircleLerp(Vector2 from, Vector2 to, float angle)
	{
		if (from == to)
		{
			return to;
		}
		else if (angle < 0)
		{
			return from;
		}

		float angleOffset = Vector2.SignedAngle(to,from);
		return Quaternion.Euler(0
				, 0
				, angleOffset > 0 
					? -Mathf.Min(angleOffset, angle)
					: Mathf.Min(-angleOffset, angle)
			) * from;
	}

	public static void GizmosDrawBounds(hwmBounds2D bounds, float z = 0)
	{
		Vector3 leftDown = bounds.min;
		leftDown.z = z;
		Vector3 rightUp = bounds.max;
		rightUp.z = z;
		Vector3 leftUp = new Vector3(leftDown.x, rightUp.y, z);
		Vector3 rightDown = new Vector3(rightUp.x, leftDown.y, z);

		Gizmos.DrawLine(leftDown, leftUp);
		Gizmos.DrawLine(leftUp, rightUp);
		Gizmos.DrawLine(rightUp, rightDown);
		Gizmos.DrawLine(rightDown, leftDown);
	}
}