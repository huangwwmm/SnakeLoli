﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class slQuadtreeGizmosWindow : EditorWindow
{
	private QuadtreeGizmosEnumerator<slFood> m_FoodEnumerator;
	private slQuadtreeGizmos m_QuadtreeGizmos;

	[MenuItem("Custom/Snake Loli/Quadtree Gizmos", false, 0)]
	public static void ShowWindow()
	{
		GetWindow<slQuadtreeGizmosWindow>("Quadtree Gizmos", true);
	}

	protected void OnEnable()
	{
	}

	protected void OnDisable()
	{
		if (m_QuadtreeGizmos != null)
		{
			m_QuadtreeGizmos.OnBehaviourDrawGizmos -= OnDrawGizmos;
			DestroyImmediate(m_QuadtreeGizmos.gameObject);
		}

		if (m_FoodEnumerator != null)
		{
			m_FoodEnumerator.Dispose();
			m_FoodEnumerator = null;
		}
	}

	protected void OnGUI()
	{
		if (!Application.isPlaying)
		{
			EditorGUILayout.HelpBox("only runtime", MessageType.Warning);
			return;
		}

		if (m_FoodEnumerator == null
			&& slQuadtreeGizmos.FoodQuadtree != null)
		{
			m_FoodEnumerator = new QuadtreeGizmosEnumerator<slFood>(slQuadtreeGizmos.FoodQuadtree);
		}

		if (m_FoodEnumerator == null)
		{
			EditorGUILayout.HelpBox("not have quadtree", MessageType.Warning);
			return;
		}

		if (m_QuadtreeGizmos == null)
		{
			GameObject go = new GameObject("Quadtree Gizmos");
			m_QuadtreeGizmos = go.AddComponent<slQuadtreeGizmos>();
			m_QuadtreeGizmos.OnBehaviourDrawGizmos += OnDrawGizmos;
		}

		m_FoodEnumerator.OnGUI();
	}

	private void OnDrawGizmos()
	{
		if (m_FoodEnumerator != null)
		{
			m_FoodEnumerator.DoDrawGizmos();
		}
	}

	private class QuadtreeGizmosEnumerator<T> where T : hwmQuadtree<T>.IElement
	{
		private const hwmQuadtreeChilderNodeIndex NOTSET_CHILDER_INDEX = (hwmQuadtreeChilderNodeIndex)(-1);
		private Color m_GizmosColor;
		private float m_GizmosZ;
		private bool m_GizomosDisplayChilderElement;
		private hwmQuadtree<T> m_Quadtree;
		private hwmQuadtree<T>.Node m_Current;
		private Stack<hwmQuadtree<T>.Node> m_LastNodes;
		private hwmQuadtreeChilderNodeIndex m_ChilderIndex;

		public QuadtreeGizmosEnumerator(hwmQuadtree<T> quadtree)
		{
			m_Quadtree = quadtree;
			m_Current = m_Quadtree.GetRootNode();
			m_GizmosColor = Color.black;
			m_GizmosZ = 0;
			m_GizomosDisplayChilderElement = true;
			m_LastNodes = new Stack<hwmQuadtree<T>.Node>();
			m_ChilderIndex = NOTSET_CHILDER_INDEX;
		}

		public void Dispose()
		{
			m_Quadtree = null;
			m_Current = null;
			m_LastNodes = null;
		}

		public void DoDrawGizmos()
		{
			if (m_Current != null)
			{
				hwmQuadtree<T>.Node gizmosNode = GetDisplayNode();
				Gizmos.color = m_GizmosColor;
				hwmUtility.GizmosDrawBox(gizmosNode.GetLooseBox(), m_GizmosZ);
				DoDrawGizomsElements(gizmosNode);
			}
		}

		public void OnGUI()
		{
			while (m_Current == null)
			{
				if (m_LastNodes.Count > 0)
				{
					m_Current = m_LastNodes.Pop();
				}
				else
				{
					m_Current = m_Quadtree.GetRootNode();
					break;
				}
			}

			if (m_Current == null)
			{
				EditorGUILayout.HelpBox("not found any node", MessageType.Error);
				return;
			}

			hwmQuadtree<T>.Node displayNode = GetDisplayNode();
			EditorGUILayout.LabelField("Depth:", displayNode.GetDepth().ToString());
			EditorGUILayout.LabelField("Element Count:", displayNode.GetElements().Count.ToString());
			EditorGUILayout.LabelField("Element Count In Self&Childers :", displayNode.GetAllElementCount().ToString());

			EditorGUILayout.Space();
			if (m_Current.GetParent() == null
				&& m_ChilderIndex == NOTSET_CHILDER_INDEX)
			{
				GUILayout.Button("No Parent");
			}
			else if (GUILayout.Button("Parent"))
			{
				if (m_ChilderIndex != NOTSET_CHILDER_INDEX)
				{
					m_ChilderIndex = NOTSET_CHILDER_INDEX;
				}
				else
				{
					m_Current = m_LastNodes.Pop();
				}
			}
			EditorGUILayout.BeginHorizontal();
			OnGUI_ChildersButton(hwmQuadtreeChilderNodeIndex.LeftUp);
			OnGUI_ChildersButton(hwmQuadtreeChilderNodeIndex.RightUp);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			OnGUI_ChildersButton(hwmQuadtreeChilderNodeIndex.LeftDown);
			OnGUI_ChildersButton(hwmQuadtreeChilderNodeIndex.RightDown);
			EditorGUILayout.EndHorizontal();
			if (m_Current.IsLeaf() || m_ChilderIndex == NOTSET_CHILDER_INDEX)
			{
				GUILayout.Button("Cant Enter Childer");
			}
			else if (GUILayout.Button("Enter Childer: " + m_ChilderIndex.ToString()))
			{
				m_LastNodes.Push(m_Current);
				m_Current = m_Current.GetChilder(m_ChilderIndex);
				m_ChilderIndex = NOTSET_CHILDER_INDEX;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Gizmos");
			m_GizmosColor = EditorGUILayout.ColorField("Color", m_GizmosColor);
			m_GizmosZ = EditorGUILayout.FloatField("Z", m_GizmosZ);
			m_GizomosDisplayChilderElement = EditorGUILayout.Toggle("Display Childer Element", m_GizomosDisplayChilderElement);

			if (GUI.changed)
			{
				GetWindow<SceneView>().Focus();
			}
		}

		private hwmQuadtree<T>.Node GetDisplayNode()
		{
			return m_ChilderIndex == NOTSET_CHILDER_INDEX
				? m_Current
				: m_Current.GetChilder(m_ChilderIndex) != null
					? m_Current.GetChilder(m_ChilderIndex)
					: m_Current;
		}

		private void OnGUI_ChildersButton(hwmQuadtreeChilderNodeIndex index)
		{
			if (m_Current.IsLeaf())
			{
				GUILayout.Button("No " + index.ToString());
			}
			else if (GUILayout.Button(index.ToString()))
			{
				
				if (m_ChilderIndex == index) // double click enter childer
				{
					m_LastNodes.Push(m_Current);
					m_Current = m_Current.GetChilder(m_ChilderIndex);
					m_ChilderIndex = NOTSET_CHILDER_INDEX;
				}
				else
				{ 
					m_ChilderIndex = index;
				}
			}
		}

		private void DoDrawGizomsElements(hwmQuadtree<T>.Node node)
		{
			if (node == null)
			{
				return;
			}
			hwmBetterList<T> elements = node.GetElements();
			for (int iElement = 0; iElement < elements.Count; iElement++)
			{
				hwmUtility.GizmosDrawBox(elements[iElement].AABB, m_GizmosZ);
			}

			if (m_GizomosDisplayChilderElement && !node.IsLeaf())
			{
				DoDrawGizomsElements(node.GetChilder(hwmQuadtreeChilderNodeIndex.LeftDown));
				DoDrawGizomsElements(node.GetChilder(hwmQuadtreeChilderNodeIndex.LeftUp));
				DoDrawGizomsElements(node.GetChilder(hwmQuadtreeChilderNodeIndex.RightUp));
				DoDrawGizomsElements(node.GetChilder(hwmQuadtreeChilderNodeIndex.RightDown));
			}
		}
	}
}