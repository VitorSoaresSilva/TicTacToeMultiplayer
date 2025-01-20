using System;
using System.Collections.Generic;
using _Ravars.Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _TicTacToe.Scripts
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
        public event EventHandler OnGameStarted;
        public event EventHandler OnRematch;
        public event EventHandler OnGameTied;
        public event EventHandler OnScoreChanged;
        public event EventHandler OnPlacedObject;
        public event EventHandler<OnGameWinEventArgs> OnGameWin;
        public event EventHandler OnCurrentPlayablePlayerTypeChanged;

        private PlayerType localPlayerType;
        private NetworkVariable<PlayerType> currentPlayablePlayerType = new();
        private PlayerType[,] _playerTypeArray;
        private List<Line> lineList;
        private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
        private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

        protected override void Awake()
        {
            base.Awake();
            _playerTypeArray = new PlayerType[3, 3];
            lineList = new()
            {
                // Horizontal
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)},
                    centerGridPosition = new Vector2Int(1,0),
                    orientation = Orientation.Horizontal
                },
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1)},
                    centerGridPosition = new Vector2Int(1,1),
                    orientation = Orientation.Horizontal
                },
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)},
                    centerGridPosition = new Vector2Int(1,2),
                    orientation = Orientation.Horizontal
                },
                //vertical
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2)},
                    centerGridPosition = new Vector2Int(0,1),
                    orientation = Orientation.Vertical
                },
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2)},
                    centerGridPosition = new Vector2Int(1,1),
                    orientation = Orientation.Vertical
                },
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2)},
                    centerGridPosition = new Vector2Int(2,1),
                    orientation = Orientation.Vertical
                },
                // Diagonals
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2)},
                    centerGridPosition = new Vector2Int(1,1),
                    orientation = Orientation.DiagonalA
                },
                new()
                {
                    gridVector2IntList = new List<Vector2Int>(){ new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0)},
                    centerGridPosition = new Vector2Int(1,1),
                    orientation = Orientation.DiagonalB
                },
            };
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log(NetworkManager.Singleton.LocalClientId);
            localPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            }
            
            currentPlayablePlayerType.OnValueChanged += (PlayerType value, PlayerType newValue) =>
            {
                OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
            };
            playerCrossScore.OnValueChanged += (value, newValue) =>
            {
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
            };
            playerCircleScore.OnValueChanged += (value, newValue) =>
            {
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                // Start Game
                currentPlayablePlayerType.Value = PlayerType.Cross;
                TriggerOnGameStartedRpc();
            }
        }

        [Rpc(SendTo.Server)]
        public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
        {
            if (playerType != currentPlayablePlayerType.Value) return;
            if (_playerTypeArray[x, y] != PlayerType.None) return;
            
            _playerTypeArray[x, y] = playerType;
            TriggerOnPlacedObjectRpc();
            OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
            {
                x=x, 
                y=y, 
                playerType = playerType
            });

            switch (currentPlayablePlayerType.Value)
            {
                default:
                case PlayerType.None:
                case PlayerType.Cross:
                    currentPlayablePlayerType.Value = PlayerType.Circle;
                    break;
                case PlayerType.Circle:
                    currentPlayablePlayerType.Value = PlayerType.Cross;
                    break;
            }
            
            TestWinner();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnGameStartedRpc()
        {
            OnGameStarted?.Invoke(this, EventArgs.Empty);
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnPlacedObjectRpc()
        {
            OnPlacedObject?.Invoke(this, EventArgs.Empty);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
        {
            Line line = lineList[lineIndex];
            OnGameWin?.Invoke(this, new OnGameWinEventArgs()
            {
                line = line,
                winPlayerType = winPlayerType,
            });
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnRematchRpc()
        {
            OnRematch?.Invoke(this, EventArgs.Empty);
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnTiedRpc()
        {
            OnGameTied?.Invoke(this, EventArgs.Empty);
        }

        private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
        {
            return a != PlayerType.None &&
                a == b &&
                a == c;
        }

        private bool TestWinnerLine(Line line)
        {
            return TestWinnerLine(
                _playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
                _playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
                _playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
        }
        private void TestWinner()
        {
            for (var index = 0; index < lineList.Count; index++)
            {
                var line = lineList[index];
                if (TestWinnerLine(line))
                {
                    currentPlayablePlayerType.Value = PlayerType.None;
                    PlayerType winPlayerType = _playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                    switch (winPlayerType)
                    {
                        case PlayerType.Cross:
                            playerCrossScore.Value++;
                            break;
                        case PlayerType.Circle:
                            playerCircleScore.Value++;
                            break;
                    }
                    TriggerOnGameWinRpc(index, winPlayerType);
                    return;
                }
            }

            bool hasTie = true;
            for (int x = 0; x < _playerTypeArray.GetLength(0); x++)
            {
                for (int y = 0; y < _playerTypeArray.GetLength(1); y++)
                {
                    if (_playerTypeArray[x, y] == PlayerType.None)
                    {
                        hasTie = false;
                        break;
                    }
                }
            }

            if (hasTie)
            {
                TriggerOnTiedRpc();
            }
        }

        [Rpc(SendTo.Server)]
        public void RematchRpc()
        {
            _playerTypeArray = new PlayerType[3, 3]; // Or For> For = none
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnRematchRpc();
        }

        public PlayerType GetLocalPlayerType()
        {
            return localPlayerType;
        }
        public PlayerType GetCurrentPlayablePlayerType()
        {
            return currentPlayablePlayerType.Value;
        }
        public class OnClickedOnGridPositionEventArgs : EventArgs
        {
            public int x;
            public int y;
            public PlayerType playerType;
        }

        public class OnGameWinEventArgs : EventArgs
        {
            public Line line;
            public PlayerType winPlayerType;
        }

        public enum PlayerType
        {
            None,
            Cross,
            Circle,
        }

        public enum Orientation
        {
            Vertical,
            Horizontal,
            DiagonalA,
            DiagonalB
        }
        public struct Line
        {
            public List<Vector2Int> gridVector2IntList;
            public Vector2Int centerGridPosition;
            public Orientation orientation;
        }

        public void GetScores(out int playerCrossScore, out int playerCircleScore)
        {
            playerCrossScore = this.playerCrossScore.Value;
            playerCircleScore = this.playerCircleScore.Value;
        }
    }
}