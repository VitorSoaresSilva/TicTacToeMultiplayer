using System;
using TMPro;
using UnityEngine;

namespace _TicTacToe.Scripts
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject crossArrowGameObject;
        [SerializeField] private GameObject circleArrowGameObject;
        [SerializeField] private GameObject crossYouTextGameObject;
        [SerializeField] private GameObject circleYouTextGameObject;
        [SerializeField] private TextMeshProUGUI playerCrossScoreTextMesh;
        [SerializeField] private TextMeshProUGUI playerCircleScoreTextMesh;

        private void Awake()
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(false);
            crossYouTextGameObject.SetActive(false);
            circleYouTextGameObject.SetActive(false);
        }

        private void Start()
        {
            GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
            GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
            GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
            playerCrossScoreTextMesh.text = "";
            playerCircleScoreTextMesh.text = "";
        }

        private void GameManager_OnScoreChanged(object sender, EventArgs e)
        {
            GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);
            playerCrossScoreTextMesh.text = playerCrossScore.ToString();
            playerCircleScoreTextMesh.text = playerCircleScore.ToString();
        }

        private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
        {
            UpdateCurrentArrow();
        }

        private void GameManager_OnGameStarted(object sender, EventArgs e)
        {
            if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
            {
                crossYouTextGameObject.SetActive(true);
            }
            else
            {
                circleYouTextGameObject.SetActive(true);
            }
            playerCrossScoreTextMesh.text = "0";
            playerCircleScoreTextMesh.text = "0";
            UpdateCurrentArrow();
        }

        private void UpdateCurrentArrow()
        {
            if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
            {
                crossArrowGameObject.SetActive(true);
                circleArrowGameObject.SetActive(false);
            }
            else if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Circle)
            {
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(true);
            }
            else
            {
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(false);
            }
        }
    }
}
