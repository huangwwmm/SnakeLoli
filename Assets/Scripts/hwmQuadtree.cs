using UnityEngine;
using System.Collections.Generic;

public class hwmQuadtree
{
	private Node m_Root;
	private int m_MaxDepth;
	/// <summary>
	/// node will split node when element num more than this value
	/// <see cref="m_MaxDepth"/> priority is greater than this
	/// </summary>
	private int m_MaxElementPerNode;
	/// <summary>
	/// childers will be merged into parent when childers element num less than this value
	/// </summary>
	private int m_MinElementPreParentNode;
	private float m_LooseScale;

	public void Initialize(int maxDepth
		, int maxElementPerNode
		, int minElementPreParentNode
		, float looseScale
		, hwmBounds2D worldBounds)
	{
		hwmDebug.Assert(maxDepth > 0, "maxDepth > 0");
		hwmDebug.Assert(maxElementPerNode > 0, "maxElementPerNode > 0");
		hwmDebug.Assert(minElementPreParentNode > 0, "minElementPreParentNode > 0");
		hwmDebug.Assert(maxElementPerNode > minElementPreParentNode, "maxElementPerNode > minElementPreParentNode");
		hwmDebug.Assert(looseScale > 1, "looseScale > 0");

		m_MaxDepth = maxDepth;
		m_MaxElementPerNode = maxElementPerNode;
		m_MinElementPreParentNode = minElementPreParentNode;
		m_LooseScale = looseScale;

		m_Root = new Node();
		m_Root.Initialize(this, null, worldBounds);
	}

	public void Dispose()
	{
		m_Root.Dispose();
		m_Root = null;
	}

	public void UpdateElement(Element element)
	{
		hwmDebug.Assert(element.Quadtree == this, "element.Quadtree == this");

		if (element._Owner != null)
		{
			Node elementOwner = element._Owner;
			elementOwner.RemoveElement(element);

			if (element._Owner.UpdateElement(element))
			{
				return;
			}
		}

		if (!m_Root.UpdateElement(element))
		{
			m_Root.AddElement(element);
		}
	}

	public class Node
	{
		private const int CHILDER_COUNT = 4;

		private hwmQuadtree m_Owner;
		private Node m_Parent;
		private Node[] m_Childers;

		private hwmBounds2D m_Bounds;
		/// <summary>
		/// size equal <see cref="m_Bounds"/> * <see cref="m_LooseScale"/>
		/// </summary>
		private hwmBounds2D m_LooseBounds;

		/// <summary>
		/// true if the meshes should be added directly to the node, rather than subdividing when possible.
		/// </summary>
		private bool m_IsLeaf;
		private int m_Depth;
		private hwmFreeList<Element> m_Elements;

		public void Initialize(hwmQuadtree owner, Node parent, hwmBounds2D bounds)
		{
			bool isRoot = parent == null;

			m_Owner = owner;
			m_Parent = parent;

			m_Bounds = bounds;
			m_LooseBounds = bounds;
			m_LooseBounds.size = m_LooseBounds.size * m_Owner.m_LooseScale;

			m_IsLeaf = true;
			m_Depth = isRoot ? 1 : parent.m_Depth + 1;
			m_Elements = new hwmFreeList<Element>();
		}

		public void Dispose()
		{
			m_Elements.Clear();
			m_Elements = null;
			m_Childers = null;
			m_Parent = null;
		}

