using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class hwmQuadtree<T> : IEnumerable, IEnumerable<hwmQuadtree<T>.Node> where T : hwmQuadtree<T>.IElement
{
	public bool AutoMergeAndSplitNode;

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
	private Vector2 m_LooseSize;

	public void Initialize(int maxDepth
		, int maxElementPerNode
		, int minElementPreParentNode
		, Vector2 looseSize
		, hwmBounds2D worldBounds)
	{
		hwmDebug.Assert(maxDepth > 0, "maxDepth > 0");
		hwmDebug.Assert(maxElementPerNode > 0, "maxElementPerNode > 0");
		hwmDebug.Assert(minElementPreParentNode > 0, "minElementPreParentNode > 0");
		hwmDebug.Assert(maxElementPerNode > minElementPreParentNode, "maxElementPerNode > minElementPreParentNode");
		hwmDebug.Assert(looseSize.x >= 0 && looseSize.y >= 0, "looseSize.x >= 0 && looseSize.y >= 0");

		m_MaxDepth = maxDepth;
		m_MaxElementPerNode = maxElementPerNode;
		m_MinElementPreParentNode = minElementPreParentNode;
		m_LooseSize = looseSize;
		AutoMergeAndSplitNode = true;

		m_Root = new Node();
		m_Root.Initialize(this, null, worldBounds, hwmQuadtreeChilderNodeIndex.Root);
	}

	public void Dispose()
	{
		m_Root.Dispose();
		m_Root = null;
	}

	public bool TryFindContains(ref Node foundNode, hwmBounds2D bounds, Node rootNode = null)
	{
		if (rootNode == null)
		{
			rootNode = m_Root;
		}

		return rootNode.TryFindContains(ref foundNode, bounds);
	}

	public void UpdateElement(T element)
	{
		hwmDebug.Assert(element.OwnerQuadtree == this, "element.Quadtree == this");

		Node foundNode = null;
		if (element.OwnerQuadtreeNode != null)
		{
			Node elementOwner = element.OwnerQuadtreeNode;
			elementOwner.RemoveElement(element);
			TryFindContains(ref foundNode, element.QuadtreeNodeBounds, elementOwner);
		}

		if (foundNode == null)
		{
			TryFindContains(ref foundNode, element.QuadtreeNodeBounds);
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

	public void MergeAndSplitAllNode()
	{
		m_Root.MergeAllNode();
		m_Root.SplitAllNode();
	}

	public IEnumerator<Node> GetEnumerator()
	{
		return new Enumerator(m_Root);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(m_Root);
	}

	public class Node : IEnumerable, IEnumerable<T>
	{
		public const int CHILDER_COUNT = 4;

		private hwmQuadtree<T> m_Owner;
		private Node m_Parent;
		private hwmQuadtreeChilderNodeIndex m_IndexInParent;
		private Node[] m_Childers;

		private hwmBounds2D m_Bounds;
		/// <summary>
		/// size equal <see cref="m_Bounds"/> * <see cref="m_LooseSize"/>
		/// </summary>
		private hwmBounds2D m_LooseBounds;

		/// <summary>
		/// true if the meshes should be added directly to the node, rather than subdividing when possible.
		/// </summary>
		private bool m_IsLeaf;
		private int m_Depth;
		private hwmBetterList<T> m_Elements;

		public void Initialize(hwmQuadtree<T> owner, Node parent, hwmBounds2D bounds, hwmQuadtreeChilderNodeIndex indexInParent)
		{
			bool isRoot = parent == null;

			m_Owner = owner;
			m_Parent = parent;
			m_IndexInParent = indexInParent;

			m_Bounds = bounds;
			m_LooseBounds = bounds;
			m_LooseBounds.size = m_LooseBounds.size + m_Owner.m_LooseSize;

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

		public bool TryFindContains(ref Node foundNode, hwmBounds2D bounds)
		{
			if (m_LooseBounds.Contains(bounds))
			{
				if (!m_IsLeaf)
				{
					for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
					{
						if (m_Childers[iChilder].TryFindContains(ref foundNode, bounds))
						{
							return true;
						}
					}
				}
				foundNode = this;
				return true;
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

			if (m_Owner.AutoMergeAndSplitNode)
			{
				TrySplitChilders();
			}
		}

		public void RemoveElement(T element)
		{
			hwmDebug.Assert(element.OwnerQuadtreeNode == this, "element._Owner == this");

			m_Elements.Remove(element);
			element.OwnerQuadtreeNode = null;

			if (m_Owner.AutoMergeAndSplitNode)
			{
				m_Parent.TryMergeChilders();
			}
		}

		public int GetDepth()
		{
			return m_Depth;
		}

		public hwmQuadtreeChilderNodeIndex GetIndexInParent()
		{
			return m_IndexInParent;
		}

		public hwmBounds2D GetBounds()
		{
			return m_Bounds;
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

		public Node GetParent()
		{
			return m_Parent;
		}

		public Node GetChilder(hwmQuadtreeChilderNodeIndex index)
		{
			return m_Childers[(int)index];
		}

		public Node GetChilder(int index)
		{
			return m_Childers[index];
		}

		public bool IsLeaf()
		{
			return m_IsLeaf;
		}

		public int GetAllElementCount()
		{
			if (m_IsLeaf)
			{
				return m_Elements.Count;
			}
			else
			{
				return m_Elements.Count + m_Childers[0].GetAllElementCount()
					+ m_Childers[1].GetAllElementCount()
					+ m_Childers[2].GetAllElementCount()
					+ m_Childers[3].GetAllElementCount();
			}
		}

		public void GetAllNode(ref List<Node> allNode)
		{
			allNode.Add(this);
			if (!m_IsLeaf)
			{
				m_Childers[0].GetAllNode(ref allNode);
				m_Childers[1].GetAllNode(ref allNode);
				m_Childers[2].GetAllNode(ref allNode);
				m_Childers[3].GetAllNode(ref allNode);
			}
		}

		public bool TrySplitChilders()
		{
			if (m_IsLeaf
				&& m_Depth < m_Owner.m_MaxDepth
				&& m_Elements.Count > m_Owner.m_MaxElementPerNode)
			{
				m_IsLeaf = false;

				m_Childers = new Node[CHILDER_COUNT];
				for (int iChild = 0; iChild < CHILDER_COUNT; iChild++)
				{
					Node iterNode = new Node();
					m_Childers[iChild] = iterNode;
					hwmQuadtreeChilderNodeIndex index = (hwmQuadtreeChilderNodeIndex)iChild;
					iterNode.Initialize(m_Owner, this, CalculateChilderBounds(index), index);
				}

				T[] elements = m_Elements.ToArray();
				m_Elements.Clear();
				m_Elements.Capacity = 4;

				for (int iElement = 0; iElement < elements.Length; iElement++)
				{
					T iterElement = elements[iElement];
					iterElement.OwnerQuadtreeNode = null;
					Node newNode = null;
					hwmDebug.Assert(TryFindContains(ref newNode, iterElement.QuadtreeNodeBounds), "TryFindNode(ref newNode, iterElement.QuadtreeNodeBounds)");
					newNode.AddElement(iterElement);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public bool TryMergeChilders()
		{
			if (!m_IsLeaf
				&& m_Childers[0].m_IsLeaf
				&& m_Childers[1].m_IsLeaf
				&& m_Childers[2].m_IsLeaf
				&& m_Childers[3].m_IsLeaf
				&& (m_Childers[0].m_Elements.Count
					+ m_Childers[1].m_Elements.Count
					+ m_Childers[2].m_Elements.Count
					+ m_Childers[3].m_Elements.Count
					+ m_Elements.Count) < m_Owner.m_MinElementPreParentNode)
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
				return true;
			}
			else
			{
				return false;
			}
		}

		public void MergeAllNode()
		{
			if (!m_IsLeaf)
			{
				m_Childers[0].MergeAllNode();
				m_Childers[1].MergeAllNode();
				m_Childers[2].MergeAllNode();
				m_Childers[3].MergeAllNode();

				TryMergeChilders();
			}
		}

		public void SplitAllNode()
		{
			TrySplitChilders();

			if (!m_IsLeaf)
			{
				m_Childers[0].SplitAllNode();
				m_Childers[1].SplitAllNode();
				m_Childers[2].SplitAllNode();
				m_Childers[3].SplitAllNode();
			}
		}

		private hwmBounds2D CalculateChilderBounds(hwmQuadtreeChilderNodeIndex index)
		{
			Vector2 size = m_Bounds.extents;
			Vector2 extents = size * 0.5f;
			switch (index)
			{
				case hwmQuadtreeChilderNodeIndex.LeftUp:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x - extents.x
							, m_Bounds.center.y + extents.y)
						, size);
				case hwmQuadtreeChilderNodeIndex.RightUp:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x + extents.x
							, m_Bounds.center.y + extents.y)
						, size);
				case hwmQuadtreeChilderNodeIndex.LeftDown:
					return new hwmBounds2D(new Vector2(m_Bounds.center.x - extents.x
							, m_Bounds.center.y - extents.y)
						, size);
				case hwmQuadtreeChilderNodeIndex.RightDown:
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

		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private Node m_Node;
			private Node m_IterNode;
			private int m_ElementIndex;
			private int m_ChilderIndex;

			public T Current { get; private set; }

			object IEnumerator.Current { get { return Current; } }

			internal Enumerator(Node node)
			{
				m_Node = node;
				m_IterNode = m_Node;
				m_ElementIndex = 0;
				m_ChilderIndex = 0;
				Current = default(T);
			}

			public bool MoveNext()
			{
				if (m_ElementIndex < m_IterNode.m_Elements.Count)
				{
					Current = m_IterNode.m_Elements[m_ElementIndex++];
					return true;
				}
				else if (!m_IterNode.m_IsLeaf
					&& m_ChilderIndex < CHILDER_COUNT)
				{
					m_IterNode = m_IterNode.m_Childers[m_ChilderIndex];
					m_ElementIndex = 0;
					m_ChilderIndex = 0;
					return MoveNext();
				}
				else if (m_IterNode.m_Parent != null
					&& m_IterNode != m_Node)
				{
					m_ChilderIndex = (int)m_IterNode.m_IndexInParent + 1;
					m_IterNode = m_IterNode.m_Parent;
					m_ElementIndex = int.MaxValue;
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
			}

			void IEnumerator.Reset()
			{
				Current = default(T);
				m_IterNode = m_Node;
				m_ChilderIndex = 0;
				m_ElementIndex = 0;
			}
		}
	}

	public interface IElement
	{
		hwmQuadtree<T> OwnerQuadtree { get; set; }
		Node OwnerQuadtreeNode { get; set; }
		hwmBounds2D QuadtreeNodeBounds { get; set; }
	}

	public struct Enumerator : IEnumerator<Node>, IEnumerator
	{
		private Node m_RootNode;
		private int m_ChilderIndex;

		public Node Current { get; private set; }

		object IEnumerator.Current { get { return Current; } }

		internal Enumerator(Node rootNode)
		{
			m_RootNode = rootNode;
			m_ChilderIndex = 0;
			Current = null;
		}

		public bool MoveNext()
		{
			if (Current == null)
			{
				Current = m_RootNode;
				return true;
			}
			else if (!Current.IsLeaf()
			   && m_ChilderIndex < Node.CHILDER_COUNT)
			{
				Current = Current.GetChilder(m_ChilderIndex);
				m_ChilderIndex = 0;
				return true;
			}
			else if (MoveToParentNode())
			{
				return MoveNext();
			}
			else
			{
				return false;
			}
		}

		public void Dispose()
		{
			Current = null;
			m_RootNode = null;
		}

		private bool MoveToParentNode()
		{
			if (Current.GetParent() != null
			   && Current != m_RootNode)
			{
				m_ChilderIndex = (int)Current.GetIndexInParent() + 1;
				Current = Current.GetParent();
				return true;
			}
			else
			{
				return false;
			}
		}

		void IEnumerator.Reset()
		{
			Current = null;
			m_ChilderIndex = 0;
		}
	}

	public struct BoundsEnumerator : IEnumerator<Node>, IEnumerator
	{
		private Node m_RootNode;
		private int m_ChilderIndex;
		private hwmBounds2D m_Bounds;

		public Node Current { get; private set; }

		object IEnumerator.Current { get { return Current; } }

		internal BoundsEnumerator(Node rootNode, hwmBounds2D bounds)
		{
			m_RootNode = rootNode;
			m_Bounds = bounds;
			m_ChilderIndex = 0;
			Current = null;
		}

		public bool MoveNext()
		{
			if (Current == null)
			{
				if (m_RootNode.GetLooseBounds().Intersects(m_Bounds))
				{
					Current = m_RootNode;
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (!Current.IsLeaf()
			   && m_ChilderIndex < Node.CHILDER_COUNT)
			{
				for (int iIndex = m_ChilderIndex; iIndex < Node.CHILDER_COUNT; iIndex++)
				{
					Node node = Current.GetChilder(iIndex);
					if (node.GetLooseBounds().Intersects(m_Bounds))
					{
						Current = node;
						m_ChilderIndex = 0;
						return true;
					}
				}

				if (MoveToParentNode())
				{
					return MoveNext();
				}
				else
				{
					return false;
				}
			}
			else if (MoveToParentNode())
			{
				return MoveNext();
			}
			else
			{
				return false;
			}
		}

		public void Dispose()
		{
			Current = null;
			m_RootNode = null;
		}

		private bool MoveToParentNode()
		{
			if (Current.GetParent() != null
			   && Current != m_RootNode)
			{
				m_ChilderIndex = (int)Current.GetIndexInParent() + 1;
				Current = Current.GetParent();
				return true;
			}
			else
			{
				return false;
			}
		}

		void IEnumerator.Reset()
		{
			Current = null;
			m_ChilderIndex = 0;
		}
	}
}


/// <summary>
/// ----------
/// |  0  1  |
/// |  2  3  |
/// ----------
/// </summary>
public enum hwmQuadtreeChilderNodeIndex
{
	Root = -1,
	LeftUp = 0,
	RightUp = 1,
	LeftDown = 2,
	RightDown = 3,
}