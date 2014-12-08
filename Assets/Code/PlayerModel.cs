using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// complete model of a player
// this includes the state of their deck and currentScore
// as well as meta informatoin such as player_id, and playerName
// Glossary
// field -- refers to the play area where the Piles are placed
// mainDeck -- the stack of 10 cards + 3 cards which the player is trying to get rid of
// discardPile -- the stack of cards which the player uses to play cards out from their hand. 
// 		must play from the top of the stack downward.
// handDeck -- the deck held in the hand. the cards from here are placed in the discardPile 3 at a time.

public class PlayerModel {
	public const int FIELD_DECK_LEN = 10;
	public const int FIELD_DECK_OUTSIDE_LEN = 3;
	public const int MAIN_TO_DISCARD_SIZE = 3;
	
	// this data is passed around to clients.
	public int playerId = -1;
	public string playerName = "jordan";
	public int currentScore =  0; // total score collected
	public int fieldScoreCount = 0; // keep track of the number of cards in the field

	// how many cards do we have left in the main deck.
	// once this is <= 3, then the player is allowed to call blitz.
	public int mainDeckCount = 0; 

	
	protected Card[] defaultCards;
	public List<Card> mainDeck = new List<Card>(); // 10 cards? try to get rid of all of these cards

	// only reliable if networkView.isMine == true
	public List<Card> discardDeck = new List<Card>(); // pile outside of the hand deck
	public List<Card> handDeck = new List<Card>(); // hand deck is the reserve deck which you can play from
	
	public PlayerModel(){
		// setup the default cards, we do this to make shuffling easy?
		int count = System.Enum.GetValues(typeof(Card.TypeEnum)).Length;
		Card.TypeEnum[] values = (Card.TypeEnum[]) Card.TypeEnum.GetValues(typeof(Card.TypeEnum));
		int cardIndex = 0;
		defaultCards = new Card[count*(Card.MAX_NUM -Card.MIN_NUM + 1)]; // this should be 40 (ie. 4X(10-1+1) )
		for( int i = 0; i < values.Length; ++i){
			for( int j = Card.MIN_NUM; j <= Card.MAX_NUM; ++j){
				defaultCards[cardIndex++] = new Card(j,values[i]);
			}
		}
	}
	
	public void MoveToDiscard(int num){
		for( int i = 0; i < num; ++i){
			if( handDeck.Count == 0){return;}

			// pop of the first element in the deck
			Card c = handDeck[0];
			handDeck.RemoveAt(0);

			// add to the front..
			discardDeck.Insert(0,c);
		}
	}
	
	public void ReshuffleDiscardIntoHand(){
		int count = discardDeck.Count;
		for( int i = count -1; i >= 0; --i){
			handDeck.Add(discardDeck[i]);
		}
		discardDeck.Clear();
	}

	public void ResetForRound(){
		// reset the player model for a new round...
		mainDeck.Clear();
		discardDeck.Clear();
		handDeck.Clear();
		fieldScoreCount = 0;
		// mainDeckCount is reset at the end...
		
		// shuffle the defaultCards
		int defaultCardsLen = defaultCards.Length;
		if( false){
			for( int i = 0; i < defaultCardsLen; ++i){
				int rand = UnityEngine.Random.Range (i,defaultCardsLen);

				// swap the place of the cards.
				Card temp = defaultCards[i];
				defaultCards[i] = defaultCards[rand];
				defaultCards[rand] = temp;
			}
		}
		
		// place the cards into the field deck
		int defaultCardsIndex = 0;
		for (int i = 0; i < FIELD_DECK_LEN + FIELD_DECK_OUTSIDE_LEN; ++i){
			mainDeck.Add(defaultCards[defaultCardsIndex]);
			defaultCardsIndex++;
		}
		
		// place the rest of the cards into the handDeck
		for(int i = defaultCardsIndex; i < defaultCardsLen; ++i){
			handDeck.Add (defaultCards[i]);
		}

		// record how many cards are left in the main deck
		mainDeckCount = mainDeck.Count;
	}

	public int PointsForRound(){
		return fieldScoreCount - 2*mainDeckCount;
	}


	private static int _playerId  = -1;
	public static int GetPlayerId(){
		if( _playerId == -1){
			_playerId = PlayerPrefs.GetInt ("PlayerModel_playerId",-1);
		}
		return _playerId;
	}

	public void LoadPlayerFromPrefs(){
		int playerId = PlayerPrefs.GetInt("PlayerModel_playerId",-1);
		string playerName = PlayerPrefs.GetString("PlayerModel_playerName","Anonymous");
		
		this.playerId = playerId;
		this.playerName = playerName;
	}
	
	public void SaveToPlayerPrefs(){
		PlayerPrefs.SetInt("PlayerModel_playerId",playerId);
		PlayerPrefs.SetString("PlayerModel_playerName",playerName);
	}

	public string AsString(){
		string s ="PlayerModel\n";
		s += "PlayerId = " + playerId + " ,playerName = " + playerName + "\n";
		s += "currentScore = " + currentScore + "\n";
		s += "fieldScoreCount = " + fieldScoreCount + "\n";
		s += "mainDeckCount = " + fieldScoreCount + "\n";
		for( int i = 0; i < defaultCards.Length; ++i){
			s += "defaultCards[" + i + "] = " + defaultCards[i].AsString() + "\n";
		}
		for(int i = 0; i< mainDeck.Count; ++i){
			s += "mainDeck[" + i + "] = " + mainDeck[i].AsString() + "\n";
		}
		return s;
	}
}
