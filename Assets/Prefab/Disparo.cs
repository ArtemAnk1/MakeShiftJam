using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disparo : MonoBehaviour
{
    private Rigidbody rb;

    public bool normal;
    public float daño;
    public float speed;
    public Vector3 direccion= Vector3.zero;

    private BossPrimero scriptBoss;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
       scriptBoss = FindObjectOfType<BossPrimero>();
    }

    public void SetDirection(Vector3 direccionBoss)
    {
        direccion = direccionBoss;
        direccion.y = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (direccion != Vector3.zero)
        {
            rb.velocity = direccion * (speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<CollidersHijos>() != null)
        {
            bool weak = collision.gameObject.GetComponent<CollidersHijos>().esDebil;
            float multip = 1;
            if (weak) multip = scriptBoss.multipEspaldaDistancia;

            if (scriptBoss.stunned)
            {
                scriptBoss.ReceiveDamage(daño);
            }
            else
            {
                scriptBoss.ReceiveDamage(daño*multip); 
            }
           Destroy(this.gameObject);
            
        }
    }
}
