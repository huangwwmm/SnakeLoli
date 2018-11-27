using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class hwmQuadtree<T> where T : hwmQuadtree<T>.IElement
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

	public bool TryFindNode(ref Node foundNode, hwmBounds2D bounds, Node rootNode = null)
	{
		if (rootNode == null)
		{
			rootNode = m_Root;
		}
		bounds.SetMinMax(new Vector2(Mathf.Max(bounds.min.x, m_Root.GetLooseBounds().min.x), Mathf.Max(bounds.min.y, m_Root.GetLooseBounds().min.y))
			, new Vector2(Mathf.Min(bounds.max.y, m_Root.GetLooseBounds().max.x), Mathf.Min(bounds.max.y, m_Root.GetLooseBounds().max.y)));

		return rootNode.TryFindNode(ref foundNode, bounds);
	}

	public void UpdateElement(T element)
	{
		hwmDebug.Assert(element.OwnerQuadtree == this, "element.Quadtree == this");

		Node foundNode = null;
		if (element.OwnerQuadtreeNode != null)
		{
			Node elementOwner = element.OwnerQuadtreeNode;
			elementOwner.RemoveElement(element);
			TryFindNode(ref foundNode, element.QuadtreeNodeBounds, elementOwner);
		}

		if (foundNode == null)
		{
			TryFindNode(ref foundNode, element.QuadtreeNodeBounds);
		}
		if (foundNode == null)
		{
			foundNode = m_Root;
		}

		foundNode.AddElement(element);
	}

	public void RemoveElement(T element)
	{
		if (element.OwnerQuadtreeNode != null)
		{
			element.OwnerQuadtreeNode.RemoveElement(element);
		}
	}

	public Node GetRootNode()
	{
		return m_Root;
	}

	public class Node:IEnumerable,  IEnumerable<T>
	{
		private const int CHILDER_COUNT = 4;

		private hwmQuadtree<T> m_Owner;
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
		private hwmBetterList<T> m_Elements;

		public void Initialize(hwmQuadtree<T> owner, Node parent, hwmBounds2D bounds)
		{
			bool isRoot = parent == null;

			m_Owner = owner;
			m_Parent = parent;

			m_Bounds = bounds;
			m_LooseBounds = bounds;
			m_LooseBounds.size = m_LooseBounds.size * m_Owner.m_LooseScale;

			m_IsLeaf = true;
			m_Depth = isRoot ? 1 : parent.m_Depth + 1;
			m_Elements = new hwmBetterList<T>();
		}

		public void Dispose()
		{
			m_Elements.Clear();
			m_Elements = null;
			m_Childers = null;
			m_Parent = null;
		}

		public bool TryFindNode(ref Node foundNode, hwmBounds2D bounds)
		{
			if (m_LooseBounds.Contains(bounds))
			{
				if (m_IsLeaf)
				{
					foundNode = this;
					return true;
				}
				else
				{
					for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
					{
						if (m_Childers[iChilder].TryFindNode(ref foundNode, bounds))
						{
							return true;
						}
					}
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public void AddElement(T element)
		{
			hwmDebug.Assert(element.OwnerQuadtreeNode == null, "element._Owner == null");

			element.OwnerQuadtreeNode = this;
			m_Elements.Add(element);

			if (m_IsLeaf
				&& m_Depth < m_Owner.m_MaxDepth
				&& m_Elements.Count > m_Owner.m_MaxElementPerNode)
			{
				SplitChilders();
			}
		}

		public void RemoveElement(T element)
		{
			hwmDebug.Assert(element.OwnerQuadtreeNode == this, "element._Owner == this");

			m_Elements.Remove(element);
			element.OwnerQuadtreeNode = null;

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

		public hwmBounds2D GetLooseBounds()
		{
			return m_LooseBounds;
		}

		public hwmBetterList<T> GetElements()
		{
			return m_Elements;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
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

			T[] elements = m_Elements.ToArray();
			m_Elements.Clear();
			m_Elements.Capacity = 4;

			for (int iElement = 0; iElement < elements.Length; iElement++)
			{
				T iterElement = elements[iElement];
				iterElement.OwnerQuadtreeNode = null;
			}
		}

		private void MergeChilders()
		{
			for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
			{
				Node iterChilder = m_Childers[iChilder];
				T[] iterElements = iterChilder.m_Elements.ToArray();

				for (int iElement = 0; iElement < iterElements.Length; iElement++)
				{
					T iterElement = iterElements[iElement];
					iterElement.OwnerQuadtreeNode = null;
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

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
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

		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private Node m_Node;
			private Node m_IterNode;
			private int m_ChilderIndex;
			private int m_ElementIndex;
			private Stack<int> m_ChilderIndexs;

			public T Current { get; private set; }

			object IEnumerator.Current { get { return Current; } }

			internal Enumerator(Node node)
			{
				m_Node = node;
				m_IterNode = m_Node;
				m_ChilderIndex = 0;
				m_ElementIndex = 0;
				m_ChilderIndexs = new Stack<int>();
				Current = default(T);
			}

			public bool MoveNext()
			{
				if ((uint)m_ElementIndex < (uint)m_IterNode.m_Elements.Count)
				{
					Current = m_IterNode.m_Elements[m_ElementIndex++];
					return true;
				}
				else if (!m_IterNode.m_IsLeaf
					&& (uint)m_ChilderIndex < CHILDER_COUNT)
				{
					m_IterNode = m_IterNode.m_Childers[m_ChilderIndex++];
					m_ChilderIndexs.Push(m_ChilderIndex);
					m_ElementIndex = 0;
					m_ChilderIndex = 0;
					return MoveNext();
				}
				else if (m_ChilderIndexs.Count > 0)
				{
					m_IterNode = m_IterNode.m_Parent;
					m_ChilderIndex = m_ChilderIndexs.Pop();
					return MoveNext();
				}
				else
				{
					return false;
				}
			}

			public void Dispose()
			{
				Current = default(T);
				m_Node = null;
				m_IterNode = null;
				m_ChilderIndexs.Clear();
				m_ChilderIndexs = null;
			}

			void IEnumerator.Reset()
			{
				Current = default(T);
				m_IterNode = m_Node;
				m_ChilderIndex = 0;
				m_ElementIndex = 0;
				m_ChilderIndexs.Clear();
			}
		}
	}

	public interface IElement
	{
		hwmQuadtree<T> OwnerQuadtree { get; set; }
		Node OwnerQuadtreeNode { get; set; }
		hwmBounds2D QuadtreeNodeBounds { get; set; }
	}
}