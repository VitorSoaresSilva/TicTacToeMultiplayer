using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _TicTacToe.Scripts
{
    public class GameVisualManager : NetworkBehaviour
    {
        private const float GRID_SIZE = 3.1f;
        [SerializeField] private Transform circlePrefab;
        [SerializeField] private Transform crossPrefab;
        [SerializeField] private Transform lineCompletePrefab;
        private List<GameObject> visualGameObjects;

        private void Awake()
        {
            visualGameObjects = new();
        }

        private void Start()
        {
            GameManager.Instance.OnClickedOnGridPosition += GameManager_onClickedOnGridPosition;
            GameManager.Instance.OnGameWin += GameManager_OnGameWin;
            GameManager.Instance.OnRematch += GameManager_OnRematch;
        }

        private void GameManager_OnRematch(object sender, EventArgs e)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            foreach (var visualGameObject in visualGameObjects)
            {
                Destroy(visualGameObject);
            }
            visualGameObjects.Clear();
        }

        private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            var eulerZ = 0f;
            switch (e.line.orientation)
            {
                default:
                case GameManager.Orientation.Horizontal:
                    eulerZ = 0f; 
                    break;
                case GameManager.Orientation.Vertical:
                    eulerZ = 90f; 
                    break;
                case GameManager.Orientation.DiagonalA:
                    eulerZ = 45f; 
                    break;
                case GameManager.Orientation.DiagonalB:
                    eulerZ = -45f; 
                    break;
            }
            
            Transform lineCompleteTransform = Instantiate(lineCompletePrefab,
                GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), 
                Quaternion.Euler(0,0,eulerZ));
            
            lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
            visualGameObjects.Add(lineCompleteTransform.gameObject);
        }

        private void GameManager_onClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
        {
            SpawnObjectRpc(e.x,e.y, e.playerType);
        }

        [Rpc(SendTo.Server)]
        private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
        {
            Transform prefab;
            switch (playerType)
            {
                default:
                case GameManager.PlayerType.Cross:
                    prefab = crossPrefab;
                    break;
                case GameManager.PlayerType.Circle:
                    prefab = circlePrefab;
                    break;
            }
            Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
            spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true); 
            visualGameObjects.Add(spawnedCrossTransform.gameObject);
        }
        private Vector2 GetGridWorldPosition(int x, int y)
        {
            return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
        }
    }
}