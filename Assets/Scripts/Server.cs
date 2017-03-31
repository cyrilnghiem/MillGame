 using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
//StreamReader uses System.IO
using System.IO;

public class Server : MonoBehaviour 
{
	//open port where nothing usually happens
	public int port = 6321;

	//list of clients
	private List<ServerClient> clients;
	//list of clients to disconnect
	private List<ServerClient> disconnectList;

	private TcpListener server;
	private bool serverStarted;

	public void Init()
	{
		//not destroying it when changing scene
		DontDestroyOnLoad (gameObject);
		clients = new List<ServerClient> ();
		disconnectList = new List<ServerClient> (); 

		try 
		{
			//listening for any connection as long as connected to 6321
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			StartListening();
			serverStarted = true;
		} 

		catch (Exception e) 
		{
			Debug.Log ("Socket error: " + e.Message);
		}
	}

	private void Update()
	{
		if (!serverStarted)
			return;

		foreach(ServerClient c in clients)
		{
			//client still connected?
			if (!IsConnected (c.tcp)) {
				c.tcp.Close ();
				disconnectList.Add (c);
				continue;
			} 

			else 
			{
				NetworkStream s = c.tcp.GetStream ();
				if (s.DataAvailable) {
					StreamReader reader = new StreamReader (s, true);
					string data = reader.ReadLine ();

					//if client sends data
					if (data != null)
						ServerIncomingData (c, data);
				}
			}
		}

		//remove clients from list
		for(int i = 0; i < disconnectList.Count - 1; i++)
		{
			//tell player the other disconnected
			SendData ("Server DISCONNECTED", clients);

			clients.Remove(disconnectList[i]);
			disconnectList.RemoveAt(i);
		}
	}

	private void StartListening()
	{
		//once it gets a connection: handshake
		server.BeginAcceptTcpClient (AcceptTcpClient, server);
	}

	private void AcceptTcpClient(IAsyncResult ar)
	{
		//the client has been accepted, grabs its listener and add it to the list
		TcpListener listener = (TcpListener)ar.AsyncState;

		//list of client names
		string allUsers = "";
		foreach(ServerClient i in clients)
		{
			allUsers += i.clientName + '|';
		}

		ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
		clients.Add(sc);

		//goes back to listening: grabs messages from other people and adds them to the list if they connect
		StartListening();

		//sending message to the last client that has connected: the client at the last index
		//using the overload
		//ID = identifying client msg
		SendData("Server ID|" + allUsers, clients[clients.Count - 1]);
	}

	private bool IsConnected(TcpClient c)
	{
		//check if connected
		try {
			if (c != null && c.Client != null && c.Client.Connected) {
				if (c.Client.Poll (0, SelectMode.SelectRead))
					return !(c.Client.Receive (new byte[1], SocketFlags.Peek) == 0);

				return true;
			}

			else
				return false;
		}

		catch 
		{
			return false;
		}
	}

	//server send
	private void SendData(string data, List<ServerClient> cl)
	{
		foreach (ServerClient sc in cl) {
			try {
				StreamWriter writer = new StreamWriter (sc.tcp.GetStream ());
				writer.WriteLine (data);
				writer.Flush ();
			} 
			catch (Exception e) 
			{
				Debug.Log ("Write error : " + e.Message);
			}
		}
	}
	
	//overload
	private void SendData(string data, ServerClient c)
	{
		//new list with only one index
		List<ServerClient> sc = new List<ServerClient> { c };
		SendData(data, sc);
	}

	//server read
	private void ServerIncomingData(ServerClient c, string data)
	{
		Debug.Log ("Server: " + data);
		string[] aData = data.Split ('|');

		switch (aData[0])
		{
			case "Client ID":
				c.clientName = aData [1];
				c.isHost = (aData[2] == "0") ? false : true;
				//CONNECTED = somebody has connected
				SendData ("Server CONNECTED|" + c.clientName, clients);
				break;

			//move
			case "Client MOVE":
				SendData ("Server MOVE|" + aData[1] + "|" + aData[2] + "|" + aData[3] + "|" + aData[4], clients);
				break;

			//move + delete
			case "Client REMOVE":
				SendData ("Server REMOVE|" + aData[1] + "|" + aData[2] + "|" + aData[3] + "|" + aData[4] + "|" + aData[5] + "|" + aData[6], clients);
				break;
		}
	}
}
	
public class ServerClient
{ 
	public string clientName;
	//socket
	public TcpClient tcp;
	public bool isHost;

	//constructor
	public ServerClient(TcpClient tcp)
	{
		this.tcp = tcp;
	}
}