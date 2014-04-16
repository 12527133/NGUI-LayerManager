using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayerManager
{
	//每一层的深度范围,不超过1000//
	public const int Layer_Distance = 1000;
	
	//层的信息//
	struct Layer
	{
		//层名//
		public string name;
		
		//层中的GameObject//
		public List<GameObject> panels;
	}
	
	
	//每一层//
	private List<Layer> mLayers = new List<Layer>(10);
	
	
	#region Layer
	
	//添加新的层到顶部//
	public void AddLayer(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		
		if(LayerContains(name)) return;
		
		Layer layer = new Layer();
		layer.name = name;
		mLayers.Add(layer);
	}
	
	//插入新的层, 负数:底部, 正数(大于count):顶部, 其他:中间//
	
	//InsertLayer("New Layer",-1);
	//InsertLayer("New Layer",int.MaxValue);
	//InsertLayer("New Layer",1);
	
	public void InsertLayer(string name,int index)
	{
		if(string.IsNullOrEmpty(name)) return;
		if(LayerContains(name)) return;
		
		Layer layer = new Layer();
		layer.name = name;
		
		
		if(index < 0) index = 0;
		
		if(index > mLayers.Count) index = mLayers.Count;
		
		mLayers.Insert(index,layer);
		
		SortLayers(index+1,mLayers.Count-index-1);
	}
	
	//将指定层向上移动一层,如果本身已经是顶层,则不会移动//
	public void MoveUpLayer(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		
		int currentIndex = NameToLayer(name);
		
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + name +  ") is not in the LayerManager");
			return;
		}
		
		if(currentIndex == mLayers.Count-1)
		{
			Debug.Log("already top");
			return;
		}
		
		//1. swap
		SwapLayer(currentIndex,currentIndex+1);
		/*Layer currentLayer = mLayers[currentIndex];
		mLayers.RemoveAt(currentIndex);
		mLayers.Insert(currentIndex+1,currentLayer);
		*/
		
		//2. sort
		SortLayer(currentIndex);
		SortLayer(currentIndex+1);
	}
	
	//将指定层向下移动一层,如果已经是最底层 不会再移动//
	public void MoveDownLayer(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		
		int currentIndex = NameToLayer(name);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + name +  ") is not in the LayerManager");
			return;
		}
		
		if(currentIndex == 0)
		{
			Debug.Log("already bottom");
			return;
		}
		
		//1. swap
		/*
		Layer currentLayer = mLayers[currentIndex];
		mLayers.RemoveAt(currentIndex);
		mLayers.Insert(currentIndex-1,currentLayer);
		*/
		SwapLayer (currentIndex,currentIndex-1);
		
		//2. sort
		SortLayer(currentIndex);
		SortLayer(currentIndex-1);
	}
	
	//将指定层移动到顶层//
	public void MoveTopLayer(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		
		int currentIndex = NameToLayer(name);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + name +  ") is not in the LayerManager");
			return;
		}
		
		if(currentIndex == mLayers.Count-1)
		{
			Debug.Log("already top");
			return;
		}
		
		//1.swap
		SwapLayer(currentIndex,mLayers.Count-1);
		
		//2.sort
		SortLayer(currentIndex);
		SortLayer(mLayers.Count-1);
		
	}
	
	//将指定层移动到底层//
	public void MoveBottomLayer(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		
		int currentIndex = NameToLayer(name);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + name +  ") is not in the LayerManager");
			return;
		}
		
		if(currentIndex == 0)
		{
			Debug.Log("already bottom");
			return;
		}
		
		//1.swap
		SwapLayer (currentIndex,0);
		
		//2.sort
		SortLayer(currentIndex);
		SortLayer(0);
	}
	#endregion
	
	
	#region Layer->Panel
	
	//add the gameobject to layer
	
	public void AddPanel(string layerName, GameObject panel)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		if(panel.GetComponent<UIPanel>() == null)
		{
			Debug.LogWarning ("the panel object havn't a UIPanel.");
			panel.AddComponent<UIPanel>().depth = 0;
			//return;
		}
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Layer curLayer = mLayers[currentIndex];
			objs = curLayer.panels = new List<GameObject>(10);
			mLayers[currentIndex] = curLayer;
		}
		else
		{
			int foundIndex = objs.IndexOf(panel);
			if(foundIndex != -1)
			{
				//Debug.LogError ("the panel already exists, the panel's name = " + panel.name);
				return;
			}
		}
		
		objs.Add(panel);
		
		SortPanel(objs,currentIndex,objs.Count-1,-1);
	}
	
	public void RemovePanel(string layerName, GameObject panel)
	{
		
		//检查有效性//
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		//查找panel//
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Debug.LogError ("objs == null");
			return;
		}
		
		int foundIndex = objs.IndexOf(panel);
		
		if(foundIndex == -1) return;
		
		
		//删除当前panel//
		objs.RemoveAt(foundIndex);
		
		//重新排列之后的界面//
		SortPanels(objs,currentIndex,foundIndex,objs.Count-foundIndex);
	}
	
	
	public void InsertPanel(string layerName,GameObject panel, int index)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		if(panel.GetComponent<UIPanel>() == null)
		{
			Debug.LogError ("the panel object havn't a UIPanel.");
			return;
		}
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Layer curLayer = mLayers[currentIndex];
			objs = curLayer.panels = new List<GameObject>(10);
			mLayers[currentIndex] = curLayer;
		}
		else
		{
			int foundIndex = objs.IndexOf(panel);
			if(foundIndex != -1)
			{
				//Debug.LogError ("the panel alreasy exists, the panel's name = " + panel.name);
				return;
			}
		}
		
		if(index < 0) index = 0;
		
		if(index > objs.Count) index = objs.Count;
		
		
		objs.Insert(index, panel);
		
		SortPanels(objs,currentIndex,index,objs.Count-index);
	}
	
	//向上移动//
	public void MoveUpPanel(string layerName,GameObject panel)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Debug.LogError ("objs == null");
			return;
		}
		
		int foundIndex = objs.IndexOf(panel);
		
		if(foundIndex == -1) return;
		
		if(foundIndex == objs.Count-1)
		{
			Debug.Log ("already top panel");
			return;
		}
		
		
		SwapPanel(objs, foundIndex, foundIndex+1);
		
		SortPanels(objs,currentIndex,foundIndex,2);
		
	}
	
	//向下移动//
	public void MoveDownPanel(string layerName,GameObject panel)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Debug.LogError ("objs == null");
			return;
		}
		
		int foundIndex = objs.IndexOf(panel);
		
		if(foundIndex == -1) return;
		
		if(foundIndex == 0)
		{
			Debug.Log ("already bottom panel");
			return;
		}
		
		SwapPanel(objs, foundIndex, foundIndex-1);
		
		SortPanels(objs,currentIndex,foundIndex-1,2);
	}
	
	//移动到顶部//
	public void MoveTopPanel(string layerName,GameObject panel)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Debug.LogError ("objs == null");
			return;
		}
		
		int foundIndex = objs.IndexOf(panel);
		
		if(foundIndex == -1) return;
		
		if(foundIndex == objs.Count-1)
		{
			Debug.Log ("already top panel");
			return;
		}
		
		SwapPanel(objs, foundIndex, objs.Count-1);
		
		SortPanels(objs,currentIndex,foundIndex,objs.Count-foundIndex);
	}
	
	//移动到底部//
	public void MoveBottomPanel(string layerName,GameObject panel)
	{
		if(string.IsNullOrEmpty(layerName)) return;
		
		int currentIndex = NameToLayer(layerName);
		
		if(currentIndex == -1)
		{
			Debug.LogError ("the layer(" + layerName +  ") is not in the LayerManager");
			return;
		}
		
		if(panel == null)
		{
			Debug.LogError ("panel is null");
			return;
		}
		
		
		List<GameObject> objs = null;
		
		objs = mLayers[currentIndex].panels;
		
		if(objs == null)
		{
			Debug.LogError ("objs == null");
			return;
		}
		
		int foundIndex = objs.IndexOf(panel);
		
		if(foundIndex == -1) return;
		
		if(foundIndex == 0)
		{
			Debug.Log ("already bottom panel");
			return;
		}
		
		SwapPanel(objs, foundIndex, 0);
		
		SortPanels(objs,currentIndex,0,foundIndex+1);
	}
	
	#endregion
	
	
	#region private
	int LayerCount()
	{
		return mLayers != null ? mLayers.Count : 0;
	}
	
	int NameToLayer(string layerName)
	{
		//if(mLayers == null) return -1;
		
		for(int i = 0 , imax = mLayers.Count; i < imax ; ++i )
		{
			if(mLayers[i].name == layerName) return i;
		}
		
		return -1;
	}
	
	
	string LayerToName(int index)
	{
		if(index < 0 || index >= LayerCount())
			return null;
		return mLayers[index].name;
	}
	
	
	//编辑器做判断, 真机不进行判断是否存在//
	//你要确保在编辑器下不出警告, 真机就不会出现异常//
	bool LayerContains(string name)
	{
		#if UNITY_EDITOR
		for(int i = 0, imax = mLayers.Count; i < imax ; ++i)
		{
			if(mLayers[i].name == name)
			{
				Debug.LogError("have a repeated layer. the name of the layer is " + name);
				return true;
			}
		}
		#endif
		return false;
	}
	
	
	void SortLayers(int startIndex, int length)
	{
		if(length <=0) return;
		
		
		
		for(int i = startIndex; length > 0; ++i,--length)
		{
			
			SortLayer(i);
			
		}
		
	}
	
	void SortLayer(int index)
	{
		List<GameObject> objs = null;
		objs = mLayers[index].panels;
		if(objs == null) return;
		SortPanels(objs,index,0,objs.Count);
	}
	
	void SortPanels(List<GameObject> objs, int layerIndex, int startIndex, int length)
	{
		if(length <= 0) return;
		
		int depth = -1;
		
		for(int i = startIndex; length > 0; ++i,--length)
		{
			depth = SortPanel(objs,layerIndex,i,depth);
		}
		
	}
	
	//defaultDepth为-1的时候,需要自己找前面的元素计算//
	//返回下一个深度//
	int SortPanel(List<GameObject> objs , int layerIndex, int index, int defaultDepth)
	{
		GameObject panel = objs[index];
		
		int depth = defaultDepth;
		
		//选择深度,如果等于-1, 那么就要看前面是否可以拿到布过局的UIPanels,然后拿到next depth//
		if(depth == -1)
		{
			if(index > 0)
			{
				depth = GetGameObjectDepth(objs[index-1]);
			}
			else
			{
				depth = layerIndex * Layer_Distance - 1;
			}
		}
		
		//所能容纳的最大depth值,默认每个层有1000,0-999.//
		int max = layerIndex * Layer_Distance + Layer_Distance - 1;
		
		
		//获取到当前object的所有panels//
		UIPanel[] allPanels = panel.GetComponentsInChildren<UIPanel>(true);
		//将所有的Panel按depth进行排序//
		System.Array.Sort(allPanels, CompareFunc);
		
		//设置新的panel值//
		for(int i = 0, imax = allPanels.Length; i < imax; ++i)
		{
			//这里不会超过max,是因为每一层所限制的Panel depth最大值//
			//如果不够用,调整Layer_Distance//
			depth = Mathf.Min(depth+1, max);
			allPanels[i].depth = depth;
		}
		
		return depth;
	}
	
	void SwapLayer(int from, int to)
	{
		if(from == to) return;
		
		Layer fromLayer = mLayers[from];
		mLayers[from] = mLayers[to];
		mLayers[to] = fromLayer;
	}
	
	void SwapPanel(List<GameObject> objs, int from, int to)
	{
		if(from == to) return;
		
		GameObject fromPanel = objs[from];
		objs[from] = objs[to];
		objs[to] = fromPanel;               
	}
	
	int GetGameObjectDepth(GameObject go)
	{
		UIPanel[] allPanels = go.GetComponentsInChildren<UIPanel>(true);
		int highest = int.MinValue;
		for(int i = 0, imax = allPanels.Length; i < imax; ++i)
		{
			highest = Mathf.Max(highest, allPanels[i].depth);
		}
		return (highest == int.MinValue) ? 0 : highest;
	}
	
	
	static public int CompareFunc (UIPanel a, UIPanel b)
	{
		if (a != b && a != null && b != null)
		{
			if (a.depth < b.depth) return -1;
			if (a.depth > b.depth) return 1;
		}
		return 0;
	}
	
	#endregion
	
}