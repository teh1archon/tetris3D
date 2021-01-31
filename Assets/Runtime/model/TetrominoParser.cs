using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace tetris3D.model
{
    public class TetrominoParser
    {
        public List<(float, Tetromino)> Parse(string path)
        {
            List<(float, Tetromino)> dropRateToPiece = new List<(float, Tetromino)>();
            TextAsset json = Resources.Load(path) as TextAsset;
            JSONArray tetroinosDefinitions = JSON.Parse(json.text).AsArray;
            float accumulatedDropRates = 0;
            for (int i = 0; i < tetroinosDefinitions.Count; i++)
            {
                int[] flatMatrix = ParseMatrix(tetroinosDefinitions[i][0].AsArray);
                Vector3Int pivot = ParsePivot(tetroinosDefinitions[i][1].AsArray);
                float dropRate = tetroinosDefinitions[i][2][0].AsFloat;
                accumulatedDropRates += dropRate;
                dropRateToPiece.Add((accumulatedDropRates, new Tetromino(flatMatrix, pivot, i, dropRate)));
            }

            return dropRateToPiece;
        }

        private Vector3Int ParsePivot(JSONArray pivot)
        {
            if (pivot.Count != 3)
                throw new Exception(String.Format("illegal pivot array size: {0}", pivot.Count));

            return new Vector3Int(pivot[0], pivot[1], pivot[2]);
        }

        private int[] ParseMatrix(JSONArray matrix)
        {
            int[] parsedFlatMatrix = new int[matrix.Count];
            for (int i = 0; i < matrix.Count; i++)
                parsedFlatMatrix[i] = matrix[i];

            return parsedFlatMatrix;
        }
    }
}