using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tetris3D.model
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    };

    public enum RotationDirection
    {
        CounterClockwise,
        Clockwise
    };

    public class Tetromino
    {
        public const int TERTOMINO_3D_MAX_SIZE = 64;
        public const int DIMENSION_SIZE = 4;

        public List<Vector3Int> CubesLocalPositions { get; private set; }
        private List<Vector3Int> defaultCubesLocalPositions;
        public Vector3Int pivot { get; private set; }
        private Vector3Int defaultPivotPos;

        public int MaterialID { get; private set; }
        public float DropRate { get; private set; }
        
        public Tetromino(int[] matrix, Vector3Int startingPivot, int matID = 0, float rate = 0.2f)
        {
            if (matrix.Length != TERTOMINO_3D_MAX_SIZE)
                throw new Exception(String.Format("illegal matrix size to instantiate Tetromino: {0}", matrix.Length));
            ValidatePivotLocation(startingPivot);

            CubesLocalPositions = new List<Vector3Int>();
            defaultCubesLocalPositions = new List<Vector3Int>();
            //fill the matrix row by row from left to right, bottom to top length, layer by layer bottom to top
            for (int y = 0; y < DIMENSION_SIZE; y++)
            {
                for (int z = 0; z < DIMENSION_SIZE; z++)
                {
                    for (int x = 0; x < DIMENSION_SIZE; x++)
                    {
                        if (matrix[x + DIMENSION_SIZE * z + DIMENSION_SIZE * DIMENSION_SIZE * y] != 0)
                        {
                            CubesLocalPositions.Add(new Vector3Int(x, y, z));
                            defaultCubesLocalPositions.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }

            pivot = startingPivot;
            defaultPivotPos = startingPivot;
            MaterialID = matID;
            DropRate = rate;
        }

        private static void ValidatePivotLocation(Vector3Int startingPivot)
        {
            if (startingPivot.x < 0 || startingPivot.x >= DIMENSION_SIZE)
                throw new Exception(String.Format("illegal pivot x location at {0}", startingPivot.x));
            if (startingPivot.y < 0 || startingPivot.y >= DIMENSION_SIZE)
                throw new Exception(String.Format("illegal pivot y location at {0}", startingPivot.y));
            if (startingPivot.z < 0 || startingPivot.z >= DIMENSION_SIZE)
                throw new Exception(String.Format("illegal pivot z location at {0}", startingPivot.z));
        }


        public void Rotate(RotationAxis axis, bool rotateClockwise)
        {
            //make a temp layer to rotate a 2D array
            int[,] tempLayer = new int[DIMENSION_SIZE, DIMENSION_SIZE];
            //rotate each layer according to the rotation axis
            switch (axis)
            {
                case RotationAxis.X: //rotate YZ plane
                    if (rotateClockwise)
                    {
                        pivot = new Vector3Int(pivot.x, pivot.z, DIMENSION_SIZE - pivot.y - 1);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(CubesLocalPositions[i].x, CubesLocalPositions[i].z,
                                DIMENSION_SIZE - CubesLocalPositions[i].y - 1);
                    }
                    else
                    {
                        pivot = new Vector3Int(pivot.x, DIMENSION_SIZE - pivot.z - 1, pivot.y);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(CubesLocalPositions[i].x,
                                DIMENSION_SIZE - CubesLocalPositions[i].z - 1, CubesLocalPositions[i].y);
                    }

                    break;
                case RotationAxis.Y: //rotate XZ plane
                    if (rotateClockwise)
                    {
                        pivot = new Vector3Int(pivot.z, pivot.y, DIMENSION_SIZE - pivot.x - 1);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(CubesLocalPositions[i].z, CubesLocalPositions[i].y,
                                DIMENSION_SIZE - CubesLocalPositions[i].x - 1);
                    }
                    else
                    {
                        pivot = new Vector3Int(DIMENSION_SIZE - pivot.z - 1, pivot.y, pivot.x);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(DIMENSION_SIZE - CubesLocalPositions[i].z - 1,
                                CubesLocalPositions[i].y, CubesLocalPositions[i].x);
                    }

                    break;

                case RotationAxis.Z: //rotate XY plane
                    if (rotateClockwise)
                    {
                        pivot = new Vector3Int(pivot.y, DIMENSION_SIZE - pivot.x - 1, pivot.z);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(CubesLocalPositions[i].y,
                                DIMENSION_SIZE - CubesLocalPositions[i].x - 1, CubesLocalPositions[i].z);
                    }
                    else
                    {
                        pivot = new Vector3Int(DIMENSION_SIZE - pivot.y - 1, pivot.x, pivot.z);
                        for (int i = 0; i < CubesLocalPositions.Count; i++)
                            CubesLocalPositions[i] = new Vector3Int(DIMENSION_SIZE - CubesLocalPositions[i].y - 1,
                                CubesLocalPositions[i].x, CubesLocalPositions[i].z);
                    }

                    break;

            }
        }

        public void Reset()
        {
            pivot = defaultPivotPos;
            for (int i = 0; i < CubesLocalPositions.Count; i++)
            {
                CubesLocalPositions[i] = defaultCubesLocalPositions[i];
            }
        }
    }
}