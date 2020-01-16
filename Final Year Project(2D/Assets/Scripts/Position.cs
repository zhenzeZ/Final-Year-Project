using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Position()
    {
        X = -1;
        Y = -1;
    }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (!(obj is Position))
        {
            return false;
        }

        return Equals((Position)obj);
    }

    public bool Equals(Position point)
    {
        if (X != point.X)
        {
            return false;
        }

        return (Y == point.Y);
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }
}
