using UnityEngine;
using System.Collections.Generic;

public class hwmQuadtree
{
	private Node m_Root;
	private int m_MaxDepth;
	/// <summary>
	/// node will create childers node when element num more than this value
	/// <see cref="m_MaxDepth"/> priority is greater than this
	/// </summary>
	private int m_MaxElementPerNode;
	private float m_LooseScale;

	public void Initialize(int maxDepth
		, int maxElementPerNode
		, float looseScale
		, Bounds worldBounds)
	{
		hwmDebug.Assert(maxDepth > 0, "maxDepth > 0");
		hwmDebug.Assert(maxElementPerNode > 0, "maxElementPerNode > 0");
		hwmDebug.Assert(looseScale > 1, "looseScale > 0");

		m_MaxDepth = maxDepth;
		m_MaxElementPerNode = maxElementPerNode;
		m_LooseScale = looseScale;

		m_Root = new Node();
	}

	public void Destroy()
	{
		m_Root = null;
	}

	public class Node
	{
		private hwmQuadtree m_Owner;
		private Node m_Parent;
		private Node[] m_Childers;
		private Bounds m_Bounds;
		/// <summary>
		/// size equal <see cref="m_Bounds"/> * <see cref="m_LooseScale"/>
		/// </summary>
		private Bounds m_LooseBounds;
		/// <summary>
		/// true if the meshes should be added directly to the node, rather than subdividing when possible.
		/// </summary>
		private bool m_IsLeaf;
		private int m_Depth;
		private List<Element> m_Elements;
	}

	public class Element
	{
		private Bounds m_Bounds;
	}
}