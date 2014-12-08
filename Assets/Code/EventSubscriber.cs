using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class EventSubscriber {
	public delegate void OnEvent(UnityEngine.GameObject self, System.Object data);	
	private Dictionary<UnityEngine.GameObject, OnEvent> subsList = new Dictionary<UnityEngine.GameObject, OnEvent>();
	private List<UnityEngine.GameObject> unsubsList = new List<UnityEngine.GameObject>();

	public void Sub(UnityEngine.GameObject id, OnEvent callback){
		if( id != null && callback != null){
			subsList[id] = callback;
		}
	}

	public void SubOnce(UnityEngine.GameObject id, OnEvent callback){
		if( id != null && callback != null){
			subsList[id] = callback;
			unsubsList.Add(id);
		}
	}

	public void UnSubAfter(UnityEngine.GameObject id){
		if( id != null){
			unsubsList.Add(id);
		}
	}

	public void UnSub(UnityEngine.GameObject id){
		if( subsList.ContainsKey(id) ){
			subsList.Remove(id);
		}
	}

	public void Publish(System.Object data){
		foreach(KeyValuePair<UnityEngine.GameObject,OnEvent> entry in subsList){
			if( entry.Key != null && entry.Value != null){
				entry.Value(entry.Key,data);
			}else{
				unsubsList.Add(entry.Key);
			}
		}

		// unsubscribe all the the shit
		for( int i = 0;i < unsubsList.Count; ++i){
			UnSub(unsubsList[i]);
		}
		unsubsList.Clear ();
	}
}
