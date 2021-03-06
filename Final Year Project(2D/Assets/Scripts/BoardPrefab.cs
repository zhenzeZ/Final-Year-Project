﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class BoardPrefab : MonoBehaviour {
    public Board mainBoard;
    Player Player1;
    Player Player2;
    public GameObject BoardTilePrefab;
    public GameObject BoardClickPrefab;
    public GameObject PiecePrefab;
    List<List<GameObject>> BoardTiles;
    List<List<GameObject>> BoardClickTiles;
    List<GameObject> BoardPieces;
    GameObject BoardBackground;

    public Text txtStatus;

    public Text txtBoardSize;
    public Text txtDepth;
    public Slider sldBoardSize;
    public Slider sldDepth;
    public Toggle tglBlackAI;
    public Toggle tglWhiteAI;
    public Toggle tglBlackAI_AlphaBeta;
    public Toggle tglWhiteAI_AlphaBeta;
    public Button btnPlay;
    public GameObject StartupPanel;
    public GameObject mainCanvas;

    bool Paused;

    float tileSize = 1.0f;

    public float tileSpacing = 0.1f;

	// Use this for initialization
	void Start () {
        mainBoard = new Board(Constants.BOARDSIZE);
        Player1 = new Player(Constants.BLACKCOLOR, ref mainBoard, false);
        Player2 = new Player(Constants.WHITECOLOR, ref mainBoard, true);

        BoardPieces = new List<GameObject>();
        txtBoardSize.GetComponent<Text>().text = "Board Size: " + Constants.BOARDSIZE + " x " + Constants.BOARDSIZE;

        sldDepth.GetComponent<Slider>().value = Constants.MAXDEPTH;
        txtDepth.GetComponent<Text>().text = "Exploration Depth: " + Constants.MAXDEPTH;

        PauseGame();
        CreateBoard();
    }
	
    void CreateBoard()
    {
        //clear out all child gameobjects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        //create regular board tiles
        BoardTiles = new List<List<GameObject>>();
        for (int i = 0; i < mainBoard.boardSize - 1; i++)
        {
            List<GameObject> row = new List<GameObject>();
            float yprogress = (float)i / (mainBoard.boardSize - 1);

            for (int j = 0; j < mainBoard.boardSize - 1; j++)
            {
                GameObject temp = Instantiate(BoardTilePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                temp.transform.SetParent(transform);
                float wid = -((mainBoard.boardSize - 1)) * tileSize;
                float xprogress = (float)j / (mainBoard.boardSize - 1);
                float xpos = ((xprogress) * wid) - wid / 2.0f;
                xpos -= tileSize / 2;
                xpos *= 1.1f;

                float zpos = ((yprogress) * wid) - wid / 2.0f;
                zpos -= tileSize / 2;
                zpos *= 1.1f;

                temp.transform.position = new Vector3(xpos, 0, zpos);
                temp.name = "Panel " + i.ToString() + ", " + j.ToString();
                row.Add(temp);
            }
            BoardTiles.Add(row);
        }

        //create board background, giving the illusion of lines
        BoardBackground = Instantiate(BoardTilePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        float backgroundSize = (tileSize / 10.0f) * mainBoard.boardSize;
        BoardBackground.transform.position = new Vector3(BoardBackground.transform.position.x, -0.01f, BoardBackground.transform.position.z);
        BoardBackground.transform.localScale = new Vector3(backgroundSize, backgroundSize, backgroundSize);
        Renderer rend = BoardBackground.GetComponent<Renderer>();
        rend.enabled = true;
        rend.material.color = Constants.BLACKCOLOR;
        BoardBackground.transform.parent = transform;

        //now create click panels, offset so that clickable panels will be right on top of the line intersections
        BoardClickTiles = new List<List<GameObject>>();
        for (int i = 0; i < mainBoard.boardSize; i++)
        {
            List<GameObject> row = new List<GameObject>();
            float yprogress = (float)i / (mainBoard.boardSize - 1);

            for (int j = 0; j < mainBoard.boardSize; j++)
            {

                GameObject temp = Instantiate(BoardClickPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                temp.transform.SetParent(transform);
                float wid = -((mainBoard.boardSize - 1)) * tileSize;
                float xprogress = (float)j / (mainBoard.boardSize - 1);
                float xpos = ((xprogress) * wid) - wid / 2.0f;
                //xpos -= tileSize / 2;
                xpos *= 1.1f;

                float zpos = ((yprogress) * wid) - wid / 2.0f;
                //zpos -= tileSize / 2;
                zpos *= 1.1f;

                temp.transform.position = new Vector3(xpos, 0.01f, zpos);
                temp.name = "ClickPanel " + i.ToString() + ", " + j.ToString();
                row.Add(temp);

            }
            BoardClickTiles.Add(row);
        }
    }

    void RebuildBoard()
    {
        for (int i = 0; i < BoardPieces.Count; i++)
        {
            Destroy(BoardPieces[i]);
        }
        BoardPieces.Clear();

        for (int i = 0; i < mainBoard.pieceMatrix.Count; i++)
        {
            float yprogress = (float)i / (mainBoard.boardSize - 1);
            for (int j = 0;j<mainBoard.pieceMatrix.Count; j++)
            {
                if (mainBoard.pieceMatrix[i][j].color == Constants.CLEARCOLOR)
                    continue;//skip this piece

                GameObject tempPiece = Instantiate(PiecePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                tempPiece.transform.parent = transform;

                float wid = -((mainBoard.boardSize - 1)) * tileSize;
                float xprogress = (float)j / (mainBoard.boardSize - 1);
                float xpos = ((xprogress) * wid) - wid / 2.0f;
                //xpos -= tileSize / 2;
                xpos *= 1.1f;

                float zpos = ((yprogress) * wid) - wid / 2.0f;
                //zpos -= tileSize / 2;
                zpos *= 1.1f;

                tempPiece.transform.position = new Vector3(xpos, 0.16f, zpos);
                BoardPieces.Add(tempPiece);

                if (mainBoard.pieceMatrix[i][j].color == Constants.WHITECOLOR) {
                    Renderer rend = tempPiece.GetComponent<Renderer>();
                    rend.enabled = true;
                    rend.material.color = Constants.WHITECOLOR;
                }
            }
        }

        Vector2 score = mainBoard.CountPieces();
    }

    public void ChangeBoardSize(Single s)
    {
        int newsize = Constants.BOARDSIZE + (int)s * 2;
        Debug.Log(newsize);
        mainBoard = new Board(newsize);
            
        Player1 = new Player(Constants.BLACKCOLOR, ref mainBoard, tglBlackAI.GetComponent<Toggle>().isOn);
        Player2 = new Player(Constants.WHITECOLOR, ref mainBoard, tglWhiteAI.GetComponent<Toggle>().isOn);

        txtBoardSize.GetComponent<Text>().text = "Board Size: " + newsize + " x " + newsize;

        CreateBoard();
    }

    //alphaBeta only
    public void ChangeAlphaBetaDepth(Single s)
    {
        int newDepth = (int)s;
        mainBoard.AlphaBetaMaxDepth = newDepth;
        txtDepth.GetComponent<Text>().text = "Exploration Depth: " + newDepth;
    }

    public void PauseGame()
    {
        Paused = true;
        StartupPanel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0);
        Player1.Playing = false;
        Player2.Playing = false;
    }

    public void unPauseGame()
    {
        Paused = false;
        int width = (int)mainCanvas.GetComponent<RectTransform>().rect.width;
        Player1.AI = tglBlackAI.GetComponent<Toggle>().isOn;
        Player2.AI = tglWhiteAI.GetComponent<Toggle>().isOn;
        Player1.AlphaBeta = tglBlackAI_AlphaBeta.GetComponent<Toggle>().isOn;
        Player2.AlphaBeta = tglWhiteAI_AlphaBeta.GetComponent<Toggle>().isOn;
        StartupPanel.GetComponent<RectTransform>().localPosition = new Vector3(width, 0);
        Debug.Log(Player1.AI);
        Debug.Log(Player2.AI);
    }

	// Update is called once per frame
	/// <summary>
    /// 
    /// </summary>
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused)
                unPauseGame();
            else
                PauseGame();
        }

        if (Paused)
            return;

        if (mainBoard.needsRefreshModel)
        {
            RebuildBoard();
            mainBoard.needsRefreshModel = false;
        }

        if (mainBoard.PossibleMovesNum == 0)
        {
            txtStatus.GetComponent<Text>().text = "End Game";
        }
        else if (mainBoard.CurrentPlayerColor == Constants.BLACKCOLOR)
        {
            txtStatus.GetComponent<Text>().text = "Black's Turn...";
            if (Player1.AI)
                txtStatus.GetComponent<Text>().text += " (Thinking)";
        }
        else if (mainBoard.CurrentPlayerColor == Constants.WHITECOLOR)
        {
            txtStatus.GetComponent<Text>().text = "White's Turn...";
            if (Player2.AI)
                txtStatus.GetComponent<Text>().text += " (Thinking)";
        }
        

        if (!Player1.Playing && !Player2.Playing && mainBoard.PossibleMovesNum != 0)
        {
            //mainBoard.PossibleMoves();//update possible moves;
            Debug.Log("possible moves: " + mainBoard.PossibleMovesNum);
        }

        if (mainBoard.PossibleMoves().Count == 0)
        {
            txtStatus.GetComponent<Text>().text = "End of Game";
        }

        //Debug.Log("player1：" + Player1.Playing);
        //Debug.Log("player2：" + Player2.Playing);
        if (Player1.AI && Player2.AI && mainBoard.PossibleMovesNum > 0)
        {
            Debug.Log("AI Move");
            // AI with alphaBeta 
            if (mainBoard.CurrentTurn == 0 && !Player1.Playing)
            {
                if (Player1.AlphaBeta)
                {
                    StartCoroutine(Player1.playAICoroutineAB());
                }
            }
            else if (mainBoard.CurrentTurn == 1 && !Player2.Playing)
            {
                if (Player2.AlphaBeta)
                {
                    StartCoroutine(Player2.playAICoroutineAB());
                }
            }


            // AI with MCTS
            if (mainBoard.CurrentTurn == 0 && !Player1.Playing)
            {
                if (!Player1.AlphaBeta)
                {
                    StartCoroutine(Player1.playAICoroutineMCTS());
                }
                //Player1.playAICoroutineMCTS();
            }
            else if (mainBoard.CurrentTurn == 1 && !Player2.Playing)
            {
                if (!Player2.AlphaBeta)
                {
                    StartCoroutine(Player2.playAICoroutineMCTS());
                }
                //Player2.playAICoroutineMCTS();
            }
        }
        else if (mainBoard.PossibleMovesNum > 0)//else if both players are not both AI...
        {
            //Debug.Log("check");
            //transform.Rotate(Vector3.up, Time.deltaTime * 20);
            if (Input.GetMouseButtonDown(0) && ((mainBoard.CurrentTurn == 0 && !Player1.AI) || (mainBoard.CurrentTurn == 1 && !Player2.AI)))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    string GameObjName = hit.transform.gameObject.name;
                    Debug.Log(GameObjName);
                    string[] delimiters = { ",", " " };
                    string[] tok = GameObjName.Split(delimiters, StringSplitOptions.None);

                    if (tok[0] == "ClickPanel")
                    {
                        if (mainBoard.CurrentTurn == 0)
                        {
                            mainBoard.PlayPiece(Int32.Parse(tok[1]), Int32.Parse(tok[3]), Constants.BLACKCOLOR);
                        }
                        else
                        {
                            mainBoard.PlayPiece(Int32.Parse(tok[1]), Int32.Parse(tok[3]), Constants.WHITECOLOR);
                        }
                    }
                }

                if (mainBoard.CurrentTurn == 0 && Player1.AI)
                {
                    Debug.Log("AI WITH BLACK PIECE");
                    if (Player1.AlphaBeta)
                    {
                        // AI with alphaBeta 
                        StartCoroutine(Player1.playAICoroutineAB());
                    }
                    else
                    {
                        // AI with MCTS
                        StartCoroutine(Player1.playAICoroutineMCTS());
                        //Player1.playAICoroutineMCTS();
                    }
                }
                else if (mainBoard.CurrentTurn == 1 && Player2.AI)
                {
                    Debug.Log("AI WITH WHITE PIECE");
                    if (Player1.AlphaBeta)
                    {
                        // AI with alphaBeta 
                        //StartCoroutine(Player2.playAICoroutineAB());
                    }
                    else
                    {
                        // AI with MCTS
                        StartCoroutine(Player2.playAICoroutineMCTS());
                        //Player2.playAICoroutineMCTS();
                    }
                }
            }
        }
        
    }
}
