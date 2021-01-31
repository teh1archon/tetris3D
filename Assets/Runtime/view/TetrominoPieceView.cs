using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using tetris3D.model;
using UnityEngine;


namespace tetris3D.view
{
    public class TetrominoPieceView : MonoBehaviour
    {
        public const float ROTATION_DURATION = 0.1f;
        private Tetromino tetrominoDefinition;
        public List<GameObject> Cubes { get; private set; } = new List<GameObject>();
        public List<Vector3Int> CubesLocalPositions => tetrominoDefinition.CubesLocalPositions;
        public Vector3Int PivotLocalPosition => tetrominoDefinition.pivot;
        public int BottomLocalHeight { get; private set; }

        public void Init(Tetromino definition)
        {
            tetrominoDefinition = definition;
            for (int i = 0; i < transform.childCount; i++)
                Cubes.Add(transform.GetChild(i).gameObject);
            tetrominoDefinition.Reset();
            UpdateBottomPart();
        }

        public void RotateModel(RotationAxis axis, bool rotateClockwise)
        {
            //rotate in model
            tetrominoDefinition.Rotate(axis, rotateClockwise);
        }

        public void RotateTetromino(RotationAxis axis, bool rotateClockwise)
        {
            switch (axis)
            {
                case RotationAxis.X:
                    transform.DOBlendableRotateBy(Vector3.left * 90 * (rotateClockwise ? 1 : -1), ROTATION_DURATION);
                    break;
                case RotationAxis.Y:
                    transform.DOBlendableRotateBy(Vector3.up * 90 * (rotateClockwise ? 1 : -1), ROTATION_DURATION);
                    break;
                case RotationAxis.Z:
                    transform.DOBlendableRotateBy(Vector3.back * 90 * (rotateClockwise ? 1 : -1), ROTATION_DURATION);
                    break;
            }

            UpdateBottomPart();
        }

        private void UpdateBottomPart()
        {
            int height = 5;
            foreach (Vector3Int cubeLocalPosition in CubesLocalPositions)
            {
                if (cubeLocalPosition.y - PivotLocalPosition.y < height)
                    height = cubeLocalPosition.y - PivotLocalPosition.y;
            }

            BottomLocalHeight = height;
        }
    }
}