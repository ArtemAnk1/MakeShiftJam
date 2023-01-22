using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //
    // Vector3 forwardWorld, rightWorld;
    // private void GetWorldsForward()
    // {
    //     forwardWorld = Camera.main.transform.forward;
    //     forwardWorld.y = 0;
    //     forwardWorld = Vector3.Normalize(forwardWorld);
    //     rightWorld = Quaternion.Euler(new Vector3(0, 90, 0)) * forwardWorld;
    // }
    //
    // public Transform target;
    // public float smoothTime = 0.3f;
    //
    // private Vector3 velocity = Vector3.zero;
    // private Vector3 offset;
    // private void Start()
    // {
    //     GetWorldsForward();
    //     target = GameObject.FindObjectOfType<Movement>().gameObject.transform;
    //     // offset = target.transform.position - this.transform.position;
    // }
    //
    // void Update()
    // {
    //     Vector3 goalPos = target.position;
    //     goalPos.y = transform.position.y;
    //     transform.position = Vector3.SmoothDamp(transform.world, goalPos, ref velocity, smoothTime);
    // }
    
    public GameObject player;
 
    private Vector3 offset;
 
    // Use this for initialization
    void Start () {
        offset = transform.position - player.transform.position;
    }
 
    public float smoothTime = 0.3f;
    
     private Vector3 velocity = Vector3.zero;
    void LateUpdate () {
        transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref velocity, smoothTime*Time.deltaTime);
        
    }
}