		public bool UpdateElement(Element element)
		{
			if (m_LooseBounds.Contains(element.Bounds))
			{
				if (m_IsLeaf)
				{
					AddElement(element);
					return true;
				}
				else
				{
					bool updateInChildersSuccess = false;
					for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
					{
						if (m_Childers[iChilder].UpdateElement(element))
						{
							updateInChildersSuccess = true;
							break;
						}
					}

					if (!updateInChildersSuccess)
					{
						AddElement(element);
					}
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public void AddElement(Element element)
		{
			hwmDebug.Assert(element._Owner == null, "element._Owner == null");

			element._Owner = this;
			m_Elements.Add(element);

			if (m_IsLeaf
				&& m_Depth < m_Owner.m_MaxDepth
				&& m_Elements.Count > m_Owner.m_MaxElementPerNode)
			{
				SplitChilders();
			}
		}

		public void RemoveElement(Element element)
		{
			hwmDebug.Assert(element._Owner == this, "element._Owner == this");

			m_Elements.Remove(element);
			element._Owner = null;

			if (m_Parent != null)
			{
				Node[] parentChilders = m_Parent.m_Childers;
				if (parentChilders[0].m_IsLeaf
					&& parentChilders[1].m_IsLeaf
					&& parentChilders[2].m_IsLeaf
					&& parentChilders[3].m_IsLeaf
					&& (parentChilders[0].m_Elements.Count
						+ parentChilders[1].m_Elements.Count
						+ parentChilders[2].m_Elements.Count
						+ parentChilders[3].m_Elements.Count
						+ m_Parent.m_Elements.Count) < m_Owner.m_MinElementPreParentNode)
				{
					m_Parent.MergeChilders();
				}
			}
		}

		public hwmFreeList<Element> GetElements()
		{
			return m_Elements;
		}

		private void SplitChilders()
		{
			hwmDebug.Assert(m_IsLeaf, "m_IsLeaf");
			m_IsLeaf = false;

			m_Childers = new Node[CHILDER_COUNT];
			for (int iChild = 0; iChild < CHILDER_COUNT; iChild++)
			{
				Node iterNode = new Node();
				m_Childers[iChild] = iterNode;
				iterNode.Initialize(m_Owner, this, CalculateChilderBounds((ChilderIndex)iChild));
			}

			Element[] elements = m_Elements.ToArray();
			m_Elements.Clear();
			m_Elements.Capacity = 4;

			for (int iElement = 0; iElement < elements.Length; iElement++)
			{
				Element iterElement = elements[iElement];
				iterElement._Owner = null;
				hwmDebug.Assert(UpdateElement(elements[iElement]), "UpdateElement(elements[iElement])");
			}
		}

		private void MergeChilders()
		{
			for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
			{
				Node iterChilder = m_Childers[iChilder];
				Element[] iterElements = iterChilder.m_Elements.ToArray();

				for (int iElement = 0; iElement < iterElements.Length; iElement++)
				{
					Element iterElement = iterElements[iElement];
					iterElement._Owner = null;
					AddElement(iterElement);
				}

				iterChilder.Dispose();
			}

			m_IsLeaf = true;
			m_Childers = null;
		}

		private hwmBounds2D CalculateChilderBounds(ChilderIndex index)
		{
			Vector2 size = m_Bounds.extents;
			Vector2 extents = size * 0.5f;
			switch (index)
			{
				case ChilderIndex.LeftUp:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x - extents.x
							, m_Bounds.center.y + extents.y)
						, size);
				case ChilderIndex.RightUp:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x + extents.x
							, m_Bounds.center.y + extents.y)
						, size);
				case ChilderIndex.LeftDown:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x - extents.x
							, m_Bounds.center.y - extents.y)
						, size);
				case ChilderIndex.RightDown:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x + extents.x
							, m_Bounds.center.y - extents.y)
						, size);
				default:
					hwmDebug.Assert(false, "invalid ChilderIndex: " + index);
					return new hwmBounds2D();
			}
		}

		/// <summary>
		/// ----------
		/// |  0  1  |
		/// |  2  3  |
		/// ----------
		/// </summary>
		public enum ChilderIndex
		{
			LeftUp = 0,
			RightUp = 1,
			LeftDown = 2,
			RightDown = 3,
		}
	}

	public class Element
	{
		public readonly hwmQuadtree Quadtree;
		/// <summary>
		/// your need call <see cref="UpdateElement"/> after change this value
		/// Q: why not auto call
		/// A: for performance
		/// </summary>
		public hwmBounds2D Bounds;

		/// <summary>
		/// only use in <see cref="hwmQuadtree"/>
		/// </summary>
		internal Node _Owner;

		public Element(hwmQuadtree quadtree)
		{
			Quadtree = quadtree;
		}

		public void UpdateElement()
		{
			Quadtree.UpdateElement(this);
		}

		public void RemoveElement()
		{
			if (_Owner != null)
			{
				_Owner.RemoveElement(this);
			}
		}
	}
}