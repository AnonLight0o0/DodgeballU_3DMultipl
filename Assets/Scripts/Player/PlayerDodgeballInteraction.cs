using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerDodgeballInteraction : MonoBehaviourPun
{
    public bool HasBall = false;
    public Transform throwPoint;
    public float ThrowForce = 300f;    
    [Tooltip("must same name as Dodgeball prefab in Resources folder")]
    public string ballPrefabName = "Dodgeball"; 
    public float BallCatchCooldown = 1.0f; 
    private float NextCatchTime = 0f; //can't use countdown or else might lag

    private Transform Cam;
    private List<GameObject> ballInReach = new List<GameObject>();

    void Start()
    {
        if (photonView.IsMine && Camera.main != null)
        {
            Cam = Camera.main.transform;
        }
    }

    void Update()
    {
        if (!photonView.IsMine || Cam == null) return;

        ballInReach.RemoveAll(ball => ball == null);

        if (Input.GetMouseButtonDown(0))
        {
            if (HasBall)
            {
                ThrowBall();
            }
            else if (!HasBall && Time.time >= NextCatchTime)
            {
                TryCatchBall();
            }
        }
    }

    private void ThrowBall()
    {
        HasBall = false;
        GameObject ball = PhotonNetwork.Instantiate(ballPrefabName, throwPoint.position, Cam.rotation);
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            Vector3 throwDir = Cam.forward + Vector3.up * 0.1f; 
            ballRb.velocity = throwDir.normalized * ThrowForce;
        }
    }

    private void TryCatchBall()
    {
        if (ballInReach.Count > 0)
        {
            GameObject ballToCatch = ballInReach[0];
            HasBall = true;
            Debug.Log("ball catched.");

            PhotonView ballView = ballToCatch.GetComponent<PhotonView>();
            if (ballView != null)
            {
                ballView.RPC("DestroySelf", RpcTarget.MasterClient); 
            }
            
            ballInReach.RemoveAt(0);
        }
        else
        {
            Debug.Log("ball not catched.");
            NextCatchTime = Time.time + BallCatchCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // only photon view on client side so won't lag die
        if (!photonView.IsMine) return;

        if (other.CompareTag("Ball") && !ballInReach.Contains(other.gameObject))
        {
            ballInReach.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.CompareTag("Ball") && ballInReach.Contains(other.gameObject))
        {
            ballInReach.Remove(other.gameObject);
        }
    }
}