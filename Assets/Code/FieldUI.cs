using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldUI : MonoBehaviour {


	public UnityEngine.GameObject pileUIPrefab = null;
	UnityEngine.UI.GridLayoutGroup gridLayoutGroup;
	List<UnityEngine.GameObject> freeBlocks = new List<UnityEngine.GameObject>();
	
	void Awake(){
		gridLayoutGroup = this.transform.GetComponent<UnityEngine.UI.GridLayoutGroup>();
	}

	// Use this for initialization
	void Start () {
		GameStateManager.instance.OnStartRound.Sub(this.gameObject,onStartRoundHandler);
		GameStateManager.instance.OnEndRound.Sub(this.gameObject,onEndRoundHandler);
		GameStateManager.instance.OnCreatePile.Sub(this.gameObject,refreshHandler);
		GameStateManager.instance.OnRemovePile.Sub(this.gameObject,refreshHandler);
	}

	void onStartRoundHandler(UnityEngine.GameObject self, System.Object data){
		int childCount = this.transform.childCount;
		for(int i = 0; i < this.transform.childCount; ++i){
			this.transform.GetChild(i).gameObject.SetActive(false);
		}
	}
	void onEndRoundHandler(UnityEngine.GameObject self, System.Object data){
		// do nothing??
	}	
	void refreshHandler(UnityEngine.GameObject self, System.Object data){
		refresh();
	}
	
	void refresh(){
		// first turn off all the children
		int childCount = this.transform.childCount;
		for(int i = 0; i < childCount; ++i){
			this.transform.GetChild(i).gameObject.SetActive(false);
		}

		// make enough blocks to display all the piles
		for(int i = 0; i < GameStateManager.instance.piles.Count - childCount; ++i){
			getFreeBlock();
		}

		// for each of piles use one of the blocks
		PlayerModel p = GameStateManager.instance.GetPlayerModel();
		int pileCount = 0;
		foreach ( KeyValuePair<int,Pile> entry in GameStateManager.instance.piles){
			UnityEngine.GameObject block = this.transform.GetChild(pileCount).gameObject;
			block.SetActive(true);

			PileUI ui = block.GetComponent<PileUI>();
			ui.Setup(entry.Value);

			pileCount++;
		}
	}
	
	private GameObject getFreeBlock(){
		if( freeBlocks.Count == 0){
			for( int i = 0;i < 4; ++i){
				UnityEngine.GameObject block = (Instantiate(pileUIPrefab) as GameObject);
				block.transform.SetParent(this.transform,false);
				block.SetActive (false);
				freeBlocks.Add(block);
			}
		}
		UnityEngine.GameObject b = freeBlocks[0];
		freeBlocks.RemoveAt(0);
		return b;
	}
	private void returnFreeBlock(UnityEngine.GameObject b){
		freeBlocks.Add(b);
	}
}
