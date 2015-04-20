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
		Debug.Log("Server Initializied");
	}

	void Update()
	{
		if(!initialized) RefreshServerList();
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

	public static void SendPositions()
	{
		nv.RPC("AllExplosion", RPCMode.Others);
	}
	
	[RPC]
	public void AllExplosion()
	{
		Debug.Log("ALL EXPLOSIONS");
		foreach(Baby b in Core.babies)
		{
			b.Explode();
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
}