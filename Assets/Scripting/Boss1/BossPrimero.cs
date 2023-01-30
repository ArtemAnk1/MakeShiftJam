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

    public float multipEspalda;
    public float multipEspaldaDistancia;
    public float multipDañoMeleSiStun;

    public bool stunned = false;
    public float tiempoStun = 23f;
    private float auxTiempoStun;
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
        if (stunned)
        {
            auxTiempoStun -= Time.deltaTime;
            if (auxTiempoStun <= 0)
            {
                auxTiempoStun = 0;
                stunned = false;
                esfuerzoActual = 0;
                VisualUpdateEsfuerzo();

            }
        }
        if (actualTiempoSinDañoMelee > 0)
        {
            actualTiempoSinDañoMelee -= Time.deltaTime;
            if (actualTiempoSinDañoMelee <= 0) actualTiempoSinDañoMelee = 0;
        }
    }

    public void ReceiveDamage(float dmg)
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
   public  void ReceiveEsfuerzo(float dmg)
    {
        if (stunned)
        {
            ReceiveDamage(dmg);
            return;
        }
        esfuerzoActual += dmg;
        if (esfuerzoActual > esfuerzoMax)
        {
            print("stun");
            esfuerzoActual = esfuerzoMax;
            stunned = true;
            auxTiempoStun = tiempoStun;
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
  
  
}
