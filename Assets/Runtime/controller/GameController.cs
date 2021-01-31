using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using tetris3D.model;
using tetris3D.view;
using TMPro;
using UnityEngine;

namespace tetris3D.controller
{
    [RequireComponent(typeof(TetrominosDataBase))]
    public class GameController : MonoBehaviour
    {
        private enum GameState
        {
            DroppingPieces,
            DroppingPiecesFast,
            EndGame
        }

        private enum Orientation
        {
            Forward,
            Right,
            Back,
            Left
        }

        private GameState gameState = GameState.EndGame;

        //should be const but must be compile-time instantiated
        [SerializeField] private Vector3 PIVOT_OFFSET_TO_GRID = new Vector3(0, 0.5f, 0);

        [SerializeField] private float decentInterval = 0.8f;
        [SerializeField] private float fastDecentInterval = 0.15f;

        [SerializeField] private Vector3Int matrixDimensions = new Vector3Int(7, 10, 7);

        //TODO: calculate it according to matrixDimensions
        private int negWidthBound = -7 / 3;
        private int posWidthBound = 7 / 3;
        private int negLengthBound = -7 / 3;
        private int posLengthBound = 7 / 3;
        private GameMatrix gameMatrix;
        private TetrominosDataBase tetrominosDataBase;
        private TetrominoPieceView activePiece;
        private List<TetrominoPieceView> prevPieces = new List<TetrominoPieceView>();

        [SerializeField] private Camera uiCamera;

        private Vector3 spawnPosition;
        private Vector3Int piecePosition;
        private Transform cameraTransform;

        private void Awake()
        {
            tetrominosDataBase = GetComponent<TetrominosDataBase>();
            cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && gameState == GameState.EndGame)
                StartGame();

            if (activePiece == null)
                return;

            Orientation orientation = GetControlsOrientation();
            HandleRotation(orientation);
            HandleMovement(orientation);

            if (Input.GetKey(KeyCode.RightShift))
                gameState = GameState.DroppingPiecesFast;
            else
                gameState = GameState.DroppingPieces;
        }

        private Orientation GetControlsOrientation()
        {
            float yRot = cameraTransform.eulerAngles.y;
            if (yRot < 0)
                yRot += 360;

            if (yRot < 45 || yRot >= 315)
                return Orientation.Forward;
            else if (cameraTransform.eulerAngles.y >= 45 && cameraTransform.eulerAngles.y < 135)
                return Orientation.Right;
            else if (cameraTransform.eulerAngles.y >= 135 && cameraTransform.eulerAngles.y < 225)
                return Orientation.Back;
            else
                return Orientation.Left;
        }

