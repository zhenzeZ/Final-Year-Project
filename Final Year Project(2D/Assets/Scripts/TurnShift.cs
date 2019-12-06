using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnShift : MonoBehaviour
{
    private bool playerTurn;

    // Start is called before the first frame update
    void Start()
    {
        playerTurn = true;
    }

    public void setTurn(bool turn)
    {
        playerTurn = turn;
    }

    public bool getTurn()
    {
        return playerTurn;
    }
}
