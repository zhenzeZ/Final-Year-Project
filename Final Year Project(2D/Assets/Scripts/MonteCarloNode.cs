using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonteCarloNode : MonoBehaviour
{
    public int score;
    public int timesVisited;

    MonteCarloNode parent;
    public List<MonteCarloNode> children;
    public List<MonteCarloNode> availableMoves;

    public Position point;
    public GoBoard board;

    public MonteCarloNode()
    {
        score = 0;
        timesVisited = 0;
        children = new List<MonteCarloNode>();
        //availableMoves = board.getAvailableMoves(); // insert board's availableMoves
        
    }

    public MonteCarloNode(MonteCarloNode Parent, Position point)
    {
        score = 0;
        timesVisited = 0;
        parent = Parent;
        children = new List<MonteCarloNode>();
        availableMoves = new List<MonteCarloNode>(parent.availableMoves);
        this.point = point;
        board = new GoBoard();
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
        if (parent != null)
        {
            parent.Backup(val);
        }

    }

    public MonteCarloNode Expand()
    {
        if (availableMoves.Count > 0)
        {
            MonteCarloNode ret = availableMoves[0];
            AddChild(ret);
            availableMoves.Remove(ret);
            return ret;
        }

        return null;
    }

    public MonteCarloNode BestChild()
    {
        int bestVal = int.MinValue;
        MonteCarloNode bestChild = null;

        foreach (MonteCarloNode node in children)
        {
            if (bestVal < node.score)
            {
                if (bestChild != null)
                {
                    // get the new node and reomve the old bestChild
                    children.Remove(bestChild);
                    bestChild = node;
                }
            }
            else if (bestVal > node.score)
            {
                // remove node for saving memories
                children.Remove(node);

            }
        }

        return bestChild;
    }

    public void AddAvailableMoves(List<Position> Points)
    {
        foreach (Position p in Points)
        {
            availableMoves.Add(new MonteCarloNode(this, p));
        }
    }


    public void AddChildren(List<Position> Points)
    {
        foreach (Position p in Points)
        {
            AddChild(new MonteCarloNode(this, p));
        }
    }

    public void AddChild(MonteCarloNode Child)
    {
        children.Add(Child);
    }

    public Position getRandomPosition(List<Position> availableMoves)
    {


        return null;
    }
}
