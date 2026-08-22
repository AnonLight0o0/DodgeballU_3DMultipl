using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Player")]
    public string playerPrefabName = "PlayerBase";
    public Transform[] spawnPoints;

    [Header("Game State")]
    public bool GameOver = false;

    [Header("Game Over UI")]
    public GameObject gameOverUIPanel;
    public TextMeshProUGUI WinnerName;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();

            // Only the Master Client checks the win condition.
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CheckWinConditionRoutine());
            }
        }
        else
        {
            Debug.LogWarning("Not connected to Photon server.");
        }

        // Hide game over screen at the start.
        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(false);
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            spawnPosition = spawnPoints[randomIndex].position;
            spawnRotation = spawnPoints[randomIndex].rotation;
        }

        PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPosition,
            spawnRotation
        );
    }

    private IEnumerator CheckWinConditionRoutine()
    {
        while (!GameOver)
        {
            yield return new WaitForSeconds(1.0f);

            // Stop checking if Photon has disconnected.
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                continue;
            }

            // Prevent CurrentRoom null-reference errors.
            if (PhotonNetwork.CurrentRoom == null)
            {
                continue;
            }

            // Don't end a match with only one player.
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                CheckAlivePlayers();
            }
        }
    }

    private void CheckAlivePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        List<GameObject> alivePlayers = new List<GameObject>();

        foreach (GameObject player in players)
        {
            PlayerHP playerHP = player.GetComponent<PlayerHP>();

            if (playerHP != null && playerHP.CurrentHP > 0)
            {
                alivePlayers.Add(player);
            }
        }

        Debug.Log("Alive players: " + alivePlayers.Count);

        // One player remains.
        if (alivePlayers.Count == 1)
        {
            GameOver = true;

            PhotonView winnerView =
                alivePlayers[0].GetComponent<PhotonView>();

            string winnerName = "Unknown";

            if (winnerView != null && winnerView.Owner != null)
            {
                winnerName = winnerView.Owner.NickName;
            }

            Debug.Log("GAME OVER! Winner: " + winnerName);

            photonView.RPC(
                "RPC_ShowGameOver",
                RpcTarget.All,
                winnerName
            );
        }

        // Nobody remains.
        else if (alivePlayers.Count == 0)
        {
            GameOver = true;

            Debug.Log("GAME OVER! Draw.");

            photonView.RPC(
                "RPC_ShowGameOver",
                RpcTarget.All,
                "Oopsie, it's a draw."
            );
        }
    }

    [PunRPC]
    public void RPC_ShowGameOver(string winnerName)
    {
        Debug.Log(
            "RPC_ShowGameOver received on " +
            PhotonNetwork.NickName
        );

        // Unlock cursor for the game-over UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controls
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerCamera playerCamera = player.GetComponentInChildren<PlayerCamera>();

            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }

            PlayerDodgeballInteraction dodgeball =
                player.GetComponent<PlayerDodgeballInteraction>();

            if (dodgeball != null)
            {
                dodgeball.enabled = false;
            }
        }

        // Show game-over UI
        if (gameOverUIPanel == null)
        {
            Debug.LogError("Game Over UI Panel is NOT assigned in GameManager!");
            return;
        }

        gameOverUIPanel.SetActive(true);

        if (WinnerName != null)
        {
            WinnerName.text = $"The Winner is: {winnerName}";
        }

        Debug.Log("Game Over UI enabled.");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // If the previous Master Client left, the new Master Client
        // needs to take over win-condition checking.
        if (PhotonNetwork.IsMasterClient && !GameOver)
        {
            StartCoroutine(CheckWinConditionRoutine());
        }
    }
}