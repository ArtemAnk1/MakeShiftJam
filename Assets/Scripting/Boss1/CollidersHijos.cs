using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersHijos : MonoBehaviour
{
    private BossPrimero boss;

    private Player player;

    public bool esDebil = false;
    float multip = 1;
    // Start is called before the first frame update
    void Start()
    {  /*boss = FindObjectOfType<BossPrimero>();
        if (esDebil) multip = boss.multipEspalda;
        player = FindObjectOfType<Player>();*/
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        /*if(boss.actualTiempoSinDañoMelee<=0&&other.CompareTag("PlayerMelee"))
        {
           
            int ataqueSelecc = player.comboCount;
         if(boss.stunned)   boss.ReceiveEsfuerzo(player.dañosNormales[ataqueSelecc]*boss.multipDañoMeleSiStun);
         boss.ReceiveEsfuerzo(player.dañosNormales[ataqueSelecc]*multip);
            boss.actualTiempoSinDañoMelee = player.duracionDeAtaqueActual;
        }*/
       
    }

    private void OnTriggerStay(Collider other)
    {
   
        /*if(other.CompareTag("PlayerRanged"))
        {
              
            boss.ReceiveDamage(player.dañosCargados[player.SeleccionDisparo()]*Time.deltaTime*multip);
              
        }*/
       
    }
}
