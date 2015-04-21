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
	private static int networkID = -1;

	void Start () 
	{

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

	public static void SendPositionToOthers(GameObject baby)
	{
		nv.RPC("ReceivePositions", RPCMode.Others, networkID, baby.transform.position, baby.transform.rotation);
	}

	[RPC]
	public void OnNewPlayerConnected(int newPlayerId) 
	{
		Debug.Log("OnNewPlayerConnected()");
		
		//Lo instanciamos
		GameObject baby = Instantiate(Resources.Load ("Baby")) as GameObject;
		Core.babies.Add(baby);
		//
		
		if(networkID == -1)
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

		if(playerId == networkID || playerId == -1) return;

		//Lo instanciamos
		GameObject baby = Instantiate(Resources.Load ("Baby")) as GameObject;
		baby.GetComponent<Baby>().networkId = playerId;
		baby.transform.position = newPlayerPosition;
		baby.transform.rotation = newPlayerRotation;
		Core.babies.Add(baby);
		//
		
		//Lo anadimos a la lista de info de jugadores
		/*BabyInfo bi = new BabyInfo();
		bi.networkId = playersInfo.Count;
		playersInfo.Add(bi);*/
		//
	}
	
	[RPC]
	public void ReceivePositions(int playerId, Vector3 newPlayerPosition, Quaternion newPlayerRotation) 
	{
		Debug.Log("ReceivePositions()");

		for (int i = 0; i < Core.babies.Count; ++i) 
		{
			GameObject baby = Core.babies[i];
			if (baby.GetComponent<Baby>().networkId == playerId) 
			{
				baby.transform.position = newPlayerPosition;
				baby.transform.rotation = newPlayerRotation;
			}
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
			nv.RPC("OnNewPlayerConnected", RPCMode.All, globalPlayersId);

			for(int i = 0; i < Core.babies.Count; ++i)
			{
				GameObject baby = Core.babies[i];
				nv.RPC("ReceiveCurrentPlayers", player, baby.GetComponent<Baby>().networkId, 
				       							baby.transform.position, 
				       							baby.transform.rotation); //Enviamos los anteriores al player que acaba de entrar
			}


		}
	}
}