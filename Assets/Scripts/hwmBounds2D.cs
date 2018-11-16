using UnityEngine;

public struct hwmBounds2D
{
	public Vector2 center { get; set; }

	public Vector2 extents { get; set; }

	public Vector2 size
	{
		get
		{
			return extents * 2f;
		}
		set
		{
			extents = value * 0.5f;
		}
	}

	public Vector2 min
	{
		get
		{
			return center - extents;
		}
		set
		{
			SetMinMax(value, max);
		}
	}

	public Vector2 max
	{
		get
		{
			return center + extents;
		}
		set
		{
			SetMinMax(min, value);
		}
	}

	public static bool operator ==(hwmBounds2D lhs, hwmBounds2D rhs)
	{
		return lhs.center == rhs.center && lhs.extents == rhs.extents;
	}

	public static bool operator !=(hwmBounds2D lhs, hwmBounds2D rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator hwmBounds2D(Bounds bound)
	{
		return new hwmBounds2D(bound.center, bound.size);
	}

	public static implicit operator Bounds(hwmBounds2D bound)
	{
		return new Bounds(bound.center, bound.size);
	}

	public hwmBounds2D(Vector2 center, Vector2 size)
	{
		this.center = center;
		extents = size * 0.5f;
	}

	public override int GetHashCode()
	{
		return center.GetHashCode() ^ extents.GetHashCode() << 2;
	}

	public override bool Equals(object other)
	{
		if (!(other is hwmBounds2D))
			return false;

		hwmBounds2D bounds = (hwmBounds2D)other;
		return center.Equals(bounds.center) && extents.Equals(bounds.extents);
	}

	public override string ToString()
	{
		return string.Format("Center: {0}, Extents: {1}", center, extents);
	}

	public void SetMinMax(Vector2 min, Vector2 max)
	{
		extents = (max - min) * 0.5f;
		center = min + extents;
	}

	public bool Contains(hwmBounds2D bounds)
	{
		return min.x <= bounds.min.x
			&& min.y <= bounds.min.y
			&& max.x >= bounds.max.x
			&& max.y >= bounds.max.y;
	}

	public bool Contains(Vector2 point)
	{
		return min.x <= point.x
			&& min.y <= point.y
			&& max.x >= point.x
			&& max.y >= point.y;
	}
}