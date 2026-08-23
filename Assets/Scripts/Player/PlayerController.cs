using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPun
{
    [Header("Controls")]
    public bool ControlsEnabled = true;

    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float jumpBufferTime = 0.3f;
    public float RepositionSpeed = 3f;

    private CharacterController controller;
    private Transform cam;
    private Vector3 velocity;
    private float jumpBufferCounter;

    void Start()
    {
        controller =
            GetComponent<CharacterController>();

        if (!photonView.IsMine)
            return;

        if (Camera.main != null)
        {
            cam = Camera.main.transform;

            PlayerCamera orbitCam =
                Camera.main.GetComponent<PlayerCamera>();

            if (orbitCam != null)
            {
                orbitCam.target = this.transform;
            }
        }
    }

    void Update()
    {
        // Only control our own player.
        if (!photonView.IsMine)
            return;

        // Stop player input when the game is over.
        if (!ControlsEnabled)
            return;

        if (cam == null)
        {
            Debug.Log("Camera is null!");
            return;
        }

        // Jump buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -1.5f;
            }

            if (jumpBufferCounter > 0f)
            {
                velocity.y =
                    Mathf.Sqrt(
                        jumpHeight * -2f * gravity
                    );

                jumpBufferCounter = 0;
            }
        }

        // Movement
        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        Vector3 inputDir =
            new Vector3(
                horizontal,
                0f,
                vertical
            ).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir =
                camForward * inputDir.z +
                camRight * inputDir.x;

            if (moveDir != Vector3.zero)
            {
                controller.Move(
                    moveDir *
                    moveSpeed *
                    Time.deltaTime
                );

                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDir);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * 10f
                    );
            }
        }
        else
        {
            Quaternion targetRotation =
                Quaternion.Euler(
                    0f,
                    transform.eulerAngles.y,
                    0f
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    RepositionSpeed *
                    Time.deltaTime
                );
        }

        velocity.y +=
            gravity * Time.deltaTime;

        controller.Move(
            velocity * Time.deltaTime
        );
    }
}