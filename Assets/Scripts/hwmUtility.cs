using System;
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

	#region Gizmos
	public static void GizmosDrawBox2D(hwmBox2D box, float z = 0)
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

	public static void GizmosDrawRotateBox2D(hwmBox2D box, Quaternion rotation, float z = 0)
	{
#if UNITY_EDITOR
		Vector2 center = box.GetCenter();
		Vector2 leftUp = new Vector2(box.Min.x, box.Max.y);
		Vector2 leftDown = box.Min;
		Vector2 rightUp = box.Max;
		Vector2 rightDown = new Vector2(box.Max.x, box.Min.y);

		Vector2 offset = center - hwmMath.QuaternionMultiplyVector(rotation, center);
		leftUp = hwmMath.QuaternionMultiplyVector(rotation, leftUp) + offset;
		leftDown = hwmMath.QuaternionMultiplyVector(rotation, leftDown) + offset;
		rightUp = hwmMath.QuaternionMultiplyVector(rotation, rightUp) + offset;
		rightDown = hwmMath.QuaternionMultiplyVector(rotation, rightDown) + offset;

		Gizmos.DrawLine(leftDown, leftUp);
		Gizmos.DrawLine(leftUp, rightUp);
		Gizmos.DrawLine(rightUp, rightDown);
		Gizmos.DrawLine(rightDown, leftDown);
#endif
	}
	#endregion
}