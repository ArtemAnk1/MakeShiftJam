using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BossPrimero : MonoBehaviour
{
    public float vidaActual = 0;

    public float vidaMax;

    public float esfuerzoActual = 0;

    public float esfuerzoMax;
    
    public Image vidaFill;

    public Image esfuerzoFill;

    public float actualTiempoSinDañoMelee;
    

    public Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        vidaActual = vidaMax;
        esfuerzoActual = 0; 
        actualTiempoSinDañoMelee=0;
        VisualUpdateEsfuerzo();
        VisualUpdateHealth();
        ;
    }

    // Update is called once per frame
    void Update()
    {
        if (actualTiempoSinDañoMelee > 0)
        {
            actualTiempoSinDañoMelee -= Time.deltaTime;
            if (actualTiempoSinDañoMelee <= 0) actualTiempoSinDañoMelee = 0;
        }
    }

    void ReceiveDamage(float dmg)
    {
        if (vidaActual > 0)
        {
            vidaActual -= dmg;
            VisualUpdateHealth();
        }

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            VisualUpdateHealth();

            //Muerte
        }
    }
    void ReceiveEsfuerzo(float dmg)
    {
        esfuerzoActual += dmg;
        if (esfuerzoActual > esfuerzoMax)
        {
            print("stun");
            esfuerzoActual = esfuerzoMax;
            //inicia modo perder vida
        }

        VisualUpdateEsfuerzo();

    }

    void VisualUpdateHealth()
    {
        vidaFill.fillAmount = vidaActual / vidaMax;
    }

    void VisualUpdateEsfuerzo()
    {
        esfuerzoFill.fillAmount = esfuerzoActual / esfuerzoMax;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(actualTiempoSinDañoMelee<=0&&other.CompareTag("PlayerMelee"))
        {
            int ataqueSelecc = player.comboCount;
            ReceiveEsfuerzo(player.dañosNormales[ataqueSelecc]);
            actualTiempoSinDañoMelee = player.duracionDeAtaqueActual;
        }
       
    }

    private void OnTriggerStay(Collider other)
    {
       
            if(other.CompareTag("PlayerRanged"))
            {
              
                ReceiveDamage(player.dañosCargados[player.SeleccionDisparo()]*Time.deltaTime);
              
            }
       
    }
  
}
