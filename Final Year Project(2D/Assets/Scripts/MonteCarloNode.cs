using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class MonteCarloNode
{
    public int score;
    public int timesVisited;

    MonteCarloNode parent;
    public List<MonteCarloNode> children;
    public List<MonteCarloNode> availableMoves;

    public Vector2 point;
    public Board board;

    Player ai;

    public MonteCarloNode(Board b, Player AI)
    {
        score = 0;
        timesVisited = 0;
        ai = AI;
        parent = null;
        children = new List<MonteCarloNode>();
        availableMoves = new List<MonteCarloNode>();
        board = b.cloneBoard();

        AddAvailableMoves(board.PossibleMoves());
    }

    public MonteCarloNode(MonteCarloNode Parent, Vector2 point)
    {
        score = 0;
        timesVisited = 0;
        ai = Parent.ai;
        parent = Parent;
        children = new List<MonteCarloNode>();
        availableMoves = new List<MonteCarloNode>(parent.availableMoves);
        this.point = point;
        board = parent.board.cloneBoard();
        if (board.CurrentTurn == 0)
        {
            board.PlayPiece((int)point.x, (int)point.y, Constants.BLACKCOLOR);
        }
        else
        {
            board.PlayPiece((int)point.x, (int)point.y, Constants.WHITECOLOR);
        }
        //board.GenerateAvailableMoves();
        //AddAvailableMoves(board.availableMoves.Keys.ToList());
    }

    public MonteCarloNode(MonteCarloNode n)
    {
        score = n.score;
        timesVisited = n.timesVisited;
        parent = n.parent;
        children = new List<MonteCarloNode>(n.children);
        availableMoves = new List<MonteCarloNode>(n.availableMoves);
        point = n.point;
    }

    public void Backup(int val)
    {
        score += val;
        timesVisited++;
        //Debug.Log(" score: " + score + " timesVisited: " + timesVisited);
        if (parent != null)
        {
            parent.Backup(val);
        }

    }

    public MonteCarloNode Expand()
    {
        UnityEngine.Random.seed = (int)Time.timeSinceLevelLoad;
        if (availableMoves.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, availableMoves.Count - 1);
            MonteCarloNode ret = availableMoves[i];
            AddChild(ret);
            availableMoves.Remove(ret);
            return ret;
        }
        Debug.Log("really really big problems");
        return null;
    }

    public MonteCarloNode BestChild()
    {
        double bestVal = double.MinValue;
        MonteCarloNode bestChild = null;

        foreach (MonteCarloNode node in children)
        {
            double utc = ((double)node.score / (double)node.timesVisited) + ai.getRHS(timesVisited, node.timesVisited);

            if (utc > bestVal)
            {
                bestChild = node;
                bestVal = utc;
            }
        }

        return bestChild;
    }

    public void AddAvailableMoves(List<Vector2> Points)
    {
        foreach (Vector2 p in Points)
        {
            availableMoves.Add(new MonteCarloNode(this, p));
        }
    }


    public void AddChildren(List<Vector2> Points)
    {
        foreach (Vector2 p in Points)
        {
            AddChild(new MonteCarloNode(this, p));
        }
    }

    public void AddChild(MonteCarloNode Child)
    {
        children.Add(Child);
    }
}
