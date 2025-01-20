using UnityEngine;

namespace _TicTacToe.Scripts
{
    public class GridPosition : MonoBehaviour
    {
        [SerializeField] private int x;
        [SerializeField] private int y;
        private void OnMouseDown(){
            GameManager.Instance.ClickedOnGridPositionRpc(x,y, GameManager.Instance.GetLocalPlayerType());
        }
    }
}
