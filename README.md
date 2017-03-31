# MillGame

## Description
As a university project, I've created a multiplayer mill game using Unity3D. This repository includes all of the Unity files as well 
as the files to launch an actual game.

## User's guide
To start a game, open either MillGameMac.app or MillGameWindows.exe depending on the operating system.
Since the game has been created in the 16/10 format, it is most suitable to select a display mode respecting this ratio. 
For example: 800 x 500, 1024 x 640, 1152 x 720, 1280 x 800, 1440 x 900 or 1680 x 1050.

In the game's menu, users can enter their name and choose between two options : 
  * Host Game: host the game and wait until another player connects. 
  * Connect To Host: enter the host's IP address then connect.
 
 Once both users are connected to one-another, the game starts. The host player has the white pieces and starts the game.

### Rules
Taking turns, the players place one of their nine pieces on the board. Those pieces can then be moved one "square" at a time. 
At any time during the game, if a player aligns three pieces horizontally or vertically, a mill is created. This means that an opponent's 
piece can be removed from the board. Once one of the player only has three pieces left, he or she can move them to any free spot on the 
board. The game is over when a player only has two pieces left. 

### Messages
> *Mill: you can remove a piece.*
  
  Shows up when the player can take a piece and that his or her opponent has at least one piece which can be taken. 
  
> *Mill: unfortunately all of the pieces are in a mill. Sorry !*
  
Shows up when the player could have taken a piece however his or her opponent does not have a piece which is not in a mill.
  
> *This piece cannot be removed. Please try another one.*

In "Remove" mode, lets the player know that the selected piece is either in a mill or not placed on the board yet.
  
> *Please select a piece from your opponent.*

In "Remove" mode, lets the player know that, either the selected piece is one of his or her own or none was actually selected. 
  
> *Player1 versus Player2.*

Shows up at the very beginning of the game to display the names of the players. Once the first player has done a move, the message 
  is disabled.
  
> *Player1/Player2 has won !*

Shows up at the end of the game to display the name of the winner. After 5 seconds, the game window is closed and a new game can be started
  by opening the game file again.
