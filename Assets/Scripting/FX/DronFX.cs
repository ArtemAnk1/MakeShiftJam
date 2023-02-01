using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronFX : MonoBehaviour
{
    public float acceleration = 10.0f; 
    public float angle = 45.0f;
    float velocity = 1;
    public float startTime;
    float t = 0;
    public List<GameObject> slots = new List<GameObject>();
    public int disparosActuales;
    public GameObject pieza1;
    public GameObject pieza2;
    bool moving;int numAtaque;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Reload(1);
        }

        if (moving)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, slots[disparosActuales].transform.position, t / startTime);
            if (t > startTime)
            {
                disparosActuales++;
                moving = false;
                print("sdf");
                t = 0;
            }
        }
    }
    public void Reload(int i)
    {
        
        velocity *= acceleration;
        if (i == 1)
        {
            moving = true;
        }
        if (i == 2)
        {

        }
        if (i == 3)
        {

        }
    }
}
