using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerBase"; 
    
    public Transform[] spawnPoints; 

    public bool GameOver = false;

    public GameObject gameOverUIPanel;
    public TextMeshProUGUI WinnerName;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
            StartCoroutine(CheckWinConditionRoutine());
        }
        else
        {
            Debug.LogWarning("Not connected to Photon server.");
        }

        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(false);
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnPosition = spawnPoints[randomIndex].position;
            spawnRotation = spawnPoints[randomIndex].rotation;
        }

        PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, spawnRotation);
    }

    IEnumerator CheckWinConditionRoutine()
    {
        while (!GameOver)
        {
            yield return new WaitForSeconds(1.0f);

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1) 
            {
                CheckAlivePlayers();
            }
        }
    }

    void CheckAlivePlayers()
    {
        GameObject[] alivePlayers = GameObject.FindGameObjectsWithTag("Player");

        if (alivePlayers.Length == 1)
        {
            GameOver = true;
            
            PhotonView winnerView = alivePlayers[0].GetComponent<PhotonView>();
            string winnerName = "Unknown";
            if (winnerView != null && winnerView.Owner != null)
            {
                winnerName = winnerView.Owner.NickName;
            }

            Debug.Log($"Game over, winner：{winnerName}！");

            photonView.RPC("RPC_ShowGameOver", RpcTarget.All, winnerName);
        }
        else if (alivePlayers.Length == 0)
        {
            GameOver = true;
            photonView.RPC("RPC_ShowGameOver", RpcTarget.All, "Oopsie, it's a draw.");
        }
    }

    [PunRPC]
    public void RPC_ShowGameOver(string winnerName)
    {
        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(true);
            if (WinnerName != null)
            {
                WinnerName.text = $"The Winner is: {winnerName}";
            }
        }
    }
}