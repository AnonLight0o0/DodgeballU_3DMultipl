using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerHP : MonoBehaviourPun
{
    public int MaxHP = 5;
    public int CurrentHP;

    void Start()
    {
        CurrentHP = MaxHP;
    }

    // PunRPC is needed, don't delete
    [PunRPC]
    public void TakeDamage(/*int damageNumbers*/)
    {
        CurrentHP -= 1; //要改数值的话记得把damageNumbers丢这里替代1
        Debug.Log($"{photonView.Owner.NickName} was hit! Current HP:{CurrentHP}");

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