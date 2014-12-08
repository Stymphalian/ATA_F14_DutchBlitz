using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameStateManager : MonoBehaviour {
	public const int MAX_NUM_PLAYERS = 4;
	public const int TARGET_SCORE = 100;

	public Dictionary<int,PlayerModel> playerModels = new Dictionary<int,PlayerModel>();
	public Dictionary<int,Pile> piles = new Dictionary<int,Pile>();
	public int roundCount = 0;
	
	public static GameStateManager instance;
	void Awake(){
		GameStateManager.instance = this;

		// if we are the server, we need to setup a player for ourselves..
		if(Network.isServer){
			PlayerModel p = new PlayerModel();
			p.LoadPlayerFromPrefs();
			joinPlayer(p.playerId,p.playerName);
		}
	}



	public class GameStateEvent{
		public GameStateEvent(int id,System.Object d){
			_playerId = id;
			_data = d;
		}

		protected int _playerId;
		protected System.Object _data;
		public int playerId {get{return _playerId;}}
		public System.Object data {get{return _data;}}
	}
	public EventSubscriber OnStartGame = new EventSubscriber();
	public EventSubscriber OnEndGame = new EventSubscriber();
	public EventSubscriber OnStartRound = new EventSubscriber();
	public EventSubscriber OnEndRound = new EventSubscriber();
	public EventSubscriber OnCreatePile = new EventSubscriber();
	public EventSubscriber OnRemovePile = new EventSubscriber();
	public EventSubscriber OnMainToPile = new EventSubscriber();
	public EventSubscriber OnDiscardToPile = new EventSubscriber();
	private bool roundLock= true;

	[RPC]
	public void StartGame(){
		if(Network.isServer){
			// prevent anyone from connection after the game starts
			Network.maxConnections = 0;
			networkView.RPC("StartGame",RPCMode.Others);
		}
		OnStartGame.Publish(null);
//		StartRound(1);
	}

	[RPC]
	public void StartRound(int round){
		if(Network.isServer){
			// for every player we do a setup.
			System.Random r = new System.Random();
			foreach( KeyValuePair<int,PlayerModel> entry in playerModels){
				int rand = r.Next();
				ResetPlayerForRound(entry.Key,rand);
			}

			networkView.RPC ("StartRound",RPCMode.Others,round);
		}

		roundCount = round;
		roundLock = false;
		OnStartRound.Publish(null);
	}

	[RPC]
	public void EndRound(){
		roundLock = true;

		// calculate the final scores for all the players
		foreach(KeyValuePair<int,PlayerModel> entry in playerModels){
			int pointsForRound = entry.Value.PointsForRound();
			entry.Value.currentScore += pointsForRound;
			// do some display layer stuff...
		}

		// push out an event that the round has ended.
		OnEndRound.Publish(null);

		if( Network.isServer){
			networkView.RPC("EndRound",RPCMode.Others);

			// do some display layer stuff...
			
			// calculate any possible winners.
			List<PlayerModel> winners = new List<PlayerModel>();
			foreach(KeyValuePair<int,PlayerModel> entry in playerModels){
				if( entry.Value.currentScore >= TARGET_SCORE){
					if( winners.Count != 0){
						if( entry.Value.currentScore > winners[0].currentScore){
							// hey this entry is bigger than any of the previous winners
							winners.Clear();
							winners.Add(entry.Value);
						}else if(entry.Value.currentScore == winners[0].currentScore){
							winners.Add (entry.Value);
						}
						
					}else{
						// a possible winner.
						winners.Add(entry.Value);
					}
				}
			}
			
			if( winners.Count == 0){
				// start another round
				StartRound(roundCount +1 );
			}else if (winners.Count == 1){
				// one winner, end the game
				EndGame();
			}else if( winners.Count > 2){
				// tie, therefore start another round.
				StartRound(roundCount + 1);
			}
		}
	}

	[RPC]
	public void EndGame(){
		if(Network.isServer){
			networkView.RPC("EndGame",RPCMode.Others);
		}

		OnEndGame.Publish(null);
		Debug.Log ("end game");
		Application.Quit();
	}
	
	[RPC]
	void ResetPlayerForRound(int playerId,int randSeed){
		if(Network.isServer){
			networkView.RPC("ResetPlayerForRound",RPCMode.Others,playerId,randSeed);
		}

		// NOTE: we seed the random number generator in order 
		// to ensure that the reseting for the round 
		// remains consistent across all the clients.
		UnityEngine.Random.seed = randSeed;
		PlayerModel p = playerModels[playerId];
		p.ResetForRound();
	}
	
	[RPC]
	void joinPlayer(int playerId,string name){
		if( Network.isServer){
			networkView.RPC ("joinPlayer",RPCMode.OthersBuffered,playerId,name);
		}

		PlayerModel p = new PlayerModel();
		playerModels[playerId] = p;
		p.playerId = playerId;
		p.playerName = name;
	}



	// -----------------------------
	// methods to manipulate the game. It will only be called by the server
	// as requested by actions from clients.
	// -----------------------------
	private int _nextPileID = 1;
	private int nextPileID {
		get{return _nextPileID++;}
	}


	// fromMainDeck -- this will be -1 if it the pile was being created from a card from the discard pile
	// else it will be the index of the card the main deck which is used to create the pile.
	[RPC]
	protected void createPile(int playerId,int newPileId, int cardNumber, int cardType, int fromMainDeck){
		if( Network.isServer){
			// validation.
			if( cardNumber != Card.MIN_NUM ){return;}
			if( cardType < 0 || cardType >= System.Enum.GetValues(typeof(Card.TypeEnum)).Length ){return;}

			// NOTE: newPileId will be garbage when first called from the server.
			newPileId = nextPileID;
			networkView.RPC ("createPile",RPCMode.Others,playerId,newPileId,cardNumber,cardType,fromMainDeck);
		}

		// create a new pile object and set the top card.
		Pile p = new Pile(newPileId);
		piles[newPileId] = p;

		if( Network.isServer){
			if( fromMainDeck < 0){
				// came from the discard pile
				discardToPile(playerId, cardNumber, cardType,newPileId);
			}else{
				// came from the main deck
				mainToPile(playerId,fromMainDeck,newPileId);
			}
		}

		// tell everyone who needs to know (i.e display layer)
		OnCreatePile.Publish(newPileId);

		if(Network.isClient){
			compareState();
		}
	}
	
	protected void removePile(int pileId){
//		if(Network.isServer){
//			// validate removal?
//			if( piles.ContainsKey(pileId) == false){return;}
//			networkView.RPC("removePile",RPCMode.Others,pileId);
//		}

		// remove the entry from the dict and then tell people..
		piles.Remove(pileId);
		OnRemovePile.Publish(pileId);

		if(Network.isClient){
			compareState();
		}
   	}
		   
	[RPC]
   	protected void mainToPile(int playerId,int cardIndex,int pileId){
		PlayerModel p;
		Card c;
		if( Network.isServer){
			// validation
			// check for valid player id
			if( playerModels.ContainsKey(playerId) == false){return;}
			p = playerModels[playerId];

			// check for valid cardIndex range
			if( cardIndex < 0 || cardIndex >PlayerModel.FIELD_DECK_OUTSIDE_LEN + 1){return;}
			if( cardIndex >= p.mainDeck.Count){return;}
			c = p.mainDeck[cardIndex];

			if( isValidCardPlay(playerId,c.number,(int)c.cardType,pileId) == false){return;}

			networkView.RPC ("mainToPile",RPCMode.Others,playerId,cardIndex,pileId);
		}
		
		p = playerModels[playerId];
		c = p.mainDeck[cardIndex];
		
		p.mainDeckCount -= 1;
		p.fieldScoreCount += 1;
		piles[pileId].topCard = c;
		p.mainDeck.RemoveAt(cardIndex);
		
		OnMainToPile.Publish(new GameStateEvent(playerId,pileId));

		// remove the pile if necessary
		if(piles[pileId].IsComplete() ){
			removePile(pileId);
		}

		if(Network.isClient){
			compareState();
		}
	}

	[RPC]
	protected void discardToPile(int playerId,int cardNumber,int cardType,int pileId){
		PlayerModel p;
		Card c;
		if( Network.isServer){
			// validation
			if( isValidCardPlay(playerId, cardNumber, cardType, pileId) == false){return;}
			networkView.RPC ("discardToPile",RPCMode.Others, playerId,cardNumber,cardType,pileId);
		}
		
		p = playerModels[playerId];
		p.fieldScoreCount += 1;
		c = new Card(cardNumber,(Card.TypeEnum)cardType);
		piles[pileId].topCard = c;

		OnDiscardToPile.Publish(new GameStateEvent(playerId,pileId));

		// remove the pile if necessary
		if(piles[pileId].IsComplete() ){
			removePile(pileId);
		}

		if(Network.isClient){
			compareState();
		}
	}

	// check to see if this is valid move to make.
	protected bool isValidCardPlay(int playerId, int cardNumber, int cardType, int pileId){
		// check for valid player id
		if( playerModels.ContainsKey(playerId) == false){return false;}
//		PlayerModel p = playerModels[playerId];
				

		// check for valid pileId
		if( piles.ContainsKey(pileId) == false){return false;}
		if( piles[pileId].IsComplete () ){return false;}
		// check for ascendeing and same colour
		if( piles[pileId].topCard != null){
			if( cardNumber != piles[pileId].topCard.number +1){return false;}
			if( cardType != (int)piles[pileId].topCard.cardType){return false;}
		}

		return true;
	}

	//-----------------------------
	// client actions
	//-----------------------------
	[RPC]
	public void DiscardToPile(int playerId,int cardNumber,int cardType, int pileId){
		if( roundLock == true){return;}
		if(Network.isClient){
			networkView.RPC ("DiscardToPile",RPCMode.Server,playerId,cardNumber,cardType,pileId);
		}else if(Network.isServer){
			discardToPile(playerId,cardNumber,cardType,pileId);
		}
	}
	[RPC]
	public void MainToPile(int playerId,int cardIndex, int pileId){
		if( roundLock == true){return;}
		if(Network.isClient){
			networkView.RPC ("MainToPile",RPCMode.Server,playerId,cardIndex,pileId);
		}else if(Network.isServer){
			mainToPile(playerId,cardIndex,pileId);
		}
	}
	[RPC]
	public void CreatePile(int playerId, int cardNumber,int cardType,int fromMainDeck){
		if( roundLock == true){return;}
		if(Network.isClient){
			networkView.RPC ("CreatePile",RPCMode.Server,playerId,cardNumber,cardType,fromMainDeck);
		}else if(Network.isServer){
			createPile(playerId,-1,cardNumber,cardType,fromMainDeck);
		}
	}

	//---------------------------------
	// On[something] methods
	//---------------------------------
	// called on the server when a player connects
	void OnPlayerConnected(NetworkPlayer player){
	}
	
	// called on the client when it connects to the server.
	void OnConnectedToServer(){
		PlayerModel p = new PlayerModel();
		p.LoadPlayerFromPrefs();
		networkView.RPC ("joinPlayer",RPCMode.Server,p.playerId,p.playerName);
//		joinPlayer(p.playerId, p.playerName);
	}




	// methods used to compare the state.
	// this is more or less a method to serialize the objects and compare
	//  them with the server..
	public string AsString(){
		string s = "";
		s += "roundCount = " + roundCount + "\n";
		
		foreach(KeyValuePair<int,PlayerModel> entry in playerModels){
			s += entry.Value.AsString()  + "\n";
		}
		
		foreach(KeyValuePair<int,Pile> entry in piles){
			s += entry.Value.AsString()  + "\n";
		}
		return s;
	}

	private void compareState(){
		CompareStateToServer(PlayerModel.GetPlayerId(),AsString());
	}
	[RPC]
	public void CompareStateToServer(int player_id,string client_state){
		if(Network.isClient){
			// send the call to the server to see if they match.
			networkView.RPC("CompareStateToServer",RPCMode.Server,player_id,client_state);

		}else if( Network.isServer){
			// result is printed out on the server
			string server_state = AsString();
			string rs = ( server_state == client_state) ? "good" : "bad";
			Debug.Log ("Comparing Player[" + player_id + "] state with Server state: " + rs);
			if( rs == "bad"){
				Debug.Log ("ServerState : \n" + server_state + "\n");
				Debug.Log ("Client State: \n" + client_state +"\n");
			}

		}
	}


	public PlayerModel GetPlayerModel(){
		return playerModels[PlayerModel.GetPlayerId()];
	}
}
