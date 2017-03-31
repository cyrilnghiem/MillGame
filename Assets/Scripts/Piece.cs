using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {

	public bool isWhite;

	public Hashtable places = new Hashtable ();

	private void Start()
	{
		places.Add (new Vector2 (3, 6), true);
		places.Add (new Vector2 (6, 6), true);
		places.Add (new Vector2 (9, 6), true);
		places.Add (new Vector2 (4, 5), true);
		places.Add (new Vector2 (6, 5), true);
		places.Add (new Vector2 (8, 5), true);
		places.Add (new Vector2 (5, 4), true);
		places.Add (new Vector2 (6, 4), true);
		places.Add (new Vector2 (7, 4), true);
		places.Add (new Vector2 (3, 3), true);
		places.Add (new Vector2 (4, 3), true);
		places.Add (new Vector2 (5, 3), true);
		places.Add (new Vector2 (7, 3), true);
		places.Add (new Vector2 (8, 3), true);
		places.Add (new Vector2 (9, 3), true);
		places.Add (new Vector2 (5, 2), true);
		places.Add (new Vector2 (6, 2), true);
		places.Add (new Vector2 (7, 2), true);
		places.Add (new Vector2 (4, 1), true);
		places.Add (new Vector2 (6, 1), true);
		places.Add (new Vector2 (8, 1), true);
		places.Add (new Vector2 (3, 0), true);
		places.Add (new Vector2 (6, 0), true);
		places.Add (new Vector2 (9, 0), true);

	}

	public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2, int turn, int piecesLeft)
	{
		//if moving on top of another piece
		if (board [x2, y2] != null)
			return false;

		//if in list 24 places
		if (places.ContainsKey(new Vector2 (x2, y2)))
		{
			//STATE PLACE or STATE JUMP
			if (turn < 19 || piecesLeft == 3)
				return true;

			//STATE MOVE
			else 
			{
				//interior square or X == 6 or y == 3 axes 
				//moves +/- 1
				if ((x2 == x1 && (y2 == y1 + 1 || y2 == y1 - 1)) || (y2 == y1 && (x2 == x1 + 1 || x2 == x1 - 1)))
					return true;	

				//exterior square
				if (x1 == 3 || x1 == 9 || y1 == 0 || y1 == 6) 
				{	
					//moves +/- 3 OR +/- 1 
					if ((x2 == x1 && (y2 == y1 + 3 || y2 == y1 - 3)) || (y2 == y1 && (x2 == x1 + 3 || x2 == x1 - 3)))
						return true;
				}

				//middle square
				if (x1 == 4 || x1 == 8 || y1 == 1 || y1 == 5) 
				{	
					//moves +/- 2
					if ((x2 == x1 && (y2 == y1 + 2 || y2 == y1 - 2)) || (y2 == y1 && (x2 == x1 + 2 || x2 == x1 - 2)))
						return true;
				}

			}
		}

		return false;
	}

	//test mill by elimination
	public bool TestMillX(Piece[,] board, int x, int y)
	{
		//mill = 1 same color column
		//exterior piece was moved
		if (x != 6) 
		{
			for (int i = 0; i <= 6; i++)
				//test if there is an empty spot on the same x axe
				if (board [x, i] == null && places.ContainsKey (new Vector2 (x, i)))
					return false;
			
				//test if a piece on the same x axe is not same color
				else if (board [x, i] != null)
					if (board [x, i].isWhite != isWhite)
						return false;
		} 

		//mill = 1 same color half column in the center
		//interior pieces was moved
		else 
		{
			//lower center
			if (y < 3) {
				for (int i = 0; i < 3; i++)
					if (board [x, i] == null && places.ContainsKey (new Vector2 (x, i)))
						return false;
					else if (board [x, i] != null)
						if (board [x, i].isWhite != isWhite)
							return false;
			} 

			//upper center
			else {
				for (int i = 4; i <= 6; i++)
					if (board [x, i] == null && places.ContainsKey (new Vector2 (x, i)))
						return false;
					else if (board [x, i] != null)
						if (board [x, i].isWhite != isWhite)
							return false;
			}
		}
		//if there is no other color piece nor empty spot on the same x axe = mill
		return true;
	}

	public bool TestMillY(Piece[,] board, int x, int y)
	{
		//mill = 1 same color line
		if (y != 3) 
		{
			for (int i = 0; i <= 9; i++)
				if (board [i, y] == null && places.ContainsKey (new Vector2 (i, y)))
					return false;

				else if (board [i, y] != null)
					if (board [i, y].isWhite != isWhite)
						return false;
		} 
			
		else 
		{
			//middle left
			if (x < 6) {
				for (int i = 3; i < 6; i++)
					if (board [i, y] == null && places.ContainsKey (new Vector2 (i, y)))
						return false;
					else if (board [i, y] != null)
						if (board [i, y].isWhite != isWhite)
							return false;
			} 

			//middle right
			else {
				for (int i = 7; i <= 9; i++)
					if (board [i, y] == null && places.ContainsKey (new Vector2 (i, y)))
						return false;
					else if (board [i, y] != null)
						if (board [i, y].isWhite != isWhite)
							return false;
			}
		}
		//if there is no other color piece nor empty spot on the same y axe = mill
		return true;
	}
}