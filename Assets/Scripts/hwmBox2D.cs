using UnityEngine;

[System.Serializable]
public struct hwmBox2D
{
	public Vector2 Min;

	public Vector2 Max;

	/// <summary>
	/// Utility function to build an AABB from Origin and Extent 
	/// </summary>
	/// <param name="center">The location of the bounding box.</param>
	/// <param name="extent">Extent Half size of the bounding box.</param>
	/// <returns>A new axis-aligned bounding box.</returns>
	public static hwmBox2D BuildAABB(Vector2 center, Vector2 extent)
	{
		return new hwmBox2D(center - extent, center + extent);
	}

	public static bool operator ==(hwmBox2D lhs, hwmBox2D rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	public static bool operator !=(hwmBox2D lhs, hwmBox2D rhs)
	{
		return !(lhs == rhs);
	}

	public static hwmBox2D operator *(Matrix4x4 matrix, hwmBox2D box)
	{
		Vector2 center = box.GetCenter();
		Vector2 leftUp = new Vector2(box.Min.x, box.Max.y);
		Vector2 leftDown = box.Min;
		Vector2 rightUp = box.Max;
		Vector2 rightDown = new Vector2(box.Max.x, box.Min.y);

		Vector2 offset = center - hwmMath.MatrixMultiplyVector(matrix, center);
		leftUp = hwmMath.MatrixMultiplyVector(matrix, leftUp) + offset;
		leftDown = hwmMath.MatrixMultiplyVector(matrix, leftDown) + offset;
		rightUp = hwmMath.MatrixMultiplyVector(matrix, rightUp) + offset;
		rightDown = hwmMath.MatrixMultiplyVector(matrix, rightDown) + offset;

		return new hwmBox2D(new Vector2(hwmMath.Min(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
				, hwmMath.Min(leftUp.y, leftDown.y, rightUp.y, rightDown.y))
			, new Vector2(hwmMath.Max(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
				, hwmMath.Max(leftUp.y, leftDown.y, rightUp.y, rightDown.y)));
	}

	public static hwmBox2D operator *(Quaternion rotation, hwmBox2D box)
	{
		{
			//Vector2 center = box.GetCenter();
			//Vector2 leftUp = new Vector2(box.Min.x, box.Max.y);
			//Vector2 leftDown = box.Min;
			//Vector2 rightUp = box.Max;
			//Vector2 rightDown = new Vector2(box.Max.x, box.Min.y);

			//Vector2 offset = center - hwmMath.QuaternionMultiplyVector(rotation, center);
			//leftUp = hwmMath.QuaternionMultiplyVector(rotation, leftUp) + offset;
			//leftDown = hwmMath.QuaternionMultiplyVector(rotation, leftDown) + offset;
			//rightUp = hwmMath.QuaternionMultiplyVector(rotation, rightUp) + offset;
			//rightDown = hwmMath.QuaternionMultiplyVector(rotation, rightDown) + offset;

			//return new hwmBox2D(new Vector2(hwmMath.Min(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
			//		, hwmMath.Min(leftUp.y, leftDown.y, rightUp.y, rightDown.y))
			//	, new Vector2(hwmMath.Max(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
			//		, hwmMath.Max(leftUp.y, leftDown.y, rightUp.y, rightDown.y)));
		}

		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num6 = rotation.z * num3;
		float num7 = rotation.x * num2;
		float num12 = rotation.w * num3;

		float num21 = 1.0f - rotation.y * num2 - num6;
		float num22 = num7 - num12;
		float num23 = num7 + num12;
		float num24 = 1.0f - rotation.x * rotation.x * 2f - num6;

		Vector2 center = box.GetCenter();
		Vector2 offset = new Vector2(center.x - num21 * center.x - num22 * center.y
			, center.y - num23 * center.x - num24 * center.y);

		Vector2 leftUp = new Vector2(num21 * box.Min.x + num22 * box.Max.y + offset.x
			, num23 * box.Min.x + num24 * box.Max.y + offset.y);
		Vector2 leftDown = new Vector2(num21 * box.Min.x + num22 * box.Min.y + offset.x
			, num23 * box.Min.x + num24 * box.Min.y + offset.y);
		Vector2 rightUp = new Vector2(num21 * box.Max.x + num22 * box.Max.y + offset.x
			, num23 * box.Max.x + num24 * box.Max.y + offset.y);
		Vector2 rightDown = new Vector2(num21 * box.Max.x + num22 * box.Min.y + offset.x
			, num23 * box.Max.x + num24 * box.Min.y + offset.y);

		return new hwmBox2D(new Vector2(hwmMath.Min(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
				, hwmMath.Min(leftUp.y, leftDown.y, rightUp.y, rightDown.y))
			, new Vector2(hwmMath.Max(leftUp.x, leftDown.x, rightUp.x, rightDown.x)
				, hwmMath.Max(leftUp.y, leftDown.y, rightUp.y, rightDown.y)));
	}

	public static implicit operator hwmBox2D(Bounds bound)
	{
		return new hwmBox2D(bound.center, bound.size);
	}

	public static implicit operator Bounds(hwmBox2D bound)
	{
		return new Bounds(bound.Min, bound.Max);
	}

	/// <summary>
	/// Creates and initializes a new box from the specified extents.
	/// </summary>
	/// <param name="min">InMin The box's minimum point.</param>
	/// <param name="max">InMax The box's maximum point.</param>
	public hwmBox2D(Vector2 min, Vector2 max)
	{
		Min = min;
		Max = max;
	}

	/// <summary>
	/// Gets the size of this box.
	/// </summary>
	public Vector2 GetSize()
	{
		return Max - Min;
	}

	/// <summary>
	/// Gets the center point of this box.
	/// </summary>
	public Vector2 GetCenter()
	{
		return new Vector2((Min.x + Max.x) * 0.5f
			, (Min.y + Max.y) * 0.5f);
	}

	/// <summary>
	/// Gets the extents of this box.
	/// </summary>
	public Vector2 GetExtent()
	{
		return (Max - Min) * 0.5f;
	}

	public override int GetHashCode()
	{
		return Min.GetHashCode() ^ Max.GetHashCode() << 2;
	}

	public override bool Equals(object other)
	{
		if (!(other is hwmBox2D))
			return false;

		hwmBox2D box = (hwmBox2D)other;
		return Max.Equals(box.Max) && Min.Equals(box.Min);
	}

	public override string ToString()
	{
		return string.Format("Center: {0}, Extents: {1}", Min, Max);
	}

	/// <summary>
	/// Checks whether a given box is fully encapsulated by this box.
	/// </summary>
	public bool IsInsideOrOn(hwmBox2D box)
	{
		return box.Min.x >= Min.x
			&& box.Min.y >= Min.y
			&& box.Max.x <= Max.x
			&& box.Max.y <= Max.y;
	}

	/// <summary>
	/// Checks whether the given location is inside or on this box.
	/// </summary>
	public bool IsInsideOrOn(Vector2 point)
	{
		return point.x >= Min.x
			&& point.x <= Max.x
			&& point.y >= Min.y
			&& point.y <= Max.y;
	}

	/// <summary>
	/// Checks whether the given bounding box intersects this bounding box.
	/// </summary>
	/// <param name="box">Other The bounding box to intersect with.</param>
	/// <returns>true if the boxes intersect, false otherwise.</returns>
	public bool Intersect(hwmBox2D box)
	{
		return Min.x <= box.Max.x
			&& Max.x >= box.Min.x
			&& Min.y <= box.Max.y
			&& Max.y >= box.Min.y;
	}

	public bool LineIntersection(Vector2 start, Vector2 end)
	{
		return LineIntersection(start, end, end - start);
	}

	public bool LineIntersection(Vector2 start, Vector2 end, Vector2 direction)
	{
		return LineIntersection(start, end, direction, hwmMath.Reciprocal(direction));
	}

	/// <summary>
	/// Checks whether the given line intersects this bounding box.
	/// </summary>
	/// <param name="start">line start</param>
	/// <param name="end">line end</param>
	/// <param name="direction">line direction</param>
	/// <param name="oneOverDirection">Reciprocal of direction</param>
	/// <returns>true if the boxes intersect, false otherwise.</returns>
	public bool LineIntersection(Vector2 start, Vector2 end, Vector2 direction, Vector2 oneOverDirection)
	{
		float timex, timey;
		bool bStartIsOutside = false;

		if (start.x < Min.x)
		{
			bStartIsOutside = true;
			if (end.x >= Min.x)
			{
				timex = (Min.x - start.x) * oneOverDirection.x;
			}
			else
			{
				return false;
			}
		}
		else if (start.x > Max.x)
		{
			bStartIsOutside = true;
			if (end.x <= Max.x)
			{
				timex = (Max.x - start.x) * oneOverDirection.x;
			}
			else
			{
				return false;
			}
		}
		else
		{
			timex = 0.0f;
		}

		if (start.y < Min.y)
		{
			bStartIsOutside = true;
			if (end.y >= Min.y)
			{
				timey = (Min.y - start.y) * oneOverDirection.y;
			}
			else
			{
				return false;
			}
		}
		else if (start.y > Max.y)
		{
			bStartIsOutside = true;
			if (end.y <= Max.y)
			{
				timey = (Max.y - start.y) * oneOverDirection.y;
			}
			else
			{
				return false;
			}
		}
		else
		{
			timey = 0.0f;
		}

		if (bStartIsOutside)
		{
			float maxTime = Mathf.Max(timex, timey);

			if (maxTime >= 0.0f && maxTime <= 1.0f)
			{
				Vector2 hit = start + direction * maxTime;
				const float BOX_SIDE_THRESHOLD = 0.1f;
				if (hit.x > Min.x - BOX_SIDE_THRESHOLD && hit.x < Max.x + BOX_SIDE_THRESHOLD
					&& hit.y > Min.y - BOX_SIDE_THRESHOLD && hit.y < Max.y + BOX_SIDE_THRESHOLD)
				{
					return true;
				}
			}

			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Performs a sphere vs box intersection test using Arvo's algorithm:
	/// <code>
	///		for each i in (x, y)
	///			if (sphereCenter(i) < BoxMin(i)) d2 += (sphereCenter(i) - BoxMin(i)) ^ 2
	///			else if (sphereCenter(i) > BoxMax(i)) d2 += (sphereCenter(i) - BoxMax(i)) ^ 2
	///	</code>
	/// </summary>
	/// <param name="sphereCenter">the center of the sphere being tested against the AABB</param>
	/// <param name="radiusSquared">the size of the sphere being tested</param>
	/// <returns>Whether the sphere/box intersect or not.</returns>
	public bool IntersectSphere(Vector2 sphereCenter, float radiusSquared)
	{
		// Accumulates the distance as we iterate axis
		float DistSquared = 0.0f;
		// Check each axis for min/max and add the distance accordingly
		// NOTE: Loop manually unrolled for > 2x speed up
		if (sphereCenter.x < Min.x)
		{
			DistSquared += hwmMath.Square(sphereCenter.x - Min.x);
		}
		else if (sphereCenter.x > Max.x)
		{
			DistSquared += hwmMath.Square(sphereCenter.x - Max.x);
		}
		if (sphereCenter.y < Min.y)
		{
			DistSquared += hwmMath.Square(sphereCenter.y - Min.y);
		}
		else if (sphereCenter.y > Max.y)
		{
			DistSquared += hwmMath.Square(sphereCenter.y - Max.y);
		}
		// If the distance is less than or equal to the radius, they intersect
		return DistSquared <= radiusSquared;
	}
}