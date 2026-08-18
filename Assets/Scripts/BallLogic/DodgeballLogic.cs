using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DodgeballLogic : MonoBehaviourPun
{
    public float minLethalVelocity = 10f; //used so that the damage
    //public int damage = 1; obsolete for now, enable when needed
    private float maxSpeed = 1000f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && rb != null)
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // check speed
            if (collision.relativeVelocity.magnitude >= minLethalVelocity)
            {
                PlayerHP targetHP = collision.gameObject.GetComponent<PlayerHP>();
                if (targetHP != null)
                {
                    PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
                    if (targetView != null)
                    {
                        // get hp
                        targetHP.photonView.RPC("TakeDamage", RpcTarget.All/*, damage*/); //enable damage if need damage numbers other than 1
                        
                        if (PhotonNetwork.IsMasterClient)
                        {
                            rb.velocity = rb.velocity * 0.9f; 
                        }
                        
                        Debug.Log("The ball flying at the velocity of" + collision.relativeVelocity.magnitude + " has hit " + targetView.Owner.NickName);
                    }
                }
            }
            else
            {
                Debug.Log("too slow, no damage");
            }
        }
    }

    [PunRPC]
    public void DestroySelf()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}