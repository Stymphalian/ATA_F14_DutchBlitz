using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainDeckUI : MonoBehaviour {

	List<UnityEngine.UI.Button> buttons= new List<UnityEngine.UI.Button>();
	List<UnityEngine.UI.Image> buttonImages = new List<UnityEngine.UI.Image>();
	List<UnityEngine.UI.Text> buttonTexts = new List<UnityEngine.UI.Text>();

	void Awake(){
		for( int i =0;i < 4; ++i){
			buttons.Add(this.transform.Find ("Button"+i).GetComponent<UnityEngine.UI.Button>());
			buttonImages.Add(this.transform.Find ("Button"+i).GetComponent<UnityEngine.UI.Image>());
			buttonTexts.Add(this.transform.Find ("Button"+i+"/Text").GetComponent<UnityEngine.UI.Text>());

			buttons[i].onClick.RemoveAllListeners();
			buttons[i].onClick.AddListener(GetToPileClickHandler(i));
		}
	}

	void Start(){
		GameStateManager.instance.OnStartRound.Sub(this.gameObject,onStartRoundHandler);
		GameStateManager.instance.OnEndRound.Sub(this.gameObject,onEndRoundHandler);
		GameStateManager.instance.OnMainToPile.Sub(this.gameObject,onMainToPileHandler);
	}

	void refresh(){
		PlayerModel player = GameStateManager.instance.GetPlayerModel();
		for(int i = 0;i < 4; ++i){
			if(i >= player.mainDeck.Count){
				buttons[i].gameObject.SetActive(false);
			}else{
				Card c = player.mainDeck[i];

				// TODO: Make a generic card prefab and monobehavior script which can display
				// a card in general
				buttons[i].gameObject.SetActive(true);
				buttonTexts[i].text = c.number.ToString();
				buttonImages[i].color = c.GetColor();
			}
		}
	}

	// add to the pending list.
//	private List<int> pendingToPileList = new List<int>();
	private UnityEngine.Events.UnityAction GetToPileClickHandler(int index){
		return delegate(){
			PlayerModel player = GameStateManager.instance.GetPlayerModel();
			if( index < 0 || index >= player.mainDeck.Count){return;}
			Card c = player.mainDeck[index];

			// HACK find an appropriate pile
			int pileId = -1;
			foreach( KeyValuePair<int,Pile> entry in GameStateManager.instance.piles){
				if(entry.Value.IsNext(c)){
					pileId = entry.Key;
					break;
				}
			}
			// can't find a pile and the card we are adding won't create a new pile
			if(pileId == -1 && c.number != Card.MIN_NUM){return;}			

			if( c.number == Card.MIN_NUM){
				GameStateManager.instance.CreatePile(player.playerId,c.number,(int)c.cardType,index);
			}else{
				GameStateManager.instance.MainToPile(player.playerId,index,pileId);
			}

//			pendingToPileList.Add(index);
		};
	}


	void onStartRoundHandler(UnityEngine.GameObject self,System.Object data){
		refresh();
	}
	
	void onEndRoundHandler(UnityEngine.GameObject self,System.Object data){
		for(int i = 0;i < 4; ++i){
			buttons[i].gameObject.SetActive(false);
		}
	}
	
	void onMainToPileHandler(UnityEngine.GameObject self, System.Object data){
		GameStateManager.GameStateEvent ev = (GameStateManager.GameStateEvent)(data);
		if(ev.playerId != PlayerModel.GetPlayerId()){return;}
		
		// TODO: we should probably have some confirmation 
		// that this is the onDiscard for the one we wanted.. but whatevs.
		// see if we can remove our card from the top of the discard pile.
//		if( pendingToPileList.Count > 0){
//			PlayerModel player = GameStateManager.instance.GetPlayerModel();
//			player.mainDeck.RemoveAt(pendingToPileList[0]);
//			pendingToPileList.Remove(0);
//		}
				
		// refresh the discard pile.
		refresh();
	}
}

