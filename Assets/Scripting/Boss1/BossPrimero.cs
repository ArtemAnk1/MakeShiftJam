using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public bool fase2 = false;
    public Player player;
    private Rigidbody rb;
    [Space(30)] [Header("STATS")] [Space(30)]
    public float speed;

    public float maxTiempoMoviendose;

    public float tiempoMoviendose;

    public float cooldownTrasAtaque = 1f;

    private float auxCdTrasAtaque = 0;

    public float duracionAtaqueActual;
    
    public List<Ataques> ataquesEnemigo=new List<Ataques>();

    public float distanciaLarga = 50f;

    public float distanciaCorta = 15f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<Player>();
        vidaActual = vidaMax;
        esfuerzoActual = 0; 
        actualTiempoSinDañoMelee=0;
        VisualUpdateEsfuerzo();
        VisualUpdateHealth();
        ;
    }

    public enum EstadosBoss 
    {
        Movimiento,Atacando,Stun
        
    }
    public EstadosBoss estadoActual = EstadosBoss.Movimiento;
    void Ataca()
    {
        
    }
    private bool CheckDistancia()
    {
        bool cerca=false;
        return cerca;
    }

    void MoverseHaciaJugador()
    {
        
    }

    void RotarHaciaJugador()
    {
        
    }
   

    Ataques CalculaAtaque()
    {
        Ataques ataqueSelecc = new Ataques(); 
        List<Ataques> candidatos=new List<Ataques>();
        if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaCorta)
        {
          
            foreach (Ataques att in ataquesEnemigo)
            {
                if (att.nameAttack == "Embestida" || att.nameAttack == "GolpesSuelo" || att.nameAttack == "ComboSimple")
                {
                    candidatos.Add(att);
                }
            }
        }else  if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaLarga)
        {
            foreach (Ataques att in ataquesEnemigo)
            {
                if (att.nameAttack == "Embestida" || att.nameAttack == "Piedra" || att.nameAttack == "Laseres")
                {
                    candidatos.Add(att);
                }
            }  
        }else if (Vector3.Distance(this.transform.position, player.transform.position) > distanciaLarga)
        {
            foreach (Ataques att in ataquesEnemigo)
            {
                if (att.nameAttack == "Piedra" || att.nameAttack == "Laseres" )
                {
                    candidatos.Add(att);
                }
            }
        }


        ataqueSelecc = candidatos[Random.Range(0, candidatos.Count)];
       
        return ataqueSelecc;
    }

    public Ataques actualAtaque;

    public bool startedAttacking = false;
    public bool embestidaMoviendo = false;

    public Vector3 cachedPlayerPos;
    // Update is called once per frame
    void Update()
    {
        if (auxCdTrasAtaque > 0)
        {
            auxCdTrasAtaque -= Time.deltaTime;
            if (auxCdTrasAtaque < 0) auxCdTrasAtaque = 0;
        }
        if (!fase2)
        {
            switch (estadoActual)
            {
                case EstadosBoss.Movimiento:
                    RotarHaciaJugador();
                    tiempoMoviendose += Time.deltaTime;
                    if (tiempoMoviendose > maxTiempoMoviendose&&auxCdTrasAtaque<=0)
                    {
                        tiempoMoviendose = 0;
                        
                     
                        estadoActual = EstadosBoss.Atacando;
                        

                    }
                    else
                    {
                        if (CheckDistancia()&&auxCdTrasAtaque<=0)
                        {
                           
                            estadoActual = EstadosBoss.Atacando;
                            tiempoMoviendose = 0;
                        }
                        else
                        {
                            MoverseHaciaJugador();
                        }
                    }

                    break;
                case EstadosBoss.Atacando:
                    if (actualAtaque == null&&auxCdTrasAtaque<=0)
                    {
                        actualAtaque= CalculaAtaque();
                        startedAttacking = false;
                    }
                   else if(auxCdTrasAtaque==0&&actualAtaque!=null)
                   {
                       switch (actualAtaque.nameAttack)
                       {
                           case "Embestida":
                               if (startedAttacking == false)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga
                                   
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {
                                   duracionAtaqueActual -= Time.deltaTime;
                                   Vector3 desPos = (this.transform.position-player.transform.position).normalized;
                                   desPos.y = 0;//Cargando la embestida
                                   rb.velocity =  desPos* (Time.deltaTime * 80);
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!embestidaMoviendo)
                               {
                                   embestidaMoviendo = true;
                                   cachedPlayerPos = player.transform.position+(player.transform.position-this.transform.position).normalized*10;//duda del 1.2
                                   cachedPlayerPos.y = this.transform.position.y;
                                   cachedPlayerPos = cachedPlayerPos;
                               }

                               if (embestidaMoviendo)
                               {//Check si se stunea contra paredes
                                   float defaultSpeed = actualAtaque.speedAttack[0];
                                   if (Vector3.Distance(this.transform.position, cachedPlayerPos) <= 15)
                                   {
                                       defaultSpeed = defaultSpeed - (100 -
                                                                      Vector3.Magnitude(this.transform.position -
                                                                          cachedPlayerPos) * 2);
                                   }
                                   Vector3 desPos = (cachedPlayerPos-this.transform.position).normalized;
                                   desPos.y = 0;
                                   rb.velocity =  desPos * (Time.deltaTime * defaultSpeed);
                                   if (Vector3.Distance(this.transform.position, cachedPlayerPos) <= 5)
                                   {
                                       rb.velocity = Vector3.zero;
                                       embestidaMoviendo = false;
                                       startedAttacking = false;
                                       auxCdTrasAtaque = cooldownTrasAtaque;
                                       actualAtaque = null;
                                   }
                               }
                               break;
                           case "GolpesSuelo":
                               
                               break;
                           case "Laseres":
                               
                               break;
                           case "Piedra":
                               
                               break;
                           case "ComboSimple":
                               
                               break;
                           
                          
                       }
                   }
                    break;
                case EstadosBoss.Stun:
                    embestidaMoviendo = false;
                    startedAttacking = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
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
