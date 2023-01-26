using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
 
    private Vector3 offset;
 
    
    void Start () {
        offset = transform.position - player.transform.position;
    }
 
    public float smoothTime = 0.3f;
    
     private Vector3 velocity = Vector3.zero;

     private void Update()
     {
       
     }

     void LateUpdate () {
        transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref velocity, smoothTime*Time.deltaTime);
        
    }
}
