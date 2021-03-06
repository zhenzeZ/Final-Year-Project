﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board {
    public List<List<Piece>> pieceMatrix;
    public int CurrentTurn { get; private set; }//0 for black, 1 for white

    public int PossibleMovesNum;
    public int Size;

    //public Dictionary<Vector2, List<Vector2>> availableMoves;

    public Color CurrentPlayerColor {
        get {
            if (CurrentTurn == 0)
            {
                return Constants.BLACKCOLOR;
            }
            return Constants.WHITECOLOR;
        }
    }
    public bool needsRefreshModel;

    public List<Vector2> StartingMoves;

    public int boardSize {
        get { return pieceMatrix.Count; }
    }

    public int AlphaBetaMaxDepth;

    public Board(int d)
    {
        PossibleMovesNum = 1;
        needsRefreshModel = false;
        AlphaBetaMaxDepth = Constants.MAXDEPTH;
        pieceMatrix = new List<List<Piece>>();
        CurrentTurn = 0;
        for (int i = 0; i < d; i++)
        {
            List<Piece> temp = new List<Piece>();
            for (int j = 0; j < d; j++)
            {
                Piece tempPiece = new Piece(new Vector2(i, j));
                temp.Add(tempPiece);
            }
            pieceMatrix.Add(temp);
        }
        Size = d;
        //availableMoves = new Dictionary<Vector2, List<Vector2>>();

        StartingMoves = new List<Vector2>();
        StartingMoves.Add(new Vector2(1, 1));
        StartingMoves.Add(new Vector2(1, boardSize - 2));
        StartingMoves.Add(new Vector2(boardSize - 2, 1));
        StartingMoves.Add(new Vector2(boardSize - 2, boardSize - 2));
        StartingMoves.Add(new Vector2((boardSize - 1) / 2, (boardSize - 1) / 2));


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="targetColor"></param>
    /// <returns></returns>
    public List<Piece> AdjPieces(int row, int col, Color targetColor)
    {

        List<Piece> adj = new List<Piece>();
        if ((row - 1) >= 0 && pieceMatrix[row - 1][col].color == targetColor)
            adj.Add(pieceMatrix[row - 1][col]);
        if ((col - 1) >= 0 && pieceMatrix[row][col - 1].color == targetColor)
            adj.Add(pieceMatrix[row][col - 1]);
        if ((row + 1) < boardSize && pieceMatrix[row + 1][col].color == targetColor)
            adj.Add(pieceMatrix[row + 1][col]);
        if ((col + 1) < boardSize && pieceMatrix[row][col + 1].color == targetColor)
            adj.Add(pieceMatrix[row][col + 1]);

        return adj;
    }

    public List<Piece> ConnectedPiecesDFS(int r, int c, List<Piece> l)
    {
        if (l == null)
            l = new List<Piece>();

        l.Add(pieceMatrix[r][c]);

        //look for adjacent pieces of the same color
        List<Piece> adj = AdjPieces(r, c, pieceMatrix[r][c].color);
        for (int i = 0; i < adj.Count; i++)
        {
            bool ShouldExplore = true;
            for (int j = 0; j < l.Count; j++)
            {
                if (l[j].position == adj[i].position)
                    ShouldExplore = false;
            }

            if (ShouldExplore) {
                ConnectedPiecesDFS((int)adj[i].position.x, (int)adj[i].position.y, l);
            }

        }

        //TODO: dfs search starting from node [r][c], looking for all connected notes of the same color
        return l;
    }

    public List<List<Piece>> FindAllGroups()
    {
        List<List<Piece>> groups = new List<List<Piece>>();

        List<List<bool>> ExploredLocations = new List<List<bool>>();
        for (int i = 0; i < boardSize; i++)
        {
            List<bool> row = new List<bool>();
            for (int j = 0; j < boardSize; j++)
            {
                row.Add(false);
            }
            ExploredLocations.Add(row);
        }

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (pieceMatrix[i][j].color != Constants.CLEARCOLOR)
                {
                    if (ExploredLocations[i][j] == false)
                    {
                        List<Piece> temp = ConnectedPiecesDFS(i, j, null);
                        ExploredLocations[i][j] = true;//record this location as having been explored
                        groups.Add(temp);
                    }
                }
            }
        }

        return groups;
    }

    public List<Piece> ConnectedGroupLiberties(List<Piece> Conn) {
        List<Piece> Liberties = new List<Piece>();
        for (int i = 0; i < Conn.Count; i++)
        {
            List<Piece> temp = AdjPieces((int)Conn[i].position.x, (int)Conn[i].position.y, Constants.CLEARCOLOR);
            for (int j = 0; j < temp.Count; j++)
            {
                Liberties.Add(temp[j]);
            }
        }

        // eliminate duplicate liberties before returning

        return Liberties;
    }

    public List<Vector2> PossibleMoves()
    {
        List<Vector2> PosMoves = new List<Vector2>();
        //every spot that isn't currently occupied. Later we can optimize this

        for (int i = 0;i< pieceMatrix.Count; i++)
        {
            for (int j = 0; j < pieceMatrix.Count; j++)
            {
                if (pieceMatrix[i][j].color == Constants.CLEARCOLOR)
                {
                    Board tempBoard = cloneBoard();
                    tempBoard.PlayPiece(i, j, CurrentPlayerColor);
                    if (tempBoard.pieceMatrix[i][j].color == Constants.CLEARCOLOR)
                    {
                        //the piece was instantly captured, this is not a playable space
                    }
                    else
                    {
                        PosMoves.Add(new Vector2(i, j));
                    }
                }
            }
        }
        PossibleMovesNum = PosMoves.Count;
        return PosMoves;
    }

    public bool PlayPiece(int r, int c, Color color)
    {
        if (r >= 0 && c >= 0 && r < pieceMatrix.Count && c < pieceMatrix.Count)
        {
            if (pieceMatrix[r][c].color == Constants.CLEARCOLOR)
            {
                pieceMatrix[r][c] = new Piece(new Vector2(r, c), color);
                EnforceTheRules();
                CurrentTurn = (CurrentTurn == 0) ? 1 : 0;//end turn, JOBS DONE and turn shift
                //TODO: evaluate and eliminate captured pieces
                needsRefreshModel = true;
                return true;
            }
        }

        return false;
        //Action could not be completed, either there is already a piece in the position specified,
        //or the position specified does not exist
    }

    public void EnforceTheRules()
    {

        List<List<Piece>> groups = FindAllGroups();

        //remove groups with no liberties for groups of the opponent's color
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i][0].color != CurrentPlayerColor)
            {
                List<Piece> Libs = ConnectedGroupLiberties(groups[i]);
                if (Libs.Count == 0)
                {
                    //there are no liberties left for this group! Remove all the pieces! EXTERMINATE!!!
                    RemovePieces(groups[i]);
                }
            }
        }

        //remove groups with no liberties for groups of the player's color
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i][0].color == CurrentPlayerColor)
            {
                List<Piece> Libs = ConnectedGroupLiberties(groups[i]);
                if (Libs.Count == 0)
                {
                    //there are no liberties left for this group! Remove all the pieces! EXTERMINATE!!!
                    RemovePieces(groups[i]);
                }
            }
        }
    }

    public Vector2 CountPieces()//returns a vector of the black pieces and white pieces 
    {
        int b = 0;
        int w = 0;

        for (int i = 0; i < pieceMatrix.Count; i++)
        {
            for (int j = 0; j < pieceMatrix[i].Count; j++)
            {
                if (pieceMatrix[i][j].color == Constants.BLACKCOLOR)
                    b++;
                else if (pieceMatrix[i][j].color == Constants.WHITECOLOR)
                    w++;
            }
        }

        // return black and white score
        return new Vector2(b, w);
    }

    public int Score()
    {
        Vector2 piecesCount = CountPieces();
        return (int)piecesCount.x - (int)piecesCount.y;
    }

    public void RemovePieces(List<Piece> l)
    {
        for (int i = 0; i < l.Count; i++)
        {
            pieceMatrix[(int)l[i].position.x][(int)l[i].position.y].color = Constants.CLEARCOLOR;
        }
        return;
    }

    public Board cloneBoard()
    {
        Board newBoard = new Board(boardSize);
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Piece temp = new Piece(pieceMatrix[i][j].position, pieceMatrix[i][j].color);
                newBoard.pieceMatrix[i][j] = temp;
            }
        }
        return newBoard;
    }

}
