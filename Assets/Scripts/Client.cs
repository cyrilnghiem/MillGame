using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;

public class Client : MonoBehaviour 
{
	public string clientName;
	public bool isHost;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;

	public List<GameClient> players = new List<GameClient> ();

	private void Start()
	{
		DontDestroyOnLoad (gameObject);
	}

	public bool ConnectToServer(string host, int port)
	{
		//if already connected
		if (socketReady)
			return false;

		try 
		{
			socket = new TcpClient (host, port);
			stream = socket.GetStream ();
			writer = new StreamWriter (stream);
			reader = new StreamReader (stream);

			socketReady = true;
		} 
		catch (Exception e) 
		{
			Debug.Log ("Socket error " + e.Message);
		}

		return socketReady;
	}

	private void Update()
	{
		if (socketReady) 
		{
			if (stream.DataAvailable) 
			{
				string data = reader.ReadLine ();
				if (data != null)
					ClientIncomingData (data);
			}
		}
	}

	//sending messages to the server
	public void Send(string data)
	{
		if (!socketReady)
			return;

			writer.WriteLine (data);
			writer.Flush();
	}

	//read messages from the server
	private void ClientIncomingData(string data)
	{
		Debug.Log ("Client : " + data);
		string[] aData = data.Split ('|');

		switch (aData[0])
		{
			case "Server ID":
				for (int i = 1; i < aData.Length - 1; i++) 
				{
					UserConnected (aData [i], false);
				}
				//server receives the client name and can identify who just connected
				Send ("Client ID|" + clientName + "|" + ((isHost) ? 1 : 0).ToString());
				break;

			case "Server CONNECTED":
				UserConnected (aData [1], false);
				break;
			//test
			case "Server DISCONNECTED":
				BoardScript.Instance.SetMessage ("Your opponent has disconnected. :(");
				break;

			//move
			case "Server MOVE":
				BoardScript.Instance.Move (int.Parse (aData [1]), int.Parse (aData [2]), int.Parse (aData [3]), int.Parse (aData [4])); 
				break;

			//move + delete
			case "Server REMOVE":
				BoardScript.Instance.Move (int.Parse (aData [1]), int.Parse (aData [2]), int.Parse (aData [3]), int.Parse (aData [4])); 
				BoardScript.Instance.Remove (int.Parse (aData [5]), int.Parse (aData [6])); 
				break;
		}
	}

	private void UserConnected(string name, bool host)
	{
		GameClient c = new GameClient();
		c.name = name;

		players.Add(c); 

		if (players.Count == 2)
			MenuScript.Instance.StartGame ();
	}

	private void OnApplicationQuit()
	{
		CloseSocket ();
	}

	private void OnDisable()
	{
		CloseSocket ();
	}

	//disconnection
	private void CloseSocket()
	{
		if (!socketReady)
			return;
		
		writer.Close ();
		reader.Close ();
		socket.Close ();
		socketReady = false;
	}
}

public class GameClient
{
	public string name;
	public bool isHost;
}