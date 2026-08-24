using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerHP : MonoBehaviourPun
{
    public GameObject HealthBar;
    private GameObject[] healthOrbs;

    public int MaxHP = 5;
    public int CurrentHP;

    void Start()
    {
        // Every networked player needs their HP initialized
        CurrentHP = MaxHP;

        // Only show the health bar for the local player
        if (!photonView.IsMine)
        {
            if (HealthBar != null)
                HealthBar.SetActive(false);

            return;
        }

        healthOrbs = new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            Transform orb = HealthBar.transform.Find("Health" + (i + 1));

            if (orb != null)
            {
                healthOrbs[i] = orb.gameObject;
            }
        }

        UpdateHealthUI();
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        // Debug: Press U to lose 1 HP
        if (Input.GetKeyDown(KeyCode.U))
        {
            TakeDamage();
        }
    }

    private void UpdateHealthUI()
    {
        if (!photonView.IsMine)
            return;

        if (healthOrbs == null)
            return;

        for (int i = 0; i < healthOrbs.Length; i++)
        {
            if (healthOrbs[i] != null)
            {
                healthOrbs[i].SetActive(i < CurrentHP);
            }
        }
    }

    // PunRPC is needed, don't delete
    [PunRPC]
    public void TakeDamage()
    {
        CurrentHP -= 1;

        // Only the owner updates their own health bar
        if (photonView.IsMine)
        {
            UpdateHealthUI();
        }

        Debug.Log(
            $"{photonView.Owner.NickName} was hit! Current HP: {CurrentHP}"
        );

        if (CurrentHP <= 0)
        {
            PlayerOut();
        }
    }

    private void PlayerOut()
    {
        //这边再具体会发生什么事就交给你了Ern Kean，玩家Out了后具体会发生什么动画还是消失什么的你就在这里播放吧~ （From: KW
        gameObject.SetActive(false); 
        Debug.Log($"Player {photonView.Owner.NickName} out!");
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        }
    }
}