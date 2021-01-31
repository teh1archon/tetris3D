using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3IntExtensions
{
    public static Vector3Int Forward(this Vector3Int v3i)
    {
        return new Vector3Int(0, 0, 1);
    }
    
    public static Vector3Int Back(this Vector3Int v3i)
    {
        return new Vector3Int(0, 0, -1);
    }
}
