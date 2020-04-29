using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct Node
{
    public int score;
    public Vector2 move;
}

public class Player {
    public bool AI;//Indicates if this player is an AI
    public Color color { get; private set; }
    public Board board;//reference to the actual Board
    public bool Playing;//this bool is just used to indicate if the AI is actively thinking at a given frame

    private static double Cp = 1 / Mathf.Sqrt(2);
    public int numExpansions = 100;
    private double[,] lookupTable = new double[1500, 1500];

    private int numLookups = 0;
    private int numNoLookups = 0;

    public Player(Color c, ref Board b)
    {
        Playing = false;
        AI = false;
        color = c;
        board = b;
    }

    public Player(Color c, ref Board b, bool isAI)
    {
        Playing = false;
        AI = isAI;
        color = c;
        board = b;

        for (int i = 1; i < 1500; ++i)
        {
            for (int j = i; j < 1500; ++j)
            {
                lookupTable[i, j] = (Cp * Math.Sqrt((Math.Log((double)i)) / (double)j));
            }
        }
    }

    public double getRHS(int n, int nj)
    {
        if (n < 1500)
        {
            numLookups++;
            return lookupTable[n, nj];
        }
        numNoLookups++;
        return (2 * Cp * Math.Sqrt((2 * (Math.Log((double)n))) / (double)nj));

    }

    public IEnumerator playAICoroutineAB()
    {
     
        Playing = true;
        yield return new WaitForSeconds(0.1f);
        int playableSpotsCount = board.PossibleMoves().Count;
        if (playableSpotsCount > 0)
        {
            Node choice;
            choice.move = Vector2.zero;
            if (color == Constants.WHITECOLOR)
            {
                choice = alphaBetaMin(board, Int32.MinValue, Int32.MaxValue, board.AlphaBetaMaxDepth);
            }
            else
            {//black
                choice = alphaBetaMax(board, Int32.MinValue, Int32.MaxValue, board.AlphaBetaMaxDepth);
            }

            board.PlayPiece((int)choice.move.x, (int)choice.move.y, color);
        }
        Playing = false;
    }


    // Generates and stores all available moves for whoever's turn it is
    // Returns the best possible computer move as a List<Point>, including the piece put down
    public IEnumerator playAICoroutineMCTS()
    {
        Debug.Log("MCTS start");

        Playing = true;
        yield return new WaitForSeconds(0.1f);
        int playableSpotsCount = board.PossibleMoves().Count;
        if (playableSpotsCount > 0)
        {
            Debug.Log("computer move");

            List<Vector2> computerMove = new List<Vector2>();
            Vector2 bestMove = new Vector2();


            // MonteCarloNode rootNode = new MonteCarloNode(board.cloneBoard(), this);
            MonteCarloNode rootNode;
            rootNode = new MonteCarloNode(board.cloneBoard(), this);


            for (int i = 0; i < numExpansions; i++)
            {
                MonteCarloNode n = TreePolicy(rootNode);
                n.Backup(Simulate(n));
            }
            Debug.Log("finished simulating");
            MonteCarloNode maxNode = null;

            Debug.Log("maxnode set");
            double maxVal = double.NegativeInfinity;

            foreach (MonteCarloNode node in rootNode.children)
            {
                if (node.timesVisited == 0)
                {
                    continue;
                }

                //Debug.Log("node: " + node.point + " score: " + node.score + " timesVisited: " + node.timesVisited);

                double UCBvalue = (double)node.score / (double)node.timesVisited + Math.Log10(numExpansions) / (double)node.timesVisited;

                //Debug.Log("UCB value: " + UCBvalue);
                //if ((double)node.score / (double)node.timesVisited > maxVal)
                //{
                //    maxNode = new MonteCarloNode(node);
                //    maxVal = (double)node.score / (double)node.timesVisited;

                //}

                if (UCBvalue > maxVal)
                {
                    maxNode = new MonteCarloNode(node);
                    maxVal = UCBvalue;
                }
            }

            bestMove = maxNode.point;

            //board.state.availableMoves.TryGetValue(bestMove, out computerMove);
            // Have to add the move itself to the list of Points
            computerMove.Insert(0, bestMove);

            board.PlayPiece((int)computerMove[0].x, (int)computerMove[0].y, color);
        }
        Playing = false;
    }


