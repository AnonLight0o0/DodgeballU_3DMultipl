using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerDodgeballInteraction : MonoBehaviourPun
{
    [Header("Controls")]
    public bool ControlsEnabled = true;

    public bool HasBall = false;

    public Transform throwPoint;
    public float ThrowForce = 300f;

    [Header("Charge Throw")]
    public float ChargeDelay = 0.5f;
    public float MinThrowMultiplier = 0.5f;
    public float MaxThrowMultiplier = 1.5f;
    public float ChargeSpeed = 1f;

    [Header("UI")]
    public Slider ChargeSlider;

    [Tooltip("Must be the same name as the Dodgeball prefab in Resources.")]
    public string ballPrefabName = "Dodgeball";

    public float BallCatchCooldown = 1.0f;

    private float NextCatchTime = 0f;

    private Transform Cam;

    private List<GameObject> ballInReach =
        new List<GameObject>();

    private bool isCharging = false;
    private float holdStartTime;
    private float currentMultiplier = 1f;

    private bool startedClickWithBall = false;

    void Start()
    {
        if (photonView.IsMine &&
            Camera.main != null)
        {
            Cam = Camera.main.transform;
        }

        if (ChargeSlider != null)
        {
            ChargeSlider.minValue =
                MinThrowMultiplier;

            ChargeSlider.maxValue =
                MaxThrowMultiplier;

            ChargeSlider.value =
                MinThrowMultiplier;

            ChargeSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine ||
            Cam == null)
            return;

        // Stop all ball interaction after game over.
        if (!ControlsEnabled)
            return;

        ballInReach.RemoveAll(
            ball => ball == null
        );

        // Hide slider if player loses the ball.
        if (!HasBall &&
            ChargeSlider != null &&
            ChargeSlider.gameObject.activeSelf)
        {
            ChargeSlider.gameObject.SetActive(false);
        }

        // Mouse pressed
        if (Input.GetMouseButtonDown(0))
        {
            startedClickWithBall = HasBall;

            if (startedClickWithBall)
            {
                holdStartTime = Time.time;
                isCharging = false;
                currentMultiplier =
                    MinThrowMultiplier;
            }
            else if (Time.time >= NextCatchTime)
            {
                TryCatchBall();
            }
        }

        // Mouse held
        if (startedClickWithBall &&
            HasBall &&
            Input.GetMouseButton(0))
        {
            if (!isCharging &&
                Time.time - holdStartTime >= ChargeDelay)
            {
                isCharging = true;

                if (ChargeSlider != null)
                {
                    ChargeSlider.gameObject.SetActive(true);
                    ChargeSlider.value =
                        MinThrowMultiplier;
                }
            }

            if (isCharging)
            {
                float t =
                    Mathf.PingPong(
                        (Time.time -
                        holdStartTime -
                        ChargeDelay) *
                        ChargeSpeed,
                        1f
                    );

                currentMultiplier =
                    Mathf.Lerp(
                        MinThrowMultiplier,
                        MaxThrowMultiplier,
                        t
                    );

                if (ChargeSlider != null)
                {
                    ChargeSlider.value =
                        currentMultiplier;
                }
            }
        }

        // Mouse released
        if (startedClickWithBall &&
            HasBall &&
            Input.GetMouseButtonUp(0))
        {
            // Quick throw
            if (!isCharging)
            {
                currentMultiplier = 1f;
            }

            ThrowBall();

            startedClickWithBall = false;
            holdStartTime = 0f;
            isCharging = false;
            currentMultiplier =
                MinThrowMultiplier;

            if (ChargeSlider != null)
            {
                ChargeSlider.value =
                    MinThrowMultiplier;

                ChargeSlider.gameObject.SetActive(false);
            }
        }
    }

    private void ThrowBall()
    {
        HasBall = false;

        GameObject ball =
        PhotonNetwork.Instantiate(
            ballPrefabName,
            throwPoint.position,
            Cam.rotation
        );

        // Give the ball the same color as the player who threw it.
        PlayerColor playerColor =
            GetComponent<PlayerColor>();

        DodgeballLogic dodgeballLogic =
            ball.GetComponent<DodgeballLogic>();

        if (playerColor != null && dodgeballLogic != null)
        {
            Color color = playerColor.GetPlayerColor();

            dodgeballLogic.photonView.RPC(
                "RPC_SetBallColor",
                RpcTarget.All,
                color.r,
                color.g,
                color.b,
                color.a
            );
        }

        Rigidbody ballRb =
            ball.GetComponent<Rigidbody>();

        Collider ballCollider =
            ball.GetComponent<Collider>();

        if (ballCollider != null)
        {
            StartCoroutine(
                IgnoreThrowerCollision(
                    ballCollider
                )
            );
        }

        if (ballRb != null)
        {
            Vector3 throwDir =
                Cam.forward +
                Vector3.up * 0.1f;

            ballRb.velocity =
                throwDir.normalized *
                ThrowForce *
                currentMultiplier;
        }
    }

    private void TryCatchBall()
    {
        if (ballInReach.Count > 0)
        {
            GameObject ballToCatch =
                ballInReach[0];

            HasBall = true;

            Debug.Log("ball catched.");

            PhotonView ballView =
                ballToCatch.GetComponent<PhotonView>();

            if (ballView != null)
            {
                ballView.RPC(
                    "DestroySelf",
                    RpcTarget.All
                );
            }

            ballInReach.RemoveAt(0);
        }
        else
        {
            Debug.Log("ball not catched.");

            NextCatchTime =
                Time.time +
                BallCatchCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Ball") &&
            !ballInReach.Contains(other.gameObject))
        {
            ballInReach.Add(
                other.gameObject
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Ball") &&
            ballInReach.Contains(other.gameObject))
        {
            ballInReach.Remove(
                other.gameObject
            );
        }
    }

    private IEnumerator IgnoreThrowerCollision(
        Collider ballCollider)
    {
        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        foreach (
            Collider playerCollider
            in playerColliders)
        {
            Physics.IgnoreCollision(
                playerCollider,
                ballCollider,
                true
            );
        }

        yield return new WaitForSeconds(0.15f);

        foreach (
            Collider playerCollider
            in playerColliders)
        {
            if (playerCollider != null &&
                ballCollider != null)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    ballCollider,
                    false
                );
            }
        }
    }
}

/*public class PlayerDodgeballInteraction : MonoBehaviourPun
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
}*/