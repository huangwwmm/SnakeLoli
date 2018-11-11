using UnityEngine;

/// <summary>
/// UNDONE support AssetBundle
/// </summary>
public class snAssetLoader
{
	public GameObject LoadPrefab(AssetType assetType, string prefabName, bool ignoreLog = false)
	{
		string prefabPath = string.Format("Prefabs/{0}/{1}", assetType, prefabName);
		GameObject gameObject = Resources.Load(prefabPath) as GameObject;
		if (gameObject == null && !ignoreLog)
		{
			Debug.LogError(string.Format("Load prefab AssetType:({0}) Name:({1}) Path:({2}) failed", assetType, prefabName, prefabPath));
		}
		return gameObject;
	}

	/// <summary>
	/// enum value need equal folder name
	/// </summary>
	public enum AssetType
	{
		UIRoot
	}
}