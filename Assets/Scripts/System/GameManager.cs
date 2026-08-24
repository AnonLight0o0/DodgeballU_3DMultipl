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

    private const string GAME_OVER_KEY = "GameOver";
    private const string WINNER_NAME_KEY = "WinnerName";

    private void Start()
    {
        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(false);
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Not connected to Photon server.");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            ResetGameState();
        }

        SpawnPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CheckWinConditionRoutine());
        }
    }

    private void SpawnPlayer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Make sure the index stays within the spawn point array.
        int spawnIndex = playerIndex % spawnPoints.Length;

        Transform spawnPoint = spawnPoints[spawnIndex];

        Debug.Log(
            PhotonNetwork.NickName +
            " spawning at Spawn Point " +
            spawnIndex
        );

        PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }

    private void ResetGameState()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        ExitGames.Client.Photon.Hashtable resetProperties =
            new ExitGames.Client.Photon.Hashtable
            {
                { GAME_OVER_KEY, false },
                { WINNER_NAME_KEY, "" }
            };

        PhotonNetwork.CurrentRoom.SetCustomProperties(resetProperties);

        GameOver = false;

        Debug.Log("Previous Game Over state reset.");
    }

    private IEnumerator CheckWinConditionRoutine()
    {
        while (!GameOver)
        {
            yield return new WaitForSeconds(1.0f);

            if (!PhotonNetwork.IsConnectedAndReady)
            {
                continue;
            }

            if (PhotonNetwork.CurrentRoom == null)
            {
                continue;
            }

            // Don't end the match with only one player.
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                CheckAlivePlayers();
            }
        }
    }

    private void CheckAlivePlayers()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        List<GameObject> alivePlayers =
            new List<GameObject>();

        foreach (GameObject player in players)
        {
            PlayerHP playerHP =
                player.GetComponent<PlayerHP>();

            if (playerHP != null && playerHP.CurrentHP > 0)
            {
                alivePlayers.Add(player);
            }
        }

        Debug.Log("Alive players: " + alivePlayers.Count);

        // One player remains.
        if (alivePlayers.Count == 1)
        {
            PhotonView winnerView =
                alivePlayers[0].GetComponent<PhotonView>();

            string winnerName = "Unknown";

            if (winnerView != null &&
                winnerView.Owner != null)
            {
                winnerName = winnerView.Owner.NickName;
            }

            EndGame(winnerName);
        }

        // Nobody remains.
        else if (alivePlayers.Count == 0)
        {
            EndGame("Oopsie, it's a draw.");
        }
    }

    private void EndGame(string winnerName)
    {
        // Prevent the Master Client from triggering this multiple times.
        if (GameOver)
            return;

        GameOver = true;

        Debug.Log("GAME OVER! Winner: " + winnerName);

        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("Cannot set Game Over state: no Photon room.");
            ShowGameOverLocally(winnerName);
            return;
        }

        // Store the game-over state in the Photon room.
        ExitGames.Client.Photon.Hashtable gameOverProperties =
            new ExitGames.Client.Photon.Hashtable
            {
                { GAME_OVER_KEY, true },
                { WINNER_NAME_KEY, winnerName }
            };

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            gameOverProperties
        );

        // Also show it immediately on the Master Client.
        ShowGameOverLocally(winnerName);
    }

    public override void OnRoomPropertiesUpdate(
        ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        // Check whether the GameOver property was updated.
        if (propertiesThatChanged.ContainsKey(GAME_OVER_KEY))
        {
            object gameOverValue =
                propertiesThatChanged[GAME_OVER_KEY];

            if (gameOverValue is bool &&
                (bool)gameOverValue)
            {
                string winnerName = "Unknown";

                if (PhotonNetwork.CurrentRoom != null &&
                    PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(
                        WINNER_NAME_KEY))
                {
                    winnerName =
                        PhotonNetwork.CurrentRoom
                            .CustomProperties[WINNER_NAME_KEY]
                            .ToString();
                }

                GameOver = true;

                ShowGameOverLocally(winnerName);
            }
        }
    }

    private void CheckExistingGameOverState()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(
            GAME_OVER_KEY))
        {
            return;
        }

        object gameOverValue =
            PhotonNetwork.CurrentRoom.CustomProperties[GAME_OVER_KEY];

        if (gameOverValue is bool &&
            (bool)gameOverValue)
        {
            string winnerName = "Unknown";

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(
                WINNER_NAME_KEY))
            {
                winnerName =
                    PhotonNetwork.CurrentRoom
                        .CustomProperties[WINNER_NAME_KEY]
                        .ToString();
            }

            GameOver = true;

            ShowGameOverLocally(winnerName);
        }
    }

    private void ShowGameOverLocally(string winnerName)
    {
        Debug.Log(
            "Showing Game Over UI for: " +
            PhotonNetwork.NickName
        );

        // Unlock cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable controls ONLY for this client's player.
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PhotonView playerView =
                player.GetComponent<PhotonView>();

            if (playerView == null ||
                !playerView.IsMine)
            {
                continue;
            }

            // Disable camera rotation.
            PlayerCamera playerCamera =
                Camera.main != null
                    ? Camera.main.GetComponent<PlayerCamera>()
                    : null;

            if (playerCamera != null)
            {
                playerCamera.ControlsEnabled = false;
            }

            // Disable movement.
            PlayerController playerController =
                player.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.ControlsEnabled = false;
            }

            // Disable throwing/catching.
            PlayerDodgeballInteraction dodgeball =
                player.GetComponent<PlayerDodgeballInteraction>();

            if (dodgeball != null)
            {
                dodgeball.ControlsEnabled = false;
            }
        }

        // Show the game-over UI locally.
        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "Game Over UI Panel is NOT assigned on " +
                PhotonNetwork.NickName
            );

            return;
        }

        // Set winner text locally.
        if (WinnerName != null)
        {
            WinnerName.text =
                $"The Winner is: {winnerName}";
        }

        Debug.Log(
            "Game Over UI enabled on: " +
            PhotonNetwork.NickName
        );
    }

    public override void OnMasterClientSwitched(
        Player newMasterClient)
    {
        // If the Master Client leaves before the match ends,
        // the new Master Client takes over win checking.
        if (PhotonNetwork.IsMasterClient && !GameOver)
        {
            StartCoroutine(CheckWinConditionRoutine());
        }
    }
}