using System.Collections;
using System.Collections.Generic;
using tetris3D.view;
using UnityEngine;

namespace tetris3D.model
{
    public class GameMatrix
    {
        private GameObject[,,] matrixState;
        private List<GameObject> overstackedCubes = new List<GameObject>();
        private int width, height, length;
        private int topLayer = 0;

        public Vector3Int MatrixDimensions => new Vector3Int(width, height, length);

        public GameMatrix(int width = 7, int height = 10, int length = 7)
        {
            matrixState = new GameObject[width, height, length];
            this.width = width;
            this.height = height;
            this.length = length;
            topLayer = 0;
        }

        public GameMatrix(Vector3Int dimensions)
        {
            width = dimensions.x;
            height = dimensions.y;
            length = dimensions.z;
            matrixState = new GameObject[width, height, length];
            topLayer = 0;
        }

        public bool IsOverlapping(TetrominoPieceView tetrominoPieceView, Vector3Int droppedPosition)
        {
            List<Vector3Int> localPoses = tetrominoPieceView.CubesLocalPositions;
            Vector3Int pivotLocalPosition = tetrominoPieceView.PivotLocalPosition;

            for (int i = 0; i < localPoses.Count; i++)
            {
                Vector3Int posInMatrix = new Vector3Int(droppedPosition.x + localPoses[i].x - pivotLocalPosition.x,
                    droppedPosition.y + localPoses[i].y - pivotLocalPosition.y,
                    droppedPosition.z + localPoses[i].z - pivotLocalPosition.z);

                //check if cube is in grace zone
                if (posInMatrix.y >= height)
                    continue;

                if (matrixState[posInMatrix.x, posInMatrix.y, posInMatrix.z] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool StackTetromino(TetrominoPieceView tetrominoPieceView, Vector3Int droppedPosition,
            out List<GameObject> cubesToShiftDown)
        {
            int possibleNewTop = 0;

            Vector3Int tetrominoPosition = droppedPosition - tetrominoPieceView.PivotLocalPosition;
            HashSet<int> layersStacked = new HashSet<int>();
            List<int> layersFilled = new List<int>();
            List<Vector3Int> localPoses = tetrominoPieceView.CubesLocalPositions;
            for (int i = 0; i < localPoses.Count; i++)
            {
                if (tetrominoPosition.y + localPoses[i].y >= height)
                {
                    overstackedCubes.Add(tetrominoPieceView.Cubes[i]);
                    possibleNewTop = height;
                    continue;
                }

                matrixState[tetrominoPosition.x + localPoses[i].x, tetrominoPosition.y + localPoses[i].y,
                    tetrominoPosition.z + localPoses[i].z] = tetrominoPieceView.Cubes[i];

                possibleNewTop = Mathf.Max(possibleNewTop, droppedPosition.y + localPoses[i].y);
                layersStacked.Add(tetrominoPosition.y + localPoses[i].y);
            }

            //check if layer cleared
            foreach (int height in layersStacked)
            {
                if (IsLayerFilled(height))
                    layersFilled.Add(height);
            }

            cubesToShiftDown = null;
            if (layersFilled.Count > 0)
                cubesToShiftDown = RemoveLayers(layersFilled[0], layersFilled.Count);


            topLayer = Mathf.Max(topLayer, possibleNewTop - layersFilled.Count);
            return topLayer < height;
        }

        public bool IsLayerFilled(int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    if (matrixState[x, height, z] == null)
                        return false;
                }
            }

            return true;
        }

        public List<GameObject> RemoveLayers(int inHeight, int numOfLayers = 1)
        {
            //destroy cubes in removed layers
            for (int h = inHeight; h < inHeight + numOfLayers; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    for (int l = 0; l < length; l++)
                        GameObject.Destroy(matrixState[w, h, l]);
                }
            }

            //shift down all the other cubes
            List<GameObject> cubesToShift = new List<GameObject>();
            for (int h = inHeight + numOfLayers; h <= topLayer; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    for (int l = 0; l < length; l++)
                    {
                        matrixState[w, h - numOfLayers, l] = matrixState[w, h, l];
                        if (matrixState[w, h, l] != null)
                        {
                            cubesToShift.Add(matrixState[w, h, l]);
                            matrixState[w, h, l] = null;
                        }

                    }
                }
            }

            topLayer -= numOfLayers;
            return cubesToShift;
        }

        public List<GameObject> GetLeftOversCubes()
        {
            List<GameObject> remaingCubes = new List<GameObject>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        if (matrixState[x, y, z] != null)
                            remaingCubes.Add(matrixState[x, y, z]);
                    }
                }
            }

            return remaingCubes;
        }

        public List<GameObject> GetOverStacked()
        {
            return overstackedCubes;
        }
    }
}