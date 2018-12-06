using UnityEngine;

public class Test : MonoBehaviour
{
	public hwmBox2D box;
	public hwmPerformanceStatistics Performance;
	public Vector2 sphere;
	public float radius;
	int Count = 0;

	private void OnDrawGizmos()
	{
		if (Performance == null)
		{
			Performance = new hwmPerformanceStatistics();
			Performance.Initialize();
		}

		box = hwmBox2D.BuildAABB(transform.position, new Vector2(6400f, 5120f));
		Quaternion rotation = transform.localRotation;
		Vector2 center = box.GetCenter();
		{
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(sphere, radius);

			Vector2 leftUp = new Vector2(box.Min.x, box.Max.y);
			Vector2 leftDown = box.Min;
			Vector2 rightUp = box.Max;
			Vector2 rightDown = new Vector2(box.Max.x, box.Min.y);

			Vector2 offset = center - hwmUtility.QuaternionMultiplyVector(rotation, center);
			leftUp = hwmUtility.QuaternionMultiplyVector(rotation, leftUp) + offset;
			leftDown = hwmUtility.QuaternionMultiplyVector(rotation, leftDown) + offset;
			rightUp = hwmUtility.QuaternionMultiplyVector(rotation, rightUp) + offset;
			rightDown = hwmUtility.QuaternionMultiplyVector(rotation, rightDown) + offset;

			Gizmos.DrawLine(leftDown, leftUp);
			Gizmos.DrawLine(leftUp, rightUp);
			Gizmos.DrawLine(rightUp, rightDown);
			Gizmos.DrawLine(rightDown, leftDown);

			Gizmos.color = Color.blue;
			hwmUtility.GizmosDrawBox2D(box);
		}
		{
			Vector2 sp = sphere;
			//Vector2 offset = center - hwmUtility.QuaternionMultiplyVector(Quaternion.Inverse(rotation), center); // 红色矩形绕原点旋转后，中心坐标的偏移
			//sp = hwmUtility.QuaternionMultiplyVector(Quaternion.Inverse(rotation), sp) // 黑色圆绕原点旋转后的坐标
			//	+ offset;
			sp = center + hwmUtility.QuaternionMultiplyVector(Quaternion.Inverse(rotation),(sp - center));
			Gizmos.color = box.IntersectSphere(sp, radius * radius) ? Color.red : Color.green;
			Gizmos.DrawSphere(sp, radius);
		}
		if (Count++ == 100)
		{
			Count = 0;
			Performance.LogAndRecord();
		}
	}
}