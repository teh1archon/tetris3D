using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tetris3D.view;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace tetris3D.model
{
    public class TetrominosDataBase : MonoBehaviour
    {
        [SerializeField] private GameObject cube;
        [SerializeField] private GameObject tetrominoPivotPrefab;
        [SerializeField] private Material[] tetrominoMaterials;
        private TetrominoParser tetrominoParser;

        [Tooltip("path for json file in Resources")]
        public string definitionsFile = "tetriminos";

        private List<float> dropRates = new List<float>();
        private List<TetrominoPieceView> tetrominosPrefabs = new List<TetrominoPieceView>();
        private List<Tetromino> tetrominoPieceData = new List<Tetromino>();

        private void Start()
        {
            tetrominoParser = new TetrominoParser();
            BuildTetrominosPrefabs(tetrominoParser.Parse(definitionsFile));
        }

        private void BuildTetrominosPrefabs(List<(float dropRateThreshold, Tetromino pieceData)> tetrominosData)
        {
            foreach ((float dropRateThreshold, Tetromino pieceData) piece in tetrominosData)
            {
                dropRates.Add(piece.dropRateThreshold);
                GameObject pivotPiece = Instantiate(tetrominoPivotPrefab);
                pivotPiece.GetComponent<TetrominoPieceView>().Init(piece.pieceData);
                Vector3 pivotPosition = piece.pieceData.pivot;
                foreach (Vector3Int localPos in piece.pieceData.CubesLocalPositions)
                {
                    GameObject subPiece = Instantiate(cube, pivotPiece.transform);
                    subPiece.transform.localPosition = localPos - pivotPosition;
                    subPiece.GetComponent<MeshRenderer>().sharedMaterial =
                        tetrominoMaterials[piece.pieceData.MaterialID];
                }

                tetrominoPieceData.Add(piece.pieceData);
                tetrominosPrefabs.Add((pivotPiece.GetComponent<TetrominoPieceView>()));
                pivotPiece.transform.position = new Vector3(1000, 1000, 1000);
            }
        }

        public TetrominoPieceView GetRandomPiece()
        {
            float rand = Random.Range(0, dropRates.Last());
            int index = 0;
            while (index < dropRates.Count && rand >= dropRates[index])
                index++;

            return GetPiece(index);
        }

        public TetrominoPieceView GetPiece(int index)
        {
            TetrominoPieceView tpv = Instantiate(tetrominosPrefabs[index]);
            tpv.Init(tetrominoPieceData[index]);
            return tpv;
        }
    }
}