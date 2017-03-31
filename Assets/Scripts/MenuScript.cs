using UnityEngine;
using System.Collections;
//UI for using input field
using UnityEngine.UI;
//System for using Exception, message
using System;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

	public static MenuScript Instance { set; get; }

	public GameObject mainMenu;
	public GameObject serverMenu;
	public GameObject connectMenu;

	public GameObject serverPrefab;
	public GameObject clientPrefab;

	public InputField nameInput;

	private void Start () 
	{
		Instance = this;

		//canvas inactive at first
		serverMenu.SetActive (false);
		connectMenu.SetActive (false);

		//not destroying it when changing scene
		DontDestroyOnLoad (gameObject);
	}

	public void ConnectButton()
	{
		mainMenu.SetActive (false);
		connectMenu.SetActive (true);
	}

	public void HostButton()
	{
		try
		{
			Server s = Instantiate(serverPrefab).GetComponent<Server>();
			s.Init();

			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			//getting the name of the host
			c.clientName = nameInput.text;
			c.isHost = true;
			if(c.clientName == "")
				c.clientName = "Host";
			
			c.ConnectToServer("127.0.0.1" , 6321);
		}
		catch (Exception e) 
		{
			Debug.Log (e.Message);
		}

		mainMenu.SetActive (false);
		serverMenu.SetActive (true);
	}

	public void ConnectToServerButton()
	{
		string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;

		//if input field is empty: localhost
		if (hostAddress == "")
			hostAddress = "127.0.0.1";

		try
		{
			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			//getting the name of the client
			c.clientName = nameInput.text;
			if(c.clientName == "")
				c.clientName = "Client";
					
			c.ConnectToServer(hostAddress, 6321);
			connectMenu.SetActive(false);
		}
		catch (Exception e)
		{
			Debug.Log (e.Message);
		}
	}

	public void BackButton()
	{
		mainMenu.SetActive (true);
		serverMenu.SetActive (false);
		connectMenu.SetActive (false);

		//delete server and client
		Server s = FindObjectOfType<Server>();
		if (s != null)
			Destroy (s.gameObject);

		Client c = FindObjectOfType<Client>();
		if (c != null)
			Destroy (c.gameObject);
	}

	public void StartGame()
	{
		SceneManager.LoadScene ("Game");
	}
}