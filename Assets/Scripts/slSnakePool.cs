using System;
using System.Collections.Generic;
using UnityEngine;

public class slSnakePool
{
	private Transform m_Root;
	private Dictionary<string, int> m_SnakeNameToIndex;
	private Dictionary<string, slSnakeTweakableProperties> m_TweakablePropertiess;
	private List<slSnakeProperties> m_Propertiess;
	private List<NodePool<slSnake.HeadNode>> m_HeadPools;
	private List<NodePool<slSnake.ClothesNode>> m_ClothesNodePools;
	private List<NodePool<slSnake.BodyNode>> m_BodyPools;

	public void Initialize()
	{
		m_Root = new GameObject("Snakes").transform;
		m_SnakeNameToIndex = new Dictionary<string, int>();
		m_TweakablePropertiess = new Dictionary<string, slSnakeTweakableProperties>();
		m_Propertiess = new List<slSnakeProperties>();

		m_HeadPools = new List<NodePool<slSnake.HeadNode>>();
		m_ClothesNodePools = new List<NodePool<slSnake.ClothesNode>>();
		m_BodyPools = new List<NodePool<slSnake.BodyNode>>();
	}

	public void Dispose()
	{
		for (int iPool = 0; iPool < m_BodyPools.Count; iPool++)
		{
			m_HeadPools[iPool].Dispose();
			m_ClothesNodePools[iPool].Dispose();
			m_BodyPools[iPool].Dispose();
		}
		m_HeadPools.Clear();
		m_HeadPools = null;
		m_ClothesNodePools.Clear();
		m_ClothesNodePools = null;
		m_BodyPools.Clear();
		m_BodyPools = null;

		m_Propertiess.Clear();
		m_Propertiess = null;
		m_TweakablePropertiess.Clear();
		m_TweakablePropertiess = null;
		m_SnakeNameToIndex.Clear();
		m_SnakeNameToIndex = null;
		UnityEngine.Object.Destroy(m_Root.gameObject);
	}

	public slSnakeTweakableProperties GetTweakableProperties(string propertiesName)
	{
		slSnakeTweakableProperties properties;
		if (!m_TweakablePropertiess.TryGetValue(propertiesName, out properties))
		{
			properties = UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game
				, slConstants.SNAKE_TWEAKABLE_PROPERTIES_PREfAB_NAME_STARTWITHS + propertiesName)) as slSnakeTweakableProperties;
			m_TweakablePropertiess.Add(propertiesName, properties);
		}
		return properties;
	}

	public slSnakeProperties GetProperties(string snakeName)
	{
		return m_Propertiess[SnakeNameToIndex(snakeName)];
	}

	public slSnake.HeadNode PopHeadNode(string snakeName, string owner)
	{
		slSnake.HeadNode node = m_HeadPools[SnakeNameToIndex(snakeName)].Pop();
		node.Node.name = owner;
		node.Predict.name = owner;
		node.Node.SetActive(true);
		node.Predict.SetActive(true);
		return node;
	}

	public slSnake.ClothesNode PopClothesNode(string snakeName, string owner)
	{
		slSnake.ClothesNode node = m_ClothesNodePools[SnakeNameToIndex(snakeName)].Pop();
		node.Node.name = owner;
		node.Node.SetActive(true);
		return node;
	}

	public slSnake.BodyNode PopBodyNode(string snakeName, string owner)
	{
		slSnake.BodyNode node = m_BodyPools[SnakeNameToIndex(snakeName)].Pop();
		node.Node.name = owner;
		node.Node.SetActive(true);
		return node;
	}

	public void PushHeadNode(string snakeName, slSnake.HeadNode node)
	{
		node.Node.SetActive(false);
		node.Predict.SetActive(false);
		m_HeadPools[SnakeNameToIndex(snakeName)].Push(node);
	}

	public void PushClothesNode(string snakeName, slSnake.ClothesNode node)
	{
		node.Node.SetActive(false);
		m_ClothesNodePools[SnakeNameToIndex(snakeName)].Push(node);
	}

	public void PushBodyNode(string snakeName, slSnake.BodyNode node)
	{
		node.Node.SetActive(false);
		m_BodyPools[SnakeNameToIndex(snakeName)].Push(node);
	}

	private int SnakeNameToIndex(string snakeName)
	{
		int index;
		if (!m_SnakeNameToIndex.TryGetValue(snakeName, out index))
		{
			Debug.LogError("Not loaded snake: " + snakeName);
		}
		return index;
	}

	public void LoadSnake(string snakeName)
	{
		int index;
		if (!m_SnakeNameToIndex.TryGetValue(snakeName, out index))
		{
			index = m_Propertiess.Count;
			m_SnakeNameToIndex.Add(snakeName, index);

			m_Propertiess.Add(UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game
				, slConstants.SNAKE_PROPERTIES_PREfAB_NAME_STARTWITHS + snakeName)) as slSnakeProperties);


			slSnakePresentationProperties snakePresentationProperties = slWorld.GetInstance().NeedPresentation()
				? UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader()
						.LoadAsset(hwmAssetLoader.AssetType.Game, slConstants.SNAKE_PRESENTATION_PROPERTIES_PREfAB_NAME_STARTWITHS + snakeName) as GameObject)
					.GetComponent<slSnakePresentationProperties>()
				: null;
			snakePresentationProperties.transform.SetParent(m_Root, false);

			NodePool<slSnake.HeadNode> headNodePool = new NodePool<slSnake.HeadNode>(snakePresentationProperties, m_Root);
			headNodePool.Initialize(4);
			m_HeadPools.Add(headNodePool);

			NodePool<slSnake.ClothesNode> clothesNodePool = new NodePool<slSnake.ClothesNode>(snakePresentationProperties, m_Root);
			clothesNodePool.Initialize(4);
			m_ClothesNodePools.Add(clothesNodePool);

			NodePool<slSnake.BodyNode> bodyNodePool = new NodePool<slSnake.BodyNode>(snakePresentationProperties, m_Root);
			bodyNodePool.Initialize(4);
			m_BodyPools.Add(bodyNodePool);
		}
	}

	private class NodePool<T> : hwmPool<T> where T : slSnake.BodyNode, new()
	{
		private Transform m_Root;
		private slSnakePresentationProperties m_Presentation;
		private NodeType m_NodeType;

		public NodePool(slSnakePresentationProperties presentation, Transform root)
		{
			m_Presentation = presentation;
			m_Root = root;

			Type nodeType = typeof(T);
			m_NodeType = nodeType == typeof(slSnake.HeadNode)
				? NodeType.Head
				: nodeType == typeof(slSnake.ClothesNode)
					? NodeType.Clothes
					: NodeType.Body;
		}

		protected override T HandleCreateItem()
		{
			T node = new T();

			if (m_Presentation != null)
			{
				switch (m_NodeType)
				{
					case NodeType.Head:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Head);
						(node as slSnake.HeadNode).Sprite = node.Node.GetComponentsInChildren<SpriteRenderer>();
						break;
					case NodeType.Clothes:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Clothes);
						node.Sprite = node.Node.GetComponent<SpriteRenderer>();
						break;
					case NodeType.Body:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Body);
						node.Sprite = node.Node.GetComponent<SpriteRenderer>();
						break;
				}
			}
			else
			{
				node.Node = new GameObject();
			}

			node.Node.name = m_NodeType.ToString();
			node.Node.transform.SetParent(m_Root, false);

			node.Collider = node.Node.AddComponent<CircleCollider2D>();
			node.Collider.isTrigger = m_NodeType == NodeType.Head;

			if (m_NodeType == NodeType.Head)
			{
				slSnake.HeadNode headNode = node as slSnake.HeadNode;
				headNode.Trigger = headNode.Node.AddComponent<slSnakeHeadTrigger>();
				headNode.Rigidbody = headNode.Node.AddComponent<Rigidbody2D>();
				headNode.Rigidbody.isKinematic = true;
				headNode.Predict = new GameObject("Predict");
				headNode.Predict.transform.SetParent(m_Root, false);
				headNode.Predict.layer = (int)slConstants.Layer.SnakePredict;
				headNode.PredictCollider = headNode.Predict.AddComponent<BoxCollider2D>();
			}

			return node;
		}
	}

	private enum NodeType
	{
		Head,
		Clothes,
		Body
	}
}