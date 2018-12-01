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
		, hwmBox2D worldBox)
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
		m_Root.Initialize(this, null, worldBox, hwmQuadtreeChilderNodeIndex.Root);
	}

	public void Dispose()
	{
		m_Root.Dispose();
		m_Root = null;
	}

	public bool TryFindContains(ref Node foundNode, hwmBox2D box, Node rootNode = null)
	{
		if (rootNode == null)
		{
			rootNode = m_Root;
		}

		return rootNode.TryFindContains(ref foundNode, box);
	}

	public void UpdateElement(T element)
	{
		hwmDebug.Assert(element.OwnerQuadtree == this, "element.Quadtree == this");

		Node foundNode = null;
		if (element.OwnerQuadtreeNode != null)
		{
			Node elementOwner = element.OwnerQuadtreeNode;
			elementOwner.RemoveElement(element);
			TryFindContains(ref foundNode, element.AABB, elementOwner);
		}

		if (foundNode == null)
		{
			TryFindContains(ref foundNode, element.AABB);
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

		private hwmBox2D m_Box;
		/// <summary>
		/// size equal <see cref="m_Box"/> * <see cref="m_LooseSize"/>
		/// </summary>
		private hwmBox2D m_LooseBox;

		/// <summary>
		/// true if the meshes should be added directly to the node, rather than subdividing when possible.
		/// </summary>
		private bool m_IsLeaf;
		private int m_Depth;
		private hwmBetterList<T> m_Elements;

		public void Initialize(hwmQuadtree<T> owner, Node parent, hwmBox2D box, hwmQuadtreeChilderNodeIndex indexInParent)
		{
			bool isRoot = parent == null;

			m_Owner = owner;
			m_Parent = parent;
			m_IndexInParent = indexInParent;

			m_Box = box;
			m_LooseBox = new hwmBox2D(m_Box.Min - m_Owner.m_LooseSize, m_Box.Max + m_Owner.m_LooseSize);

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

		public bool TryFindContains(ref Node foundNode, hwmBox2D box)
		{
			if (m_LooseBox.IsInsideOrOn(box))
			{
				if (!m_IsLeaf)
				{
					for (int iChilder = 0; iChilder < CHILDER_COUNT; iChilder++)
					{
						if (m_Childers[iChilder].TryFindContains(ref foundNode, box))
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

		public hwmBox2D GetBox()
		{
			return m_Box;
		}

		public hwmBox2D GetLooseBox()
		{
			return m_LooseBox;
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

		public void GetAllIntersectNode(ref List<Node> allNode, hwmBox2D aabb)
		{
			if (m_LooseBox.Intersect(aabb))
			{
				allNode.Add(this);
				if (!m_IsLeaf)
				{
					m_Childers[0].GetAllIntersectNode(ref allNode, aabb);
					m_Childers[1].GetAllIntersectNode(ref allNode, aabb);
					m_Childers[2].GetAllIntersectNode(ref allNode, aabb);
					m_Childers[3].GetAllIntersectNode(ref allNode, aabb);
				}
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
					iterNode.Initialize(m_Owner, this, CalculateChilderBox(index), index);
				}

				T[] elements = m_Elements.ToArray();
				m_Elements.Clear();
				m_Elements.Capacity = 4;

				for (int iElement = 0; iElement < elements.Length; iElement++)
				{
					T iterElement = elements[iElement];
					iterElement.OwnerQuadtreeNode = null;
					Node newNode = null;
					hwmDebug.Assert(TryFindContains(ref newNode, iterElement.AABB), "TryFindNode(ref newNode, iterElement.AABB)");
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

		private hwmBox2D CalculateChilderBox(hwmQuadtreeChilderNodeIndex index)
		{
			Vector2 extent = m_Box.GetExtent();
			switch (index)
			{
				case hwmQuadtreeChilderNodeIndex.LeftUp:
					return new hwmBox2D(new Vector2(m_Box.Min.x
							, m_Box.Min.y + extent.y)
						, new Vector2(m_Box.Max.x - extent.x
							, m_Box.Max.y));
				case hwmQuadtreeChilderNodeIndex.RightUp:
					return new hwmBox2D(m_Box.Min + extent
						, m_Box.Max);
				case hwmQuadtreeChilderNodeIndex.LeftDown:
					return new hwmBox2D(m_Box.Min
						, m_Box.Max - extent);
				case hwmQuadtreeChilderNodeIndex.RightDown:
					return new hwmBox2D(new Vector2(m_Box.Min.x + extent.x
							, m_Box.Min.y)
						, new Vector2(m_Box.Max.x
							, m_Box.Max.y - extent.y));
				default:
					hwmDebug.Assert(false, "invalid ChilderIndex: " + index);
					return new hwmBox2D();
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
		hwmBox2D AABB { get; set; }
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

	public struct AABBEnumerator : IEnumerator<Node>, IEnumerator
	{
		private Node m_RootNode;
		private int m_ChilderIndex;
		private hwmBox2D m_AABB;

		public Node Current { get; private set; }

		public void Reset()
		{
			Current = null;
			m_ChilderIndex = 0;
		}

		object IEnumerator.Current { get { return Current; } }

		internal AABBEnumerator(Node rootNode, hwmBox2D aabb)
		{
			m_RootNode = rootNode;
			m_AABB = aabb;
			m_ChilderIndex = 0;
			Current = null;
		}

		public bool MoveNext()
		{
			if (Current == null)
			{
				if (m_RootNode.GetLooseBox().Intersect(m_AABB))
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
					if (node.GetLooseBox().Intersect(m_AABB))
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