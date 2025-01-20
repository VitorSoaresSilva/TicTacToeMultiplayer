using System;
using Unity.Services.Core;
using UnityEngine;

namespace _TicTacToe.Scripts.Cloud
{
    public class InitializationCloud : MonoBehaviour
    {
        private async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [ContextMenu("Status")]
        public void Status()
        {
            Debug.Log(UnityServices.State);
        }
    }
}