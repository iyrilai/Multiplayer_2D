using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [SerializeField] float moveSpeed = 0.15f;

    void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

        MovePlayer();
    }

    void MovePlayer()
    {
        Vector3 moveInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.position += moveInput * moveSpeed;
    }
}