    Node alphaBetaMax(Board B, int alpha, int beta, int depth)//black
    {
        List<Vector2> possible = B.PossibleMoves();

        if (depth == 0 || possible.Count == 0)
        {//only score the Board at the last iteration
            Node LeafNode;
            LeafNode.score = B.Score();
            LeafNode.move = new Vector2(0, 0);
            return LeafNode;
        }

        Node n;
        n.move = new Vector2(0, 0);
        n.score = 0;

        for (int i = 0; i < possible.Count; i++)
        {
            int x = (int)possible[i].x;
            int y = (int)possible[i].y;
            Board temp = B.cloneBoard();
            temp.PlayPiece(x, y, Constants.BLACKCOLOR);
            int score = alphaBetaMin(temp, alpha, beta, depth - 1).score;
            if (score >= beta)
            {
                n.score = beta;
                n.move = new Vector2(x, y);
                return n;//pruning, don't use this branch
            }
            if (score > alpha)//update alpha for this branch
            {
                alpha = score;
                n.move = new Vector2(x, y);
            }
        }
        n.score = alpha;
        return n;
    }

    Node alphaBetaMin(Board B, int alpha, int beta, int depth)//white
    {
        List<Vector2> possible = B.PossibleMoves();

        if (depth == 0 || possible.Count == 0)//only score the Board at the last iteration
        {//only score the Board at the last iteration
            Node LeafNode;
            LeafNode.score = B.Score();
            LeafNode.move = new Vector2(0, 0);
            return LeafNode;
        }

        Node n;
        n.move = new Vector2(0, 0);
        n.score = 0;

        for (int i = 0; i < possible.Count; i++)
        {
            int x = (int)possible[i].x;
            int y = (int)possible[i].y;
            Board temp = B.cloneBoard();
            temp.PlayPiece(x, y, Constants.WHITECOLOR);
            int score = alphaBetaMax(temp, alpha, beta, depth - 1).score;
            if (score <= alpha)
            {
                n.score = alpha;
                n.move = new Vector2(x, y);
                return n;//cutoff all further nodes
            }
            if (score < beta)
            {
                beta = score;
                n.move = new Vector2(x, y);
            }
        }
        n.score = beta;
        return n;
    }

    private MonteCarloNode TreePolicy(MonteCarloNode n)
    {
        MonteCarloNode v = n;
        List<Vector2> possible = v.board.PossibleMoves();

        while (possible.Count != 0)
        {
            v.AddAvailableMoves(v.board.PossibleMoves());
            if (v.availableMoves.Count != 0)
            {
                return v.Expand();
            }
            else
            {
                v = v.BestChild();
            }
        }
        return v;
    }

    public int Simulate(MonteCarloNode node)
    {
        //Debug.Log ("simulate" );
        Board temp = node.board.cloneBoard();
        UnityEngine.Random.seed = (int)Time.timeSinceLevelLoad;

        while (temp.PossibleMoves().Count != 0)
        {
            List<Vector2> moves = temp.PossibleMoves();
            int i = UnityEngine.Random.Range(0, moves.Count - 1);

            //List<Point> p = new List<Point>();W

            //if (!board.AvailableMove(moves[i], ref p))
            //{

            //}

            //Debug.Log(i + "  " + moves.Count() + " random pos: " + moves[i]);
            if (temp.CurrentTurn == 0)
            {
                temp.PlayPiece((int)moves[i].x, (int)moves[i].y, Constants.BLACKCOLOR);
            }
            else
            {
                temp.PlayPiece((int)moves[i].x, (int)moves[i].y, Constants.WHITECOLOR);
            }
            //board.ApplyMove(board.PlacePiece(moves[i]));
            //board.GenerateAvailableMoves();
        }
        //Debug.Log ("simulated");
        if (temp.CountPieces().x > temp.CountPieces().y)
        {
            return 1;
        }
        else if (temp.CountPieces().y > temp.CountPieces().x)
        {
            return -1;
        }
        else
        {
            return 0;
        }

    }

}
