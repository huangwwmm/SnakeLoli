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

		float angleOffset = Vector2.SignedAngle(to, from);
		return Quaternion.Euler(0
				, 0
				, angleOffset > 0
					? -Mathf.Min(angleOffset, angle)
					: Mathf.Min(-angleOffset, angle)
			) * from;
	}

	public static void GizmosDrawBox(hwmBox2D box, float z = 0)
	{
#if UNITY_EDITOR
		Vector3 leftDown = box.Min;
		leftDown.z = z;
		Vector3 rightUp = box.Max;
		rightUp.z = z;
		Vector3 leftUp = new Vector3(leftDown.x, rightUp.y, z);
		Vector3 rightDown = new Vector3(rightUp.x, leftDown.y, z);

		Gizmos.DrawLine(leftDown, leftUp);
		Gizmos.DrawLine(leftUp, rightUp);
		Gizmos.DrawLine(rightUp, rightDown);
		Gizmos.DrawLine(rightDown, leftDown);
#endif
	}

	/// <summary>
	/// Gets the reciprocal of this vector, avoiding division by zero.
	/// Zero components are set to float.MaxValue.
	/// </summary>
	/// <param name="vec"></param>
	/// <returns>Reciprocal of this vector.</returns>
	public static Vector2 Reciprocal(Vector2 vec)
	{
		return new Vector2(vec.x != 0.0f ? 1.0f / vec.x : float.MaxValue
			, vec.y != 0.0f ? 1.0f / vec.y : float.MaxValue);
	}
}