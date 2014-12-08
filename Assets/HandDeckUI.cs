using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandDeckUI : MonoBehaviour {
	
	UnityEngine.UI.Button discardButton;
	UnityEngine.GameObject discardPile;
	UnityEngine.UI.Text card1Text;
	UnityEngine.UI.Image card1Image;
	UnityEngine.UI.Button deckButton;
	UnityEngine.UI.Text deckButtonText;
	UnityEngine.UI.Button zeroCardsButton;
	
	void Awake(){
		discardPile = this.transform.Find ("DiscardPile").gameObject;
		discardButton = this.transform.Find("DiscardPile/Button").GetComponent<UnityEngine.UI.Button>();
		card1Text = this.transform.Find("DiscardPile/Card1/Text").GetComponent<UnityEngine.UI.Text>();
		card1Image = this.transform.Find("DiscardPile/Card1/Image").GetComponent<UnityEngine.UI.Image>();
		deckButton = this.transform.Find("DeckButton").GetComponent<UnityEngine.UI.Button>();
		deckButtonText = this.transform.Find("DeckButton/Text").GetComponent<UnityEngine.UI.Text>();
		zeroCardsButton = this.transform.Find("ZeroCardsButton").GetComponent<UnityEngine.UI.Button>();

		discardButton.onClick.AddListener(toPileHandler);
		deckButton.onClick.AddListener(toDiscardHandler);
	}

	// this action doesn't need to go through the server to be validated
	// therefore we can modify as much shit as we want.
	private void toDiscardHandler(){
		PlayerModel player = GameStateManager.instance.GetPlayerModel();
		player.MoveToDiscard(PlayerModel.MAIN_TO_DISCARD_SIZE); // in this case 3

		if( player.handDeck.Count == 0){
			if(player.discardDeck.Count > 0){			
				// no more cards in the hand, but still have some in the discard pile
				// therefore setup the button to allow the player to reshuffle.
				deckButton.onClick.RemoveListener(toDiscardHandler);
				deckButton.onClick.RemoveListener(reshuffleHandler);
				deckButton.onClick.AddListener(reshuffleHandler);
			}
		}

		refresh();
	}
	private void reshuffleHandler(){
		PlayerModel player = GameStateManager.instance.GetPlayerModel();
		player.ReshuffleDiscardIntoHand();

		// set the correct button click handlers
		deckButton.onClick.RemoveListener(reshuffleHandler);
		deckButton.onClick.RemoveListener(toDiscardHandler);
		deckButton.onClick.AddListener(toDiscardHandler);

		// refresh the ui
		refresh();
	}

	// this call must go through the server, therfore
	// we shouldn't modify the model, we must leave that to a callback.
	private List<int> pendingToPileList = new List<int>();
	private void toPileHandler(){
		PlayerModel player = GameStateManager.instance.GetPlayerModel();

		if( player.discardDeck.Count == 0){return;}
		Card c = player.discardDeck[0];

		// HACK for now just auto find the pile we want to add to
		int pileId = -1;
		foreach( KeyValuePair<int,Pile> entry in GameStateManager.instance.piles){
			if(entry.Value.IsNext(c)){
				pileId = entry.Key;
				break;
			}
		}
		// can't find a pile and the care we are adding won't create a new pile
		if(pileId == -1 && c.number != Card.MIN_NUM){return;}

		if( c.number == Card.MIN_NUM){
			GameStateManager.instance.CreatePile(player.playerId,c.number,(int)c.cardType,-1);
		}else{
			GameStateManager.instance.DiscardToPile(player.playerId,c.number,(int)c.cardType,pileId);
		}

		// add to the pending list.
		pendingToPileList.Add(pileId);
	}
	
	private void refresh(){
		PlayerModel player = GameStateManager.instance.GetPlayerModel();

		// show the discard pile
		if( player.discardDeck.Count == 0 ){
			discardPile.SetActive(false);
		}else{
			discardPile.SetActive(true);
			card1Text.text = player.discardDeck[0].number.ToString();
			card1Image.color = player.discardDeck[0].GetColor();
		}


		deckButton.gameObject.SetActive(true);
		if(player.handDeck.Count == 0){
			// no more cards in the hand deck
			if(player.discardDeck.Count == 0){
				// also no more cards in the discard pile.. therefore
				// show that they no longer have any more cards.
				deckButton.gameObject.SetActive(false);
				zeroCardsButton.gameObject.SetActive(true);				
			}else{
				// still have cards in the discard pile
				deckButtonText.text = "RESHUFFLE";
			}
		}else{
			deckButtonText.text = "DECK " + player.handDeck.Count;
		}
	}

	// Use this for initialization
	void Start () {
		GameStateManager.instance.OnStartRound.Sub(this.gameObject,onStartRoundHandler);
		GameStateManager.instance.OnEndRound.Sub(this.gameObject,onEndRoundHandler);
		GameStateManager.instance.OnDiscardToPile.Sub(this.gameObject,onDiscardToPileHandler);
	}

	void onStartRoundHandler(UnityEngine.GameObject self,System.Object data){
		discardButton.interactable = true;
		deckButton.interactable = true;
		zeroCardsButton.interactable = false;

		// discardPile is initially empty.
		discardPile.SetActive(false);

		// reset all the button handler
		deckButton.onClick.RemoveAllListeners();
		zeroCardsButton.onClick.RemoveAllListeners();
		discardButton.onClick.RemoveAllListeners();
		discardButton.onClick.AddListener(toPileHandler);
		deckButton.onClick.AddListener(toDiscardHandler);
					
		refresh();
	}

	void onEndRoundHandler(UnityEngine.GameObject self,System.Object data){
		discardButton.interactable = false;
		deckButton.interactable = false;
		zeroCardsButton.interactable = false;
	}

	void onDiscardToPileHandler(UnityEngine.GameObject self, System.Object data){
		GameStateManager.GameStateEvent ev = (GameStateManager.GameStateEvent)(data);
		if(ev.playerId != PlayerModel.GetPlayerId()){return;}

		// TODO: we should probably have some confirmation 
		// that this is the onDiscard for the one we wanted.. but whatevs.
		// see if we can remove our card from the top of the discard pile.
		PlayerModel player = GameStateManager.instance.GetPlayerModel();
		player.discardDeck.RemoveAt(0);

		// refresh the discard pile.
		refresh();
	}

}