        private void HandleRotation(Orientation orientation)
        {
            bool clockwise = false;
            RotationAxis rotationAxis = RotationAxis.X;
            bool hadInput = false;
            if (Input.GetKeyDown(KeyCode.Q))
            {
                switch (orientation)
                {
                    case Orientation.Forward:
                        activePiece.RotateModel(RotationAxis.X, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Right:
                        activePiece.RotateModel(RotationAxis.Z, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Back:
                        activePiece.RotateModel(RotationAxis.X, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Left:
                        activePiece.RotateModel(RotationAxis.Z, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.Z;
                        break;
                }

                hadInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                switch (orientation)
                {
                    case Orientation.Forward:
                        activePiece.RotateModel(RotationAxis.X, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Right:
                        activePiece.RotateModel(RotationAxis.Z, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Back:
                        activePiece.RotateModel(RotationAxis.X, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Left:
                        activePiece.RotateModel(RotationAxis.Z, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.Z;
                        break;
                }

                hadInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                activePiece.RotateModel(RotationAxis.Y, false);
                if (!IsInBounds(Vector3Int.zero))
                {
                    activePiece.RotateModel(RotationAxis.Y, true);
                    return;
                }

                clockwise = false;
                rotationAxis = RotationAxis.Y;
                hadInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                activePiece.RotateModel(RotationAxis.Y, true);
                if (!IsInBounds(Vector3Int.zero))
                {
                    activePiece.RotateModel(RotationAxis.Y, false);
                    return;
                }

                clockwise = true;
                rotationAxis = RotationAxis.Y;
                hadInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                switch (orientation)
                {
                    case Orientation.Forward:
                        activePiece.RotateModel(RotationAxis.Z, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Right:
                        activePiece.RotateModel(RotationAxis.X, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Back:
                        activePiece.RotateModel(RotationAxis.Z, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Left:
                        activePiece.RotateModel(RotationAxis.X, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.X;
                        break;
                }

                hadInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                switch (orientation)
                {
                    case Orientation.Forward:
                        activePiece.RotateModel(RotationAxis.Z, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Right:
                        activePiece.RotateModel(RotationAxis.X, true);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, false);
                            return;
                        }

                        clockwise = true;
                        rotationAxis = RotationAxis.X;
                        break;
                    case Orientation.Back:
                        activePiece.RotateModel(RotationAxis.Z, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.Z, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.Z;
                        break;
                    case Orientation.Left:
                        activePiece.RotateModel(RotationAxis.X, false);
                        if (!IsInBounds(Vector3Int.zero))
                        {
                            activePiece.RotateModel(RotationAxis.X, true);
                            return;
                        }

                        clockwise = false;
                        rotationAxis = RotationAxis.X;
                        break;
                }

                hadInput = true;
            }
            else
            {
                hadInput = false;
            }

            if (hadInput)
                activePiece.RotateTetromino(rotationAxis, clockwise);
        }

        private void HandleMovement(Orientation orientation)
        {
            Vector3Int targetDirection = new Vector3Int();
            switch (orientation)
            {
                case Orientation.Forward:
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        targetDirection = Vector3Int.right;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        targetDirection = Vector3Int.left;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        targetDirection = targetDirection.Forward();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        targetDirection = targetDirection.Back();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    break;
                case Orientation.Right:
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        targetDirection = targetDirection.Back();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        targetDirection = targetDirection.Forward();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        targetDirection = Vector3Int.right;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        targetDirection = Vector3Int.left;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    break;
                case Orientation.Back:
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        targetDirection = Vector3Int.left;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        targetDirection = Vector3Int.right;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        targetDirection = targetDirection.Back();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        targetDirection = targetDirection.Forward();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    break;
                case Orientation.Left:
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        targetDirection = targetDirection.Forward();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        targetDirection = targetDirection.Back();
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        targetDirection = Vector3Int.left;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        targetDirection = Vector3Int.right;
                        if (!IsInBounds(targetDirection))
                            return;
                    }

                    break;
            }

            activePiece.transform.DOBlendableMoveBy(targetDirection, 0.1f);
            piecePosition += targetDirection;
        }

        private bool IsInBounds(Vector3Int targetDirection)
        {
            Vector3Int targetPosition = piecePosition + targetDirection - activePiece.PivotLocalPosition;
            foreach (Vector3Int cubePos in activePiece.CubesLocalPositions)
            {
                Vector3Int cubePosition = targetPosition + cubePos;
                if (cubePosition.x < 0 || cubePosition.x >= matrixDimensions.x || cubePosition.z < 0 ||
                    cubePosition.z >= matrixDimensions.z)
                    return false;
            }

            return !gameMatrix.IsOverlapping(activePiece, piecePosition + targetDirection);
        }

        public void StartGame()
        {
            uiCamera.enabled = false;
            ResetMatrix();
            for (int i = prevPieces.Count - 1; i >= 0; i--)
            {
                Destroy(prevPieces[i].gameObject);
            }

            prevPieces.Clear();
            spawnPosition = new Vector3(0, matrixDimensions.y + 1, 0) + PIVOT_OFFSET_TO_GRID;
            GetPiece();
            gameState = GameState.DroppingPieces;
            StartCoroutine(Loop());
        }

        private void ResetMatrix()
        {
            if (gameMatrix != null)
            {
                List<GameObject> cubesFromPrevRound = gameMatrix.GetLeftOversCubes();
                cubesFromPrevRound.AddRange(gameMatrix.GetOverStacked());
                for (int i = cubesFromPrevRound.Count - 1; i >= 0; i--)
                    Destroy(cubesFromPrevRound[i]);
            }

            gameMatrix = new GameMatrix(matrixDimensions);
        }

        private void GetPiece()
        {
            if (activePiece != null)
                prevPieces.Add(activePiece);

            activePiece = tetrominosDataBase.GetRandomPiece();
            activePiece.transform.position = spawnPosition;
            piecePosition = new Vector3Int(3, matrixDimensions.y + 1, 3);
        }

        IEnumerator Loop()
        {
            List<GameObject> cubesToShiftDown = null;
            while (gameState != GameState.EndGame)
            {
                if (gameState == GameState.DroppingPieces)
                    yield return new WaitForSeconds(decentInterval);
                else
                    yield return new WaitForSeconds(fastDecentInterval);

                piecePosition += Vector3Int.down;
                if (gameMatrix.IsOverlapping(activePiece, piecePosition))
                {
                    piecePosition += Vector3Int.up;
                    if (gameMatrix.StackTetromino(activePiece, piecePosition, out cubesToShiftDown))
                    {
                        GetPiece();
                    }
                    else
                    {
                        GameOver();
                    }
                }
                else
                {
                    activePiece.transform.DOBlendableLocalMoveBy(Vector3Int.down, 0.1f);
                    if (piecePosition.y + activePiece.BottomLocalHeight == 0)
                    {
                        gameMatrix.StackTetromino(activePiece, piecePosition, out cubesToShiftDown);
                        GetPiece();
                    }
                }

                if (cubesToShiftDown != null && cubesToShiftDown.Count > 0)
                {
                    foreach (GameObject cube in cubesToShiftDown)
                    {
                        cube.transform.DOBlendableMoveBy(Vector3Int.down, 0.1f);
                        cubesToShiftDown = null;
                    }
                }
            }
        }

        private void GameOver()
        {
            uiCamera.enabled = true;
            uiCamera.GetComponentInChildren<TextMeshProUGUI>().text = "GAME OVER\n[Dark Souls: you died music]";
            gameState = GameState.EndGame;
            activePiece = null;
        }
    }
}