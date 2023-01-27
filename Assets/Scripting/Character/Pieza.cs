using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pieza : MonoBehaviour
{

    public bool piezaA = false;

    public bool fusionAa= false;
    public bool fusionAb= false;
    public bool fusionBb= false;
    private GameObject player;
    private Rigidbody rb;
    public float speed=10;
    public float pickUpDistance = 2;
    public float activatedDistance = 3;
    public float forgetDistance = 6;
    private Player _player;

    private float startY;
    // Start is called before the first frame update
    void Start()
    { player = FindObjectOfType<Player>().gameObject;
        _player = player.GetComponent<Player>();
        startY=player.transform.position.y;
        rb = GetComponent<Rigidbody>();
    }



    public bool activated;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!activated&&Vector3.Distance(this.transform.position, player.transform.position) < activatedDistance)
        {
            activated = true;
        }
        if (activated&&Vector3.Distance(this.transform.position,player.transform.position)<pickUpDistance)
        {
            if (!_player.fullPiezas)
            {
                  _player.CogerPieza(this);
                            this.gameObject.SetActive((false));
            }
          
        }
        else if (activated)
        {
          
            if (Vector3.Distance(this.transform.position, player.transform.position) > forgetDistance-1&&Vector3.Distance(this.transform.position, player.transform.position)< forgetDistance)
            {
                Vector3 originPosWPlayerY = new Vector3(this.transform.position.x,startY,this.transform.position.z);
                rb.velocity = speed*0.5f * Time.deltaTime *   (player.transform.position-originPosWPlayerY).normalized;
            }else if (Vector3.Distance(this.transform.position, player.transform.position) > forgetDistance)
            {
                activated = false;
                rb.velocity = Vector3.zero;
            }else
            { 
                Vector3 originPosWPlayerY = new Vector3(this.transform.position.x,startY,this.transform.position.z);
                rb.velocity = speed * Time.deltaTime *   (player.transform.position-originPosWPlayerY).normalized;
            }

        }
    }
}
