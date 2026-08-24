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
        // Only Master Client handles dodgeball damage
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Check speed
        if (collision.relativeVelocity.magnitude < minLethalVelocity)
        {
            Debug.Log("Too slow, no damage");
            return;
        }

        PlayerHP targetHP =
            collision.gameObject.GetComponent<PlayerHP>();

        if (targetHP == null)
        {
            Debug.LogWarning(
                "Player was hit, but PlayerHP was not found on " +
                collision.gameObject.name
            );

            return;
        }

        PhotonView targetView =
            collision.gameObject.GetComponent<PhotonView>();

        if (targetView == null)
        {
            Debug.LogWarning(
                "Player was hit, but PhotonView was not found on " +
                collision.gameObject.name
            );

            return;
        }

        // Deal damage
        targetHP.photonView.RPC(
            "TakeDamage",
            RpcTarget.All
        );

        // Slow the ball down slightly after hitting a player
        if (rb != null)
        {
            rb.velocity = rb.velocity * 0.9f;
        }

        // Safely get the player's Photon name
        string targetName = "Unknown";

        if (targetView.Owner != null)
        {
            targetName = targetView.Owner.NickName;
        }

        Debug.Log(
            "The ball flying at the velocity of " +
            collision.relativeVelocity.magnitude +
            " has hit " +
            targetName
        );
    }

    [PunRPC]
    public void RPC_SetBallColor(
    float r,
    float g,
    float b,
    float a)
    {
        Color color =
            new Color(r, g, b, a);

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = color;
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