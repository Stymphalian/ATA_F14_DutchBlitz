using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	public int serverPortRangeMin = 32000;
	public int serverPortRangeMax = 33000;
	public int serverCreateAttempts = 10;
	public int clientJoinAttempts = 10;
	public bool autoConnect= false;

	public delegate void OnPlay();
	private bool shouldConnect = false;
	private OnPlay onPlayCallback = null;
	
	public static NetworkManager instance;
	void Awake(){
		NetworkManager.instance = this;
	}
	
	void Start(){
		if( autoConnect && Network.isServer == false){
			Play(_startDefaultGame);
		}
	}
	private void _startDefaultGame(){
		Application.LoadLevel("instance");
	}
	
	public void Play(OnPlay callback){
		Debug.Log ("Play");
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("jordanGame");
		shouldConnect = true;
		onPlayCallback = callback;
	}
	private void _Play(){
		Debug.Log ("_Play");
		shouldConnect = false;

		HostData[] hostData = MasterServer.PollHostList();			
		bool hostFound = false;
		for( int i = 0;i < hostData.Length; ++i){		
			Debug.Log("Game name : " + hostData[i].gameName);
			Debug.Log("connectedPlayers : " + hostData[i].connectedPlayers);
			Debug.Log("PlayerLimit : " + hostData[i].playerLimit);

			if(hostData[i].connectedPlayers < GameStateManager.MAX_NUM_PLAYERS &&
			   hostData[i].playerLimit > 1 )
			{
				hostFound = true;
				joinServer(hostData[i]);
				break;
			}
		}

		if(hostFound == false){
			startNewServer();
		}

		// use the callback and do shit.
		if( onPlayCallback != null){
			onPlayCallback();
		}

	}
	private void startNewServer(){
		Debug.Log ("starting new server");
		Application.runInBackground = true;
		bool useNat = !Network.HavePublicAddress();

		int port = UnityEngine.Random.Range(serverPortRangeMin,serverPortRangeMax);
		int attempts = serverCreateAttempts;

		while( attempts > 0){
			NetworkConnectionError e =  Network.InitializeServer(32,port,useNat);
			if(e == NetworkConnectionError.NoError){
				MasterServer.RegisterHost("jordanGame","dutchBlitz","Here is a comment");
				break;
			}else{
				// try again with new ports?
				port = UnityEngine.Random.Range(serverPortRangeMin,serverPortRangeMax);
				attempts -= 1;
			}
		}
	}

	private void joinServer(HostData hostData){
		Debug.Log ("Joining Server " + hostData.gameName + " " + hostData.gameType + " " + hostData.ip+":"+ hostData.port);
		int attempts = clientJoinAttempts;
		while(attempts > 0){
			attempts -= 1;

			NetworkConnectionError e = Network.Connect(hostData);
			if( e == NetworkConnectionError.NoError){
				break;
			}
		}
	}


	// -------------------------
	// monobehaviour On[something] methods
	// -------------------------
	void OnApplicationQuit(){
		Debug.Log("OnApplicationQuit");
		Network.Disconnect();
		if(Network.isServer){
			MasterServer.UnregisterHost();
		}
	}

	void OnConnectedToServer(){

	}

	void OnDisconnectedFromServer(){
		Debug.Log ("You have been disconnected from the server");
		if( Network.isClient){
			Notification.instance.Message("Disconnected from server",-1);
			StartCoroutine(quitAfterSeconds(3));
		}
	}
	private IEnumerator quitAfterSeconds(float seconds){
		yield return new WaitForSeconds(seconds);
		Application.Quit ();
	}

	void OnFailedToConnect(){

	}
	
	void OnPlayerConnected(NetworkPlayer player){

	}
	void OnPlayerDisconnected(NetworkPlayer player){
		Debug.Log ("player Disconnected ");
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects(player);
	}

	void OnServerInitialized(){
		Debug.Log ("Server Initialized");
	}
	
	void OnMasterServerEvent(MasterServerEvent ev){
		if( ev == MasterServerEvent.HostListReceived){
			Debug.Log ("MasterServerEvent");
			if( shouldConnect){
				_Play();
			}
		}
	}
}
