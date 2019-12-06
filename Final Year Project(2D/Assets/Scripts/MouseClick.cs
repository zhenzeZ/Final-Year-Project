using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClick : MonoBehaviour
{
    public TurnShift turn;

    private Vector2 mousePosition;

    private Rigidbody2D rb;

    private float deltaX, deltaY;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMouseDown()
    {
        deltaX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;
        deltaY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y;
    }

    private void OnMouseUp()
    {
        if (turn.getTurn())
        {
            turn.setTurn(false);
        }
        else
        {
            turn.setTurn(true);
        }

    }
}
