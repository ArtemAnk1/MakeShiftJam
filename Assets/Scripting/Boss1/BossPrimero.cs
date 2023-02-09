using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
    public float movementSPEED=40;

    public float maxTiempoMoviendose;

    public float tiempoMoviendose;

    private float _cooldownTrasAtaque = 1f;
    public float modCooldownTrasAtaques = 2f;
    private float auxCdTrasAtaque = 0;

    public float duracionAtaqueActual;
    
    public List<Ataques> ataquesEnemigo=new List<Ataques>();
    public List<Ataques> ataquesEnemigoF2=new List<Ataques>();
    public float distanciaLarga = 50f;

    public float distanciaCorta = 15f;
    public float tiempoTransformacionFase2 = 5f;
    public float auxTiempoTransformacion = 0;
    public Animator anim;
    public float cooldownTrasAtaque
    {
        get => _cooldownTrasAtaque+Random.Range(-modCooldownTrasAtaques,modCooldownTrasAtaques);
        set => _cooldownTrasAtaque = value;
    }

    public float distanciaDeteccionParaAtaque = 25;
    
    
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
        anim=GetComponent<Animator>();
    }

    public enum EstadosBoss 
    {
        Movimiento,Atacando,Stun
        
    }
    public EstadosBoss estadoActual = EstadosBoss.Movimiento;
  
    private bool CheckDistancia()
    {
        bool cerca=false;
        if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaDeteccionParaAtaque)
        {
            cerca = true;
        }

        return cerca;
    }

  
    void MoverseHaciaJugador()
    {
        Vector3 desPos = (player.transform.position-this.transform.position).normalized;
        desPos.y = 0;
        rb.velocity =  desPos * (Time.deltaTime * movementSPEED);
        anim.SetFloat("Speed",rb.velocity.magnitude);
    }

     public float rotationSpeed = 200;
    void RotarHaciaJugador()
    {
        var desPos = new Vector3(player.transform.position.x, this.transform.position.y,
            player.transform.position.z);
        var rotDesired = Quaternion.LookRotation(desPos - this.transform.position, Vector3.up);
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotDesired,
            rotationSpeed * Time.deltaTime); 
        anim.SetFloat("Rotation",rb.velocity.magnitude);
    }
   

    Ataques CalculaAtaque()
    { Ataques ataqueSelecc = ScriptableObject.CreateInstance<Ataques>(); 
             List<Ataques> candidatos=new List<Ataques>();
        if (!fase2)
        {
            
      
       
        if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaCorta)
        {
            foreach (Ataques att in ataquesEnemigo)
            {
                if (att.nameAttack == "Embestida" || att.nameAttack == "GolpeSuelo" || att.nameAttack == "ComboSimple")//piedra corta distancia
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

  }
        else
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaCorta)
            {
                foreach (Ataques att in ataquesEnemigoF2)
                {
                    if (att.nameAttack == "EmbestidaF2" || att.nameAttack == "GolpeSueloF2" || att.nameAttack == "ComboSimpleF2"|| att.nameAttack == "PiedraGorda")//piedra corta distancia
                    {
                        candidatos.Add(att);
                    }
                }
            }else  if (Vector3.Distance(this.transform.position, player.transform.position) < distanciaLarga)
            {
                foreach (Ataques att in ataquesEnemigoF2)
                {
                    if (att.nameAttack == "EmbestidaF2" || att.nameAttack == "GolpeSueloF2" || att.nameAttack == "LaseresF2"|| att.nameAttack == "PiedraGorda")
                    {
                        candidatos.Add(att);
                    }
                }  
            }else if (Vector3.Distance(this.transform.position, player.transform.position) > distanciaLarga)
            {
                foreach (Ataques att in ataquesEnemigoF2)
                {
                    if (att.nameAttack == "GolpeSueloF2" || att.nameAttack == "LaseresF2" )
                    {
                        candidatos.Add(att);
                    }
                }
            }  
        }
        ataqueSelecc = candidatos[Random.Range(0, candidatos.Count)];
       
        return ataqueSelecc;
      
    }

    public Ataques actualAtaque;

    public bool startedAttacking = false;
    public bool embestidaMoviendo = false;
    public bool esperandoDesactivarArea = false;
    public Vector3 cachedPlayerPos;

    public GameObject manoLanzaPiedra;
    public GameObject otraLanzaPiedraGorda;
    private float auxCdAtaque=0;
    private GameObject hijoArea;
    private GameObject piedra;

    public LineRenderer lineRend;

    public GameObject lineRendGO;

    public float cooldownEntreLaseres = 1;
    private float auxCdLaseres = 0;
    public int laserCount = 0;
    public float cooldownEntreSimples = 1;
    public float cooldownEntreSimplesF2 = 0.8f;
    private float auxCdSimples = 0;
    public int simpleCount = 0;
    public GameObject combo1;
    public GameObject combo2;
    public GameObject combo3;
    public GameObject puntoLaserCabeza;

    public int embestidasCountF2 = 0;

    public int laseresCountF2 = 0;
    // Update is called once per frame

    void Update()
    {
        if (auxTiempoTransformacion > 0)
        {
            auxTiempoTransformacion -= Time.deltaTime;
            if (auxTiempoTransformacion < 0)
            {DeactivateEverything();
                FinishAttack();
                auxTiempoTransformacion = 0;
            }
        }
    }
    bool AnimatorIsPlaying(){
        return anim.GetCurrentAnimatorStateInfo(0).length >
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    bool AnimatorIsPlaying(string stateName){
        return AnimatorIsPlaying() && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
    void FixedUpdate()
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
                        if (CheckDistancia()/*&&auxCdTrasAtaque<=0*/)
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
                        rb.velocity = Vector3.zero;
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
                               {   RotarHaciaJugador();
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if(duracionAtaqueActual<0.8f&&!AnimatorIsPlaying("Embestida"))anim.SetBool("Embestida",true);
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
                                       anim.SetBool("Embestida",false);
                                       FinishAttack();
                                   }
                               }
                               break;
                           case "GolpeSuelo":
                                 if (startedAttacking == false)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                  
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if (duracionAtaqueActual < 2f && !AnimatorIsPlaying("GolpeSueloIzq") &&
                                       !AnimatorIsPlaying("GolpeSueloDer"))
                                   {
                                      int r= Random.Range(0, 2);
                                      if (r == 1)
                                      {
                                          anim.SetTrigger(("GolpeSueloIzq"));
                                      }
                                      else
                                      {
                                          anim.SetTrigger(("GolpeSueloDer"));
                                      }
                                   }
                                   if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.6f)RotarHaciaJugador();
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                    hijoArea = Instantiate(actualAtaque.prefab1, this.transform.position,
                                       Quaternion.identity);
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//EL TIEMPO QUE DURA EL AREAS
                                   esperandoDesactivarArea = true;
                               }

                                 if (esperandoDesactivarArea)
                                 { if(duracionAtaqueActual>actualAtaque.duracionAttack[1]*0.5f)RotarHaciaJugador();
                                     auxCdAtaque -= Time.deltaTime;
                                     if (auxCdAtaque < 0)
                                     {if(hijoArea!=null)hijoArea.SetActive((false));
                                         hijoArea = null;
                                          GameObject p=Instantiate(actualAtaque.pieza, this.transform.position,
                                                                                        Quaternion.identity);
                                         esperandoDesactivarArea = false;
                                        embestidaMoviendo = false;
                                        anim.ResetTrigger(("GolpeSueloDer"));
                                        anim.ResetTrigger(("GolpeSueloIzq"));
                                         FinishAttack();
                                         auxCdTrasAtaque += 1;

                                     }
                                 }
                             
                                      
                               
                               break;
                           case "Laseres":
                               if (startedAttacking == false&&auxCdLaseres==0)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                  anim.SetTrigger("Laseres");
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.3f)RotarHaciaJugador();
                                   if (duracionAtaqueActual > actualAtaque.duracionAttack[0] * 0.3f)
                                   {
                                       cachedPlayerPos = player.transform.position;
                                   }
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                   Vector3 desPos=cachedPlayerPos+(cachedPlayerPos-puntoLaserCabeza.transform.position).normalized*50;
                                   desPos.y = player.transform.position.y;
                                   lineRendGO.SetActive(true);
                                 lineRend.SetPosition(0,puntoLaserCabeza.transform.position);
                                 lineRend.SetPosition(1,desPos);
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//EL TIEMPO QUE DURA EL LASER
                                   esperandoDesactivarArea = true;
                               }

                               if (esperandoDesactivarArea)
                               { if (Physics.SphereCast(puntoLaserCabeza.transform.position,6f,lineRend.GetPosition(1)-lineRend.GetPosition(0), out RaycastHit t, 1000))
                                   {  
                                       Debug.DrawRay(transform.position,new Vector3((lineRend.GetPosition(1)-lineRend.GetPosition(0)).normalized.x,0, ((lineRend.GetPosition(1)-lineRend.GetPosition(0)).normalized.z))*50f,Color.red,2f);
                                       player.GetComponent<Player>().RecibirDaño("Laser");
                                    
                                   }
                                   auxCdAtaque -= Time.deltaTime;
                                   if (auxCdAtaque < 0&&laserCount==0)
                                   { lineRendGO.SetActive(false);
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                       anim.ResetTrigger("Laseres");
                                       FinishAttack();
                                       auxCdLaseres = 0;
                                       laserCount = 0;
                                       //auxCdTrasAtaque += 1;
                                       break;
                                   }
                                   // if (auxCdAtaque < 0&&laserCount<2)
                                   // { lineRendGO.SetActive(false);
                                   //     hijoArea = null;
                                   //     esperandoDesactivarArea = false;
                                   //     embestidaMoviendo = false;
                                   //     //FinishAttack();
                                   //     
                                   //     RepeatLaser();
                                   //    
                                   //     //auxCdTrasAtaque += 1;
                                   //
                                   // }
                                 
                               }

                               if (auxCdLaseres > 0)
                               {RotarHaciaJugador();
                                   auxCdLaseres -= Time.deltaTime;
                                   if (auxCdLaseres < 0)
                                   {
                                       auxCdLaseres = 0;
                                   }
                               }

                               break;
                           case "Piedra":
                                  if (startedAttacking == false)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga
                                   
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {   RotarHaciaJugador();
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if (piedra == null)
                                   {
                                       piedra = Instantiate(actualAtaque.prefab1, manoLanzaPiedra.transform.position,Quaternion.identity);
                                   }
                                   anim.SetTrigger(("Piedra"));
                                  piedra.transform.parent = manoLanzaPiedra.transform;
                                  piedra.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                  //CARGANDO PIEDRA
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&auxCdAtaque==0)
                               {
                                 
                                   cachedPlayerPos = player.transform.position+(player.transform.position-this.transform.position).normalized*2;//duda del 1.2
                                   cachedPlayerPos.y = player.transform.position.y;
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//TIEMPO DE LANZAMIENTO DE LA PIEDRA PARA ANIMACION
                                   //Animacion Lanza Piedra
                               }

                                  if (auxCdAtaque > 0)
                                  { 
                                   auxCdAtaque -= Time.deltaTime;
                                  
                                     
                                  
                               }
                                  else if(auxCdAtaque<0)
                                  {
                                     
                                       if (piedra.transform.parent != null)
                                       {
                                           piedra.transform.parent=null;
                                           piedra.transform.position=new Vector3(piedra.transform.position.x,player.transform.position.y,piedra.transform.position.z);

                                       }
                                       float defaultSpeed = actualAtaque.speedAttack[0];
                                       if (Vector3.Distance(piedra.transform.position, cachedPlayerPos) <=15)
                                       {
                                           defaultSpeed = defaultSpeed - (100 -
                                                                          Vector3.Magnitude(piedra.transform.position -
                                                                              cachedPlayerPos) );
                                       }
                                       Vector3 desPos = (cachedPlayerPos-piedra.transform.position).normalized;
                                       desPos.y = 0;
                                       piedra.GetComponent<Rigidbody>().velocity =  desPos * (Time.deltaTime * defaultSpeed);
                                       cachedPlayerPos.y = piedra.transform.position.y;
                                       print("PEDRUSCO"+" DISTANCIA: "+Vector3.Distance(piedra.transform.position, cachedPlayerPos) +" MAGNITUD: "+(piedra.transform.position- cachedPlayerPos).magnitude);
                                       if ((piedra.transform.position- cachedPlayerPos).magnitude <= 2)
                                       { anim.ResetTrigger(("Piedra"));
                                           rb.velocity = Vector3.zero;
                                           //ANIM PIEDRA SE ROMPE
                                           GameObject p=Instantiate(actualAtaque.pieza, piedra.transform.position,
                                               Quaternion.identity);
                                           p.transform.position= new Vector3(piedra.transform.position.x,player.transform.position.y,piedra.transform.position.z);
                                           Destroy(piedra,0.5f);
                                           piedra = null;
                                         FinishAttack();
                                       }
                                       
                                   
                                  }
                               break;
                           case "ComboSimple":
                                if (startedAttacking == false&&auxCdSimples==0)
                               {
                                   startedAttacking = true;
                                 if(simpleCount==0)  duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                 if(simpleCount==1)  duracionAtaqueActual = actualAtaque.duracionAttack[1];
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  MoverseHaciaJugador();
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if(simpleCount==0)
                                   {
                                      if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.2f)RotarHaciaJugador();
                                                                        if(duracionAtaqueActual<actualAtaque.duracionAttack[0]*0.8f)RotarHaciaJugador();  
                                   }
                                   if(simpleCount==1)
                                   {
                                       if(duracionAtaqueActual>actualAtaque.duracionAttack[1]*0.2f)RotarHaciaJugador();
                                       if(duracionAtaqueActual<actualAtaque.duracionAttack[1]*0.8f)RotarHaciaJugador();  
                                   }
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                   if (simpleCount == 0)
                                   {
                                       combo1.SetActive((true));
                                       auxCdAtaque = actualAtaque.duracionAttack[2];//EL TIEMPO QUE DURAN LOS ATAQUIES
                                       
                                   }
                                   else  if (simpleCount == 1)
                                   {
                                       combo2.SetActive((true));
                                       auxCdAtaque = actualAtaque.duracionAttack[3];  //EL TIEMPO QUE DURA EL ATAQUE 
                                   }
                                 
                              
                                   esperandoDesactivarArea = true;
                               }

                               if (esperandoDesactivarArea)
                               { 
                                   auxCdAtaque -= Time.deltaTime;
                                   if (auxCdAtaque < 0&&simpleCount==1)
                                   {   combo2.SetActive((false));
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                       FinishAttack();
                                       auxCdSimples = 0;
                                       simpleCount = 0;
                                       //auxCdTrasAtaque += 1;
                                       break;
                                   }
                                   if (auxCdAtaque < 0&&simpleCount<1)
                                   { combo1.SetActive((false));
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                     //REPETIR ATAWQUE
                                       
                                       auxCdAtaque = 0;
                                       duracionAtaqueActual = 0;
                                       startedAttacking = false;
                                       
                                      
        
                                       simpleCount += 1;
                                       auxCdSimples = cooldownEntreSimples;
                                      
                                       //auxCdTrasAtaque += 1;

                                   }
                                 
                               }

                               if (auxCdSimples > 0)
                               {RotarHaciaJugador();
                                   auxCdSimples -= Time.deltaTime;
                                   if (auxCdSimples < 0)
                                   {
                                       auxCdSimples = 0;
                                   }
                               }
                               break;
                           
                          
                       }
                   }
                    else
                    {
                        auxCdTrasAtaque -= Time.deltaTime;
                        if (auxCdTrasAtaque < 0) auxCdTrasAtaque = 0;
                    }
                    break;
                case EstadosBoss.Stun:
                    rb.velocity = Vector3.zero;
                    embestidaMoviendo = false;
                    startedAttacking = false;
                    if(piedra!=null)Destroy(piedra,1f);
                    piedra = null;
                    auxCdAtaque = 0;
                    duracionAtaqueActual = 0;
                    startedAttacking = false;
                    auxCdTrasAtaque = cooldownTrasAtaque;
                    actualAtaque = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        if (fase2 && auxTiempoTransformacion == 0)
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
                        if (CheckDistancia()/*&&auxCdTrasAtaque<=0*/)
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
                        rb.velocity = Vector3.zero;
                        actualAtaque= CalculaAtaque();
                        anim.SetFloat("Speed",0);
                        startedAttacking = false;
                    }
                   else if(auxCdTrasAtaque==0&&actualAtaque!=null)
                   {
                       switch (actualAtaque.nameAttack)
                       {
                           case "EmbestidaF2":
                               if (startedAttacking == false&&auxCdAtaque==0)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga
                                   
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {   RotarHaciaJugador();
                                   if(duracionAtaqueActual<0.8f&&!AnimatorIsPlaying("Embestida"))anim.SetBool("Embestida",true);
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
                                       if (embestidasCountF2 < 2)
                                       {
                                           auxCdAtaque = actualAtaque.duracionAttack[1];
                                           duracionAtaqueActual = 0;
                                           startedAttacking = false;
                                           
                                           embestidasCountF2 += 1;
                                           anim.SetBool("Embestida",false);

                                       }
                                       else
                                       {
                                           anim.SetBool("Embestida",false);
                                            FinishAttack();
                                       }
                                   }
                               }

                               if (auxCdAtaque > 0)
                               {
                                   auxCdAtaque -= Time.deltaTime;
                                   if (auxCdAtaque < 0)
                                   {
                                       auxCdAtaque = 0;
                                   }
                               }
                               break;
                           case "GolpeSueloF2":
                                 if (startedAttacking == false)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                  
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if (duracionAtaqueActual < 0.9f && !AnimatorIsPlaying("GolpeSueloIzq") &&
                                       !AnimatorIsPlaying("GolpeSueloDer"))
                                   {
                                       int r= Random.Range(0, 2);
                                       if (r == 1)
                                       {
                                           anim.SetTrigger(("GolpeSueloIzq"));
                                       }
                                       else
                                       {
                                           anim.SetTrigger(("GolpeSueloDer"));
                                       }
                                   }
                                   if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.6f)RotarHaciaJugador();
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                    hijoArea = Instantiate(actualAtaque.prefab1, this.transform.position,
                                       Quaternion.identity);
                                    CrearBolasTecho();
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//EL TIEMPO QUE DURA EL AREAS
                                   esperandoDesactivarArea = true;
                               }

                                 if (esperandoDesactivarArea)
                                 { if(duracionAtaqueActual>actualAtaque.duracionAttack[1]*0.5f)RotarHaciaJugador();
                                     auxCdAtaque -= Time.deltaTime;
                                     
                                     if (auxCdAtaque < 0)
                                     {if(hijoArea!=null)hijoArea.SetActive((false));
                                         hijoArea = null;
                                         GameObject p=Instantiate(actualAtaque.pieza, this.transform.position,
                                             Quaternion.identity);
                                         esperandoDesactivarArea = false;
                                        embestidaMoviendo = false;
                                        anim.ResetTrigger(("GolpeSueloDer"));
                                        anim.ResetTrigger(("GolpeSueloIzq"));
                                         FinishAttack();
                                         auxCdTrasAtaque += 1;

                                     }
                                 }
                             
                                      
                               
                               break;
                            case "PiedraGorda":
                                  if (startedAttacking == false)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga
                                   
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {   RotarHaciaJugador();
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if (piedra == null)
                                   {
                                       piedra = Instantiate(actualAtaque.prefab1, otraLanzaPiedraGorda.transform.position,Quaternion.identity);
                                       anim.SetTrigger(("PiedraGorda"));
                                   }
                                  piedra.transform.parent = otraLanzaPiedraGorda.transform;
                                  piedra.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                  //CARGANDO PIEDRA
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&auxCdAtaque==0)
                               {
                                 
                                   cachedPlayerPos = player.transform.position+(player.transform.position-this.transform.position).normalized*2;//duda del 1.2
                                   cachedPlayerPos.y = player.transform.position.y;
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//TIEMPO DE LANZAMIENTO DE LA PIEDRA PARA ANIMACION
                                   //Animacion Lanza Piedra
                               }

                                  if (auxCdAtaque > 0)
                                  { 
                                   auxCdAtaque -= Time.deltaTime;
                                  
                                     
                                  
                               }
                                  else if(auxCdAtaque<0)
                                  {
                                     
                                       if (piedra.transform.parent != null)
                                       {
                                           piedra.transform.parent=null;
                                           piedra.transform.position=new Vector3(piedra.transform.position.x,player.transform.position.y,piedra.transform.position.z);

                                       }
                                       float defaultSpeed = actualAtaque.speedAttack[0];
                                       if (Vector3.Distance(piedra.transform.position, cachedPlayerPos) <=15)
                                       {
                                           defaultSpeed = defaultSpeed - (100 -
                                                                          Vector3.Magnitude(piedra.transform.position -
                                                                              cachedPlayerPos) );
                                       }
                                       Vector3 desPos = (cachedPlayerPos-piedra.transform.position).normalized;
                                       desPos.y = 0;
                                       piedra.GetComponent<Rigidbody>().velocity =  desPos * (Time.deltaTime * defaultSpeed);
                                       cachedPlayerPos.y = piedra.transform.position.y;
                                       print("PEDRUSCO"+" DISTANCIA: "+Vector3.Distance(piedra.transform.position, cachedPlayerPos) +" MAGNITUD: "+(piedra.transform.position- cachedPlayerPos).magnitude);
                                       if ((piedra.transform.position- cachedPlayerPos).magnitude <= 4)
                                       { anim.ResetTrigger(("PiedraGorda"));
                                           rb.velocity = Vector3.zero;
                                           //ANIM PIEDRA SE ROMPE
                                           GameObject p=Instantiate(actualAtaque.pieza, piedra.transform.position,
                                               Quaternion.identity);
                                           p.transform.position= new Vector3(piedra.transform.position.x,player.transform.position.y,piedra.transform.position.z);
                                           Destroy(piedra,1f);
                                           piedra = null;
                                         FinishAttack();
                                       }
                                       
                                   
                                  }

                                  break;
                           case "LaseresF2":
                               if (startedAttacking == false&&auxCdLaseres==0)
                               {
                                   startedAttacking = true;
                                   duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                   anim.SetTrigger("Laseres");
                               }
                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.3f)RotarHaciaJugador();
                                   if (duracionAtaqueActual > actualAtaque.duracionAttack[0] * 0.3f)
                                   {
                                       cachedPlayerPos = player.transform.position;
                                   }
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                   Vector3 desPos=cachedPlayerPos+(cachedPlayerPos-puntoLaserCabeza.transform.position).normalized*50;
                                   desPos.y = player.transform.position.y;
                                   lineRendGO.SetActive(true);
                                   lineRend.SetPosition(0,puntoLaserCabeza.transform.position);
                                   lineRend.SetPosition(1,desPos);
                                   auxCdAtaque = actualAtaque.duracionAttack[1];//EL TIEMPO QUE DURA EL LASER
                                   esperandoDesactivarArea = true;
                               }

                               if (esperandoDesactivarArea)
                               {  if (Physics.SphereCast(puntoLaserCabeza.transform.position,6f,lineRend.GetPosition(1)-lineRend.GetPosition(0), out RaycastHit t, 1000))
                                   {  
                                       Debug.DrawRay(transform.position,new Vector3((lineRend.GetPosition(1)-lineRend.GetPosition(0)).normalized.x,0, ((lineRend.GetPosition(1)-lineRend.GetPosition(0)).normalized.z))*50f,Color.red,2f);
                                      player.GetComponent<Player>().RecibirDaño("Laser");
                                    
                                   }
                                   auxCdAtaque -= Time.deltaTime;
                                   if (auxCdAtaque < 0&&laserCount==2)
                                   { lineRendGO.SetActive(false);
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                       if (laseresCountF2 == 1)
                                       {anim.ResetTrigger("Laseres");
                                           FinishAttack();
                                           laserCount = 0;
                                           auxCdLaseres = 0;
                                           laseresCountF2 = 0;
                                       }
                                       else
                                       {
                                             RepeatLaser();
                                                                                  auxCdLaseres = cooldownEntreLaseres+1f;
                                                                                  laserCount = 0;
                                                                                  laseresCountF2 += 1;
                                       }
                                   
                                       //auxCdTrasAtaque += 1;
                                       break;
                                   }
                                   if (auxCdAtaque < 0&&laserCount<2)
                                   { lineRendGO.SetActive(false);
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                       //FinishAttack();
                                       
                                       RepeatLaser();
                                       auxCdLaseres = 0;
                                       //auxCdTrasAtaque += 1;

                                   }
                                 
                               }

                               if (auxCdLaseres > 0)
                               {RotarHaciaJugador();
                                   auxCdLaseres -= Time.deltaTime;
                                   if (auxCdLaseres < 0)
                                   {
                                       auxCdLaseres = 0;
                                   }
                               }

                               break;
                          
                           case "ComboSimpleF2":
                                if (startedAttacking == false&&auxCdSimples==0)
                               {
                                   startedAttacking = true;
                                 if(simpleCount==0)  duracionAtaqueActual = actualAtaque.duracionAttack[0];//Duracion attack es tiempo de carga antes de golpear
                                 if(simpleCount==1)  duracionAtaqueActual = actualAtaque.duracionAttack[1];
                                 if(simpleCount==2)  duracionAtaqueActual = actualAtaque.duracionAttack[4];
                               }

                               if (duracionAtaqueActual > 0 && startedAttacking)
                               {  MoverseHaciaJugador();
                                   duracionAtaqueActual -= Time.deltaTime;
                                   if(simpleCount==0)
                                   {
                                      if(duracionAtaqueActual>actualAtaque.duracionAttack[0]*0.2f)RotarHaciaJugador();
                                                                        if(duracionAtaqueActual<actualAtaque.duracionAttack[0]*0.8f)RotarHaciaJugador();  
                                   }
                                   if(simpleCount==1)
                                   {
                                       if(duracionAtaqueActual>actualAtaque.duracionAttack[1]*0.2f)RotarHaciaJugador();
                                       if(duracionAtaqueActual<actualAtaque.duracionAttack[1]*0.8f)RotarHaciaJugador();  
                                   }
                                   if(simpleCount==2)
                                   {
                                       if(duracionAtaqueActual>actualAtaque.duracionAttack[4]*0.2f)RotarHaciaJugador();
                                       if(duracionAtaqueActual<actualAtaque.duracionAttack[4]*0.8f)RotarHaciaJugador();  
                                   }
                               }

                               if (duracionAtaqueActual <= 0 && startedAttacking&&!esperandoDesactivarArea)
                               {
                                   if (simpleCount == 0)
                                   {
                                       combo1.SetActive((true));
                                       auxCdAtaque = actualAtaque.duracionAttack[2];//EL TIEMPO QUE DURAN LOS ATAQUIES
                                       
                                   }
                                   else  if (simpleCount == 1)
                                   {
                                       combo2.SetActive((true));
                                       auxCdAtaque = actualAtaque.duracionAttack[3];  //EL TIEMPO QUE DURA EL ATAQUE 
                                   }  else  if (simpleCount == 2)
                                   {
                                       combo3.SetActive((true));
                                       auxCdAtaque = actualAtaque.duracionAttack[5];  //EL TIEMPO QUE DURA EL ATAQUE 
                                   }
                                 
                              
                                   esperandoDesactivarArea = true;
                               }

                               if (esperandoDesactivarArea)
                               { 
                                   auxCdAtaque -= Time.deltaTime;
                                   if (auxCdAtaque < 0&&simpleCount==2)
                                   {   combo3.SetActive((false));
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                       FinishAttack();
                                       auxCdSimples = 0;
                                       simpleCount = 0;
                                       //auxCdTrasAtaque += 1;
                                       break;
                                   }
                                   if (auxCdAtaque < 0&&simpleCount<2)
                                   { combo1.SetActive((false));
                                       combo2.SetActive((false));
                                       hijoArea = null;
                                       esperandoDesactivarArea = false;
                                       embestidaMoviendo = false;
                                     //REPETIR ATAWQUE
                                       
                                       auxCdAtaque = 0;
                                       duracionAtaqueActual = 0;
                                       startedAttacking = false;
                                       
                                      
        
                                       simpleCount += 1;
                                       auxCdSimples = cooldownEntreSimplesF2;
                                      
                                       //auxCdTrasAtaque += 1;

                                   }
                                 
                               }

                               if (auxCdSimples > 0)
                               {RotarHaciaJugador();
                                   auxCdSimples -= Time.deltaTime;
                                   if (auxCdSimples < 0)
                                   {
                                       auxCdSimples = 0;
                                   }
                               }
                               break;
                          
                          
                       }
                   }
                    else
                    {
                        auxCdTrasAtaque -= Time.deltaTime;
                        if (auxCdTrasAtaque < 0) auxCdTrasAtaque = 0;
                    }
                    break;
                case EstadosBoss.Stun:
                    DeactivateEverything();
                    FinishAttack();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }
        if (stunned)
        {
            estadoActual = EstadosBoss.Stun;
            auxTiempoStun -= Time.deltaTime;
            if (auxTiempoStun <= 0)
            {
                estadoActual = EstadosBoss.Movimiento;
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

    public int numBolasTecho = 6;
    public float yOffsetBolas=20f;
    void CrearBolasTecho()
    {
        float distanciaMaxBola = actualAtaque.distanciaMax[0];
        
        for (int i = 0; i < numBolasTecho; i++)
        {
            Vector3 randomVector = new Vector3(Random.Range(-distanciaMaxBola, distanciaMaxBola)+player.transform.position.x, yOffsetBolas,
                Random.Range(-distanciaMaxBola, distanciaMaxBola)+player.transform.position.z);
            Instantiate(actualAtaque.prefab2, randomVector, Quaternion.identity);
        }
    }
    void DeactivateEverything()
    {
        rb.velocity = Vector3.zero;
        embestidaMoviendo = false;
        startedAttacking = false;
        combo1.SetActive(false);
    combo2.SetActive((false));
    combo3.SetActive((false));
    if(piedra!=null)Destroy(piedra,1f);
    piedra = null;
    auxCdAtaque = 0;
    duracionAtaqueActual = 0;
    startedAttacking = false;
    auxCdTrasAtaque = cooldownTrasAtaque;
    actualAtaque = null;
    if(hijoArea!=null)  hijoArea.SetActive((false));
    lineRendGO.SetActive(false);
    hijoArea = null;
    esperandoDesactivarArea = false;
    auxCdLaseres = 0;
    laserCount = 0;
    auxCdSimples = 0;
    simpleCount = 0;
    
    laseresCountF2 = 0;
    }
    void RepeatLaser()
    {
        auxCdAtaque = 0;
        duracionAtaqueActual = 0;
        startedAttacking = false;
        lineRendGO.SetActive(false);
        hijoArea = null;
        esperandoDesactivarArea = false;
        embestidaMoviendo = false;
        
        laserCount += 1;
       auxCdLaseres = cooldownEntreLaseres;
         
    }
    void FinishAttack()
    {
        auxCdAtaque = 0;
        duracionAtaqueActual = 0;
        startedAttacking = false;
        auxCdTrasAtaque = cooldownTrasAtaque;
        actualAtaque = null;
        estadoActual = EstadosBoss.Movimiento;
        auxCdLaseres = 0;
        embestidasCountF2 = 0;
    }

    public void ReceiveDamage(float dmg)
    {
        if (auxTiempoTransformacion <= 0)
        {
             if (vidaActual > 0)
                   {
                       vidaActual -= dmg;
                       VisualUpdateHealth();
                   }
           
                   if (vidaActual < vidaMax * 0.5f&&!fase2)
                   {
                       fase2 = true;
                       auxTiempoTransformacion = tiempoTransformacionFase2;
                   }
                   if (vidaActual <= 0)
                   {
                       vidaActual = 0;
                       VisualUpdateHealth();
           
                       //Muerte
                   } 
        }
      
    }
   public  void ReceiveEsfuerzo(float dmg)
    { if (auxTiempoTransformacion <= 0)
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
