using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public interface hwmICodePerformanceStatistics
{
	void Initialize();
	void Dispose();
	hwmCodePerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false);
	hwmCodePerformanceStatisticsItem Start(string itemName);
	void Start(hwmCodePerformanceStatisticsItem item);
	hwmCodePerformanceStatisticsItem Finish(string itemName);
	void Finish(hwmCodePerformanceStatisticsItem item);
	hwmCodePerformanceStatisticsItem ClearHistory(string itemName);
	void ClearHistory(hwmCodePerformanceStatisticsItem item);
}

public class hwmCodePerformanceStatistics : hwmICodePerformanceStatistics
{
	private string m_RecordDirectory;
	private Dictionary<string, hwmCodePerformanceStatisticsItem> m_Items;

	public void Initialize()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		m_RecordDirectory = Application.dataPath + "/../Temp/CodePerformanceStatistics/";
#elif UNITY_ANDROID
		m_RecordDirectory = Application.persistentDataPath + "/LogRecord/";
#endif
		if (!Directory.Exists(m_RecordDirectory))
		{
			Directory.CreateDirectory(m_RecordDirectory);
		}

		m_Items = new Dictionary<string, hwmCodePerformanceStatisticsItem>();
	}

	public void Dispose()
	{
		List<string> historyStrs = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CodePerformanceStatistics:");
		foreach (KeyValuePair<string, hwmCodePerformanceStatisticsItem> item in m_Items)
		{
			string itemFilePath = m_RecordDirectory + item.Value._Name + ".txt";
			if (File.Exists(itemFilePath))
			{
				File.Delete(itemFilePath);
			}

			List<hwmCodePerformanceStatisticsItem.History> historys = item.Value._Historys;
			long[] milliseconds = new long[historys.Count];
			long[] ticks = new long[historys.Count];
			historyStrs.Clear();
			for (int iHistory = 0; iHistory < historys.Count; iHistory++)
			{
				hwmCodePerformanceStatisticsItem.History iterHistory = historys[iHistory];
				milliseconds[iHistory] = iterHistory._Milliseconds;
				ticks[iHistory] = iterHistory._Ticks;
				historyStrs.Add(hwmCodePerformanceStatisticsItem.SerializeHistory(iterHistory));
			}
			File.WriteAllLines(itemFilePath, historyStrs.ToArray());

			if (historys.Count > 0)
			{
				ValueAnalysis tickAnalysis = new ValueAnalysis(ticks);
				ValueAnalysis millisecondsAnalysis = new ValueAnalysis(milliseconds);
				stringBuilder.AppendLine(string.Format("Name: {0} Coung:{1} AvgTick:{2:F2} AvgMs:{3:F2}"
					, item.Value._Name
					, historys.Count
					, tickAnalysis.Avg
					, millisecondsAnalysis.Avg));
			}
		}
		historyStrs.Clear();
		historyStrs = null;
		m_Items.Clear();
		m_Items = null;

		UnityEngine.Debug.Log(stringBuilder.ToString());
	}

	public hwmCodePerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false)
	{
		hwmCodePerformanceStatisticsItem item;
		if (!m_Items.TryGetValue(itemName, out item))
		{
			item = new hwmCodePerformanceStatisticsItem();
			item._Name = itemName;
			item._Stopwatch = new Stopwatch();
			item._Stopwatch.Reset();
			item._Historys = new List<hwmCodePerformanceStatisticsItem.History>();

			if (!ignoreHistoryOnLoad)
			{
				string itemFilePath = m_RecordDirectory + itemName + ".txt";
				if (File.Exists(itemFilePath))
				{
					string[] historyLines = File.ReadAllLines(itemFilePath);
					for (int iHistory = 0; iHistory < historyLines.Length; iHistory++)
					{
						string iterLine = historyLines[iHistory];
						hwmCodePerformanceStatisticsItem.History history;
						if (hwmCodePerformanceStatisticsItem.TryDeserializeHistory(iterLine, out history))
						{
							item._Historys.Add(history);
						}
					}
				}
			}

			m_Items.Add(itemName, item);
		}
		return item;
	}

	public hwmCodePerformanceStatisticsItem Start(string itemName)
	{
		hwmCodePerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Start(item);
		return item;
	}

	public void Start(hwmCodePerformanceStatisticsItem item)
	{
		item._Stopwatch.Reset();
		item._Stopwatch.Start();
	}

	public hwmCodePerformanceStatisticsItem Finish(string itemName)
	{
		hwmCodePerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Finish(item);
		return item;
	}

	public void Finish(hwmCodePerformanceStatisticsItem item)
	{
		item._Stopwatch.Stop();
		hwmCodePerformanceStatisticsItem.History history = new hwmCodePerformanceStatisticsItem.History();
		history._Milliseconds = item._Stopwatch.ElapsedMilliseconds;
		history._Ticks = item._Stopwatch.ElapsedTicks;
		item._Historys.Add(history);
	}

	public hwmCodePerformanceStatisticsItem ClearHistory(string itemName)
	{
		hwmCodePerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		ClearHistory(item);
		return item;
	}

	public void ClearHistory(hwmCodePerformanceStatisticsItem item)
	{
		item._Historys.Clear();
	}

	private struct ValueAnalysis
	{
		public int Count;
		public long Max;
		public long Min;
		public double Avg;

		public ValueAnalysis(long[] values)
		{
			Count = values.Length;
			Max = long.MinValue;
			Min = long.MaxValue;

			double total = 0;
			for (int iValue = 1; iValue < Count; iValue++)
			{
				long iterValue = values[iValue];
				Max = iterValue > Max ? iterValue : Max;
				Min = iterValue < Min ? iterValue : Min;
				total += iterValue;
			}

			if (Count > 2)
			{
				Avg = (total - Min - Max) / (Count - 2);
			}
			else
			{
				Avg = total / Count;
			}
		}
	}
}

public class hwmEmptyCodePerformanceStatistics : hwmICodePerformanceStatistics
{
	public hwmCodePerformanceStatisticsItem ClearHistory(string itemName)
	{
		return null;
	}

	public void ClearHistory(hwmCodePerformanceStatisticsItem item)
	{
	}

	public void Dispose()
	{
	}

	public hwmCodePerformanceStatisticsItem Finish(string itemName)
	{
		return null;
	}

	public void Finish(hwmCodePerformanceStatisticsItem item)
	{
	}

	public void Initialize()
	{
	}

	public hwmCodePerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false)
	{
		return null;
	}

	public hwmCodePerformanceStatisticsItem Start(string itemName)
	{
		return null;
	}

	public void Start(hwmCodePerformanceStatisticsItem item)
	{
	}
}

public class hwmCodePerformanceStatisticsItem
{
	internal string _Name;
	internal Stopwatch _Stopwatch;
	internal List<History> _Historys;

	public static string SerializeHistory(History history)
	{
		return string.Format("{0},{1}", history._Milliseconds, history._Ticks);
	}

	public static bool TryDeserializeHistory(string str, out History history)
	{
		int index = str.IndexOf(',');
		history = new History();
		return index > 0 && index < str.Length - 1
			&& long.TryParse(str.Substring(0, index), out history._Milliseconds)
			&& long.TryParse(str.Substring(index + 1), out history._Ticks);
	}

	public struct History
	{
		internal long _Milliseconds;
		internal long _Ticks;
	}
}