using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCamera : MonoBehaviour
{
    private GameObject player; // Reference to the player's transform
    private Vector3 offset; // Offset from the player
    [SerializeField]
    public float smoothSpeed = 0.125f; // Smoothing speed for camera movement
    private Vector3 HoopLocation;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 lookingLocation = (player.transform.position + HoopLocation) * 0.5f;
        transform.LookAt(lookingLocation); // Optional: Make the camera look at the player
    }

    public void SetPlayer(GameObject player)
    {
        if (player == null) return;
        print($"Player is {player}");
        this.player = player;
        //offset = transform.position - player.transform.position;

        offset = player.transform.InverseTransformPoint(transform.position);
        print($"Offset is {offset}");
    }

    public void SetHoopPosition(Vector3 location)
    {
        HoopLocation = location;
    }

    public void SetPosition ()
    {
        transform.position = player.transform.TransformPoint(offset);
        transform.rotation = player.transform.rotation;
    }
}
