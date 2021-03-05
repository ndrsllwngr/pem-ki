using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

    public GameObject player;        //Public variable to store a reference to the player game object


    private Vector3 _offset;            //Private variable to store the offset distance between the player and camera

    // Use this for initialization
    private void Start () 
    {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        _offset = transform.position - player.transform.position;
    }

    // LateUpdate is called after Update each frame
    private void LateUpdate () 
    {
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        transform.position = player.transform.position + _offset;
    }
}