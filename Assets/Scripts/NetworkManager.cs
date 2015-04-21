using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	private List<HostData> serverList;

	
	private const string typeName = "BabySimulator";
	private const string gameName = "Game";
	private bool initialized;

	public static NetworkManager networkManager;
	public static NetworkView nv;
	public int maxPeople = 25;

	public static int globalPlayersId = 0;
	public static List<BabyInfo> playersInfo;
	private int networkID = -1;

	void Start () 
	{
		playersInfo = new List<BabyInfo>();

		serverList = new List<HostData>();
		initialized = false;

		nv = GetComponent<NetworkView>();
		nv.observed = this;
		networkManager = this;
		
		RefreshServerList();
	}

	void StartServer()
	{
		Network.InitializeServer(maxPeople, 25001, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}
	
	void OnServerInitialized()
	{
		OnPlayerConnected(Network.player);
	}

	private void RefreshServerList()
	{
		MasterServer.RequestHostList(typeName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if(initialized) return;

		if (msEvent == MasterServerEvent.HostListReceived)
		{
			serverList.Clear();
			serverList.AddRange(MasterServer.PollHostList());
			if(serverList.Count >= 1)
			{
				int randomServer = Random.Range(0, serverList.Count-1);
				JoinServer(serverList[randomServer]);
			}
			else
			{
				StartServer();
			}

			initialized = true;
		}
	}
	
	
	void Update()
	{
		if(!initialized) RefreshServerList();

		//SendPositionToOthers();
	}

	public static void SendPositionToOthers()
	{
		//nv.RPC("ReceivePositions", RPCMode.Others, Core.babyMe.networkId, Core.babyMe.transform.position, Core.babyMe.transform.rotation);
	}

	[RPC]
	public void OnNewPlayerConnected(int newPlayerId) 
	{
		Debug.Log("OnNewPlayerConnected()");
		
		//Lo instanciamos
		GameObject baby = Instantiate(Resources.Load ("Baby")) as GameObject;
		Core.babies.Add(baby);
		//
		
		if(networkID != -1)
		{
			//Lo anadimos a la lista de info de jugadores
			BabyInfo bi = new BabyInfo();
			bi.networkId = playersInfo.Count;
			playersInfo.Add(bi);
			//
		}
		else
		{
			networkID = newPlayerId;
			Core.babyMe = baby;
			Camera.main.GetComponent<CameraControl>().babyTransform = baby.transform;
		}
	}
	
	[RPC]
	public void ReceiveCurrentPlayers(int playerId, Vector3 newPlayerPosition, Quaternion newPlayerRotation) 
	{
		Debug.Log("ReceiveCurrentPlayers()");

		//Lo instanciamos
		GameObject baby = Instantiate(Resources.Load ("Baby")) as GameObject;
		Core.babies.Add(baby);
		//
		
		//Lo anadimos a la lista de info de jugadores
		BabyInfo bi = new BabyInfo();
		bi.networkId = playersInfo.Count;
		playersInfo.Add(bi);
		//
	}
	
	[RPC]
	public void ReceivePositions(int newPlayerId, Vector3 newPlayerPosition, Quaternion newPlayerRotation) 
	{
		Debug.Log("ReceivePositions()");

		//Afegeixo al baby
		GameObject go = (GameObject)( Instantiate(Resources.Load("Baby")) );
		Core.babies.Add(go);
		
		playersInfo[ playersInfo.Count - 1 ].networkId = newPlayerId;
		
		if (networkID == -1)
		{
			//Soc el nou conectat
			Debug.Log("Im the new connected");
			networkID = newPlayerId; //Pillo la nueva id
			Core.babyMe = go;		 //soy yo
		}
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
		Debug.Log ("JoinServer()");
	}
	
	void OnConnectedToServer()
	{
		Debug.Log("Joined to server");
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		if (Network.isServer)
		{
			Debug.Log("Detected a new player connected (ServerSide)");

			++globalPlayersId;
			for(int i = 0; i < playersInfo.Count; ++i)
			{
				GameObject baby = Core.babies[i];
				nv.RPC("ReceiveCurrentPlayers", player, playersInfo[i].networkId, 
				       							baby.transform.position, 
				       							baby.transform.rotation); //Enviamos los anteriores al player que acaba de entrar
			}

			nv.RPC("OnNewPlayerConnected", RPCMode.All, globalPlayersId);
		}
	}
}