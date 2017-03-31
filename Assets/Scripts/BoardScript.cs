using UnityEngine;
using System.Collections;
//text elements
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardScript : MonoBehaviour 
{
	public static BoardScript Instance { set; get; }

	public Piece[,] pieces = new Piece[13, 7];
	public GameObject whitePiecePrefab;
	public GameObject blackPiecePrefab;
	public GameObject canvas;

	//0.5f x and y to place the piece in the center of the square
	//0.1f to appear higher than the board
	private Vector3 centerPiece = new Vector3(0.5f, 0.1f, 0.5f);

	public bool isWhite;
	private bool isWhiteTurn;
	private bool canRemove;
	private bool gameOver;
	private bool noUpdate;

	public int turn;
	private int whitePieces;
	private int blackPieces;

	private Piece selectedPiece;

	private Vector2 mouse;
	private Vector2 startMove;
	private Vector2 endMove;

	private Client client;

	private float gameOverTime;

	//initialization
	private void Start () 
	{
		Instance = this;
		client = FindObjectOfType<Client> ();
		isWhite = client.isHost;

		turn = 1;
		whitePieces = 9;
		blackPieces = 9;

		isWhiteTurn = true;
		canRemove = false;
		CreateGamePieces();

		SetMessage(client.players[0].name + " versus " + client.players[1].name);

		GameObject.Find ("HostName").GetComponent<Text> ().text = client.players[0].name + " (white)";
		GameObject.Find ("GuestName").GetComponent<Text> ().text = client.players[1].name + " (black)";
	}

	private void Update()
	{
		//not updating when the game is over
		if (gameOver)
		{
			//5 seconds after game is over: close window
			if (Time.time - gameOverTime > 5.0f) 
			{
				Application.Quit();
			}
			return;
		}

		UpdateMouse ();

		if ((isWhite) ? isWhiteTurn : !isWhiteTurn) 
		{
			int x = (int)mouse.x;
			int y = (int)mouse.y;

			//new turn 
			if (!canRemove && !noUpdate) {
				if (selectedPiece != null)
					UpdateSelectedPiece (selectedPiece);	

				//0 = left click on the mouse
				if (Input.GetMouseButtonDown (0)) 
					Select (x, y);

				if (Input.GetMouseButtonUp (0))
					Move ((int)startMove.x, (int)startMove.y, x, y);
			}

			//removing a piece
			if (canRemove)
			{
				if (Input.GetMouseButtonDown (0)) {
					Piece p = pieces [x, y];

					if (p != null && p.isWhite != isWhite)
						Remove (x, y);

					else
						if(isWhite == isWhiteTurn)
							SetMessage ("Please select a piece from your opponent.");
				}
			}
		}
	}

	private void UpdateMouse()
	{
		RaycastHit hit;
		//if collides with something solid within the board mask (= Board plane)
		//25.0f = length of raycast (max distance to reach smthg solid)
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
		{
			mouse.x = (int)(hit.point.x);
			mouse.y = (int)(hit.point.z);
		}
	}

	//active piece elevated
	private void UpdateSelectedPiece(Piece p)
	{
		RaycastHit hit;
		if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board"))) 
		{
			p.transform.position = hit.point + Vector3.up;
		} 
	}

	private void Select(int x, int y)
	{
		//outside of the board
		if (x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length)
			return;
		
		//STATE PLACE : Moving not yet allowed
		//white player
		if (turn < 19 && x > 2 && pieces [x, y].isWhite == true)
			return;
			
		//black player
		if (turn < 19 && x < 10 && pieces [x, y].isWhite == false)
			return;

		Piece p = pieces [x, y];

		if (p != null && p.isWhite == isWhite) 
		{
			selectedPiece = p;
			startMove = mouse;
		}
	}

	public void Move(int x1, int y1, int x2, int y2)
	{
		//redifine values (multiplayer support)
		startMove = new Vector2 (x1, y1);
		endMove = new Vector2 (x2, y2);
		selectedPiece = pieces [x1, y1];

		//outside of the board
		if (x2 < 0 || x2 >= pieces.Length || y2 < 0 || y2 >= pieces.Length) 
		{
			//cancelling the move : back to x1 and y1 (startMove)
			if (selectedPiece != null)
				MovePiece (selectedPiece, x1, y1);


			startMove = Vector2.zero;
			selectedPiece = null;
			return;
		}

		//when a piece is selected
		if (selectedPiece != null)
		{
			//if it has not moved
			if (endMove == startMove)
			{
				//cancelling the move : back to x1 and y1 (startMove)
				MovePiece(selectedPiece, x1, y1);

				startMove = Vector2.zero;
				selectedPiece = null;
				return;
			}

			//check if its a valid move
			if (selectedPiece.ValidMove (pieces, x1, y1, x2, y2, turn, isWhiteTurn ? whitePieces : blackPieces)) 
			{
				//move the piece
				pieces [x2, y2] = selectedPiece; 
				pieces [x1, y1] = null;
				MovePiece (selectedPiece, x2, y2);

				//test if the moved piece forms a mill
				if (selectedPiece.TestMillX (pieces, x2, y2) || selectedPiece.TestMillY (pieces, x2, y2)) {

					//check all the pieces to see if there is at least one that can be taken
					for (int i = 3; i <= 9; i++) {
						for (int j = 0; j <= 6; j++) {
							if (pieces [i, j] != null && pieces [i, j].isWhite != isWhiteTurn) {

								if (!pieces [i, j].TestMillX (pieces, i, j) && !pieces [i, j].TestMillY (pieces, i, j)) {
									if (isWhite == isWhiteTurn)
										SetMessage ("Mill: you can remove a piece.");

									canRemove = true;
									return;
								} 
							}
						}
					}
					if (isWhite == isWhiteTurn) 
					{
						SetMessage ("Mill: unfortunately all of the pieces are in a mill. Sorry !");
						noUpdate = true;
						StartCoroutine (Wait ());
					}
					//Player receiving msg
					else
						SendMove ();
				}

				//no mill = turn is over
				else 
					SendMove ();
			}

			//unvalide move
			else {
				//cancelling the move : back to x1 and y1 (startMove)
				MovePiece (selectedPiece, x1, y1);

				startMove = Vector2.zero;
				selectedPiece = null;
				return;
			}
		}
	}

	//sends Move message
	private void SendMove()
	{
		string msg = "Client MOVE|";
		msg += startMove.x.ToString () + "|";
		msg += startMove.y.ToString () + "|";
		msg += endMove.x.ToString () + "|";
		msg += endMove.y.ToString ();
		client.Send (msg);

		EndTurn ();
	}

	//removes piece and sends Move/Remove message
	public void Remove(int x, int y)
	{
		selectedPiece = pieces [x, y];

		//multiplayer purposes : when server sends this instruction back it is executed again = piece has already been removed
		if (selectedPiece != null) {
			
			//test that its not in a mill (which also excludes not being placed yet : lines 91 & 96 Pieces.cs -> TestMillX always TRUE)
			if (!selectedPiece.TestMillX (pieces, x, y) && !selectedPiece.TestMillY (pieces, x, y)) {
				//delete it
				Piece p = pieces [x, y];
				pieces [x, y] = null;

				if (isWhite == isWhiteTurn)
					DestroyImmediate (p.gameObject);
				else
					//after 1 sec
					Destroy (p.gameObject, 1);

				string msg = "Client REMOVE|";
				msg += startMove.x.ToString () + "|";
				msg += startMove.y.ToString () + "|";
				msg += endMove.x.ToString () + "|";
				msg += endMove.y.ToString () + "|";
				msg += x.ToString () + "|";
				msg += y.ToString ();
				client.Send (msg);

				canRemove = false;

				if (isWhiteTurn)
					blackPieces--;
				else
					whitePieces--;

				EndTurn ();

				CheckVictory ();
			} 

			else if (isWhite == isWhiteTurn)
				SetMessage ("This piece cannot be removed. Please try another one.");
		}
	}

	private void EndTurn()
	{
		selectedPiece = null;
		startMove = Vector2.zero;

		turn = turn + 1;

		isWhiteTurn = !isWhiteTurn;

		if (isWhiteTurn) {
			GameObject.Find ("HostTurn").GetComponent<Text> ().text = "•";
			GameObject.Find ("GuestTurn").GetComponent<Text> ().text = "";
		} 
		else 
		{
			GameObject.Find ("HostTurn").GetComponent<Text> ().text = "";
			GameObject.Find ("GuestTurn").GetComponent<Text> ().text = "•";
		}

		canvas.transform.GetChild(0).gameObject.SetActive(false);
		noUpdate = false;
	}

	private void CheckVictory()
	{
		if (whitePieces < 3) 
		{
			SetMessage (client.players[1].name + " has won!");
			Victory ();
		}

		if (blackPieces < 3) 
		{
			SetMessage (client.players[0].name + " has won!");
			Victory ();
		}
	}

	private void Victory()
	{
		//sets the time of end of the game
		gameOverTime = Time.time;
		gameOver = true;
	}

	private void CreateGamePieces()
	{
		//white pieces
		for (int x = 0; x < 2; x++) 
		{
			for (int y = 1; y < 6; y++) 
			{
				CreatePiece (x, y);
				if (x == 1 && y == 4)
					break;
			}
		}

		//black pieces
		for (int x = 12; x > 10; x--) 
		{
			for (int y = 5; y > 0; y--) 
			{
				CreatePiece (x, y);
				if (x == 11 && y == 2)
					break;
			}
		}
	}

	private void CreatePiece(int x, int y)
	{
		bool isPieceWhite = (x < 11) ? true : false;

		GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
		go.transform.SetParent (transform);
		Piece p = go.GetComponent<Piece> ();
		pieces [x, y] = p;
		MovePiece (p, x, y);
	}

	private void MovePiece(Piece p, int x, int y)
	{
		p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + centerPiece;
	}

	public void SetMessage(string text)
	{
		//first child in Canvas
		canvas.transform.GetChild(0).gameObject.SetActive(true);
		canvas.GetComponentInChildren<Text>().text = text;
	}

	private IEnumerator Wait() 
	{
		yield return new WaitForSeconds(4);
		SendMove ();
	}
}