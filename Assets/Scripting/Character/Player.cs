using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    [Header("STARTVARS")] [Space(10)]
    private Rigidbody rb;
    private float yOffset;
    private float cameraOffset;
    private Camera cam;
    private GameObject cursor;
    public GameObject rotatingPart;
    public GameObject bodyRotatingPart;
    public GameObject boss;
    public BossPrimero scriptBoss;
    private void Start()
    {
        rotatingPart = GameObject.Find("Rig 1");
        bodyRotatingPart = GameObject.Find("Rig2");
        cursor = FindObjectOfType<CursorPointer>().gameObject;
        currentSpeedMult = baseSpeedMult;
        GetWorldsForward();
        rb = GetComponent<Rigidbody>();
        yOffset = transform.position.y;
        cam = Camera.main;
        hipStartingY = rotatingPart.transform.position.y;
        boss = FindObjectOfType<BossPrimero>().gameObject;
        scriptBoss = boss.GetComponent<BossPrimero>();
    }


    private Vector3 forwardWorld, rightWorld;

    private void GetWorldsForward()
    {
        forwardWorld = Camera.main.transform.forward;
        forwardWorld.y = 0;
        forwardWorld = Vector3.Normalize(forwardWorld);
        rightWorld = Quaternion.Euler(new Vector3(0, 90, 0)) * forwardWorld;
    }

    private void FixedUpdate()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (canMove)
        {
            MovementXY();
        }
       
       
    if(!attacking&&!shooting) BodySpin();
    
    }
   
    void Update()
    {
        if (canShoot)
        {
            Shoot();
        }
        if (canAttack && !dashing && !shooting) Attack();
     Dash();
    
    }
    [Space(10)]
    [Header("ATTACKS")] [Space(10)]
    public bool canAttack = true;
    public bool attacking = false;
    public int comboCount = 0;
    public int maxComboCount = 3;
    public float duracionDeAtaqueActual = 0;
    public GameObject[] ataquesObjetos;
    public float[] tiempoParaCombar;
    public float[] duracionesDeAtaques;
    public float[] anguloAperturaAtaques;
    public float anguloActual;
    public float[] distanciaMaxAtaques;
    public float actualDistanciaMax;
    public float cooldownAfterCombo = 2f;
    [SerializeField] private float timerAuxCD;
    [SerializeField] private bool finishedCombo = false;
    [SerializeField] private float tiempoActualParaCombar = 0;
    public bool waitingForCombo = false;
    public bool cachedAttack = false;
    
    private void Attack()
    {
        if (duracionDeAtaqueActual == 0 && comboCount < maxComboCount && timerAuxCD == 0 &&
            Input.GetKeyDown(KeyCode.Mouse0) && waitingForCombo == false)
        {
            ataquesObjetos[comboCount].gameObject.SetActive(true);
            duracionDeAtaqueActual = duracionesDeAtaques[comboCount];
            actualDistanciaMax = distanciaMaxAtaques[comboCount];
            anguloActual = anguloAperturaAtaques[comboCount];
            attacking = true;
        }
        else if (attacking)
        {
            CheckBossCollision(anguloActual, actualDistanciaMax, out bool espalda, out bool hit);
            if (hit)
            {
                if(scriptBoss.actualTiempoSinDañoMelee<=0)
                {
           
                    int ataqueSelecc = comboCount;
                    float multip = 1;
                    if (espalda) multip = scriptBoss.multipEspalda;
                    
                    if(scriptBoss.stunned)   scriptBoss.ReceiveEsfuerzo(dañosNormales[ataqueSelecc]*scriptBoss.multipDañoMeleSiStun);
                    scriptBoss.ReceiveEsfuerzo(dañosNormales[ataqueSelecc]*multip);
                    scriptBoss.actualTiempoSinDañoMelee = duracionDeAtaqueActual+0.1f;
                }
            }
            duracionDeAtaqueActual -= Time.deltaTime;
            if (duracionDeAtaqueActual <= 0)
            {
                duracionDeAtaqueActual = 0;
                attacking = false;
                ataquesObjetos[comboCount].gameObject.SetActive(false);
                waitingForCombo = true;
                if (comboCount == maxComboCount - 1)
                {

                    timerAuxCD = cooldownAfterCombo;
                    comboCount = 0;
                    finishedCombo = true;
                    waitingForCombo = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                cachedAttack = true;
            }
          
        }

        if (!attacking && tiempoActualParaCombar == 0 && waitingForCombo)
        {
            tiempoActualParaCombar = tiempoParaCombar[comboCount];
        }
        else if (!attacking && waitingForCombo && tiempoActualParaCombar > 0)
        {
            tiempoActualParaCombar -= Time.deltaTime;
            if (tiempoActualParaCombar <= 0)
            {
                comboCount = 0;
                waitingForCombo = false;
                tiempoActualParaCombar = 0;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0)||cachedAttack)
            {
                if (cachedAttack) cachedAttack = false;
              
              
                 comboCount += 1;
                    attacking = true;
                    ataquesObjetos[comboCount].gameObject.SetActive(true);
                    duracionDeAtaqueActual = duracionesDeAtaques[comboCount];
                    finishedCombo = false;
                    tiempoActualParaCombar = 0;
                    actualDistanciaMax = distanciaMaxAtaques[comboCount];
                    anguloActual = anguloAperturaAtaques[comboCount];
            }

          
        }
        if (timerAuxCD > 0)
        {
            timerAuxCD -= Time.deltaTime;
            if (timerAuxCD <= 0)
            {
                timerAuxCD = 0;
                if (finishedCombo) comboCount = 0;
                finishedCombo = false;
            }
        }

        if (attacking )
        {
            canShoot = false;
            
        }
        else if(!dashing)
        {
            canShoot = true;
        }
    }

    void CheckBossCollision(float angulo, float distancia,out bool espalda,out bool hit)
    {           
                 espalda = false;
                 hit = false;
                 Vector3 hitpoint=boss.transform.position;
                 hitpoint.y = this.transform.position.y;
                 var desPos = new Vector3(cursor.transform.position.x,  this.transform.position.y,
                     cursor.transform.position.z);
                 Vector3 forwarY0 =(desPos-this.transform.position);
                 forwarY0.y = this.transform.position.y;
                 if (Physics.Raycast(this.transform.position, forwarY0, out RaycastHit rayhit, distancia))
                 {

                     hitpoint = rayhit.point;
                     hitpoint.y = this.transform.position.y;
                  
                 }
                 
                Vector3 direccion = hitpoint - this.transform.position;
            
                direccion.y = 0;
                
                if (Vector3.Angle(forwarY0, direccion) <= angulo)
                 {
                     
                      
                 if (Physics.Raycast(transform.position, direccion, out RaycastHit t, distancia))
                 {  
                Debug.DrawRay(transform.position,new Vector3( direccion.normalized.x,0, direccion.normalized.z)*distancia,Color.red,2f);
                     hit = true;
                     if (t.collider.GetComponent<CollidersHijos>()?.esDebil ??false) espalda = true;
                 }
                 
                 }
        
        

    }

    [Space(10)]
    [Header("SHOOTING")] [Space(10)]
    public bool canShoot = true;
    public bool charging = false;
    public bool shooting = false;
    [SerializeField] private float duracionDeDisparoActual = 0;
    public GameObject[] disparosObjetos;
    public float[] tiemposDeCarga;
    public float tiempoActualDeCarga = 0;
    private float auxTiempoCarga=0;
    public float[] duracionesDeDisparos;
    public float[] distanciaMaxCargado;
    public float[] anguloAperturaCargados;
    public float cooldownTrasDisparo = 2f;
    [SerializeField] private float timerAuxDisp=0;
  
    
    void Shoot()
    {
        if (timerAuxDisp > 0)
        {
            timerAuxDisp -= Time.deltaTime;
            if (timerAuxDisp < 0)
            {
                timerAuxDisp = 0;
            }
        }
        if (piezasListas.Count >= 1&&timerAuxDisp==0)
        {
           
            if (Input.GetKeyDown(KeyCode.Mouse1)&&!shooting)
            { 
                charging = true;
                auxTiempoCarga = 0;

                tiempoActualDeCarga = tiemposDeCarga[SeleccionDisparo()];
                rb.velocity = Vector3.zero;

            }
        

            if (charging)
            {
                canMove = false;
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    auxTiempoCarga += Time.deltaTime;
               
                }
                else if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    if (auxTiempoCarga > tiempoActualDeCarga)
                    {
                        tiempoActualDeCarga = 0;
                        charging = false;
                        shooting = true;
                        auxTiempoCarga = 0;
                        duracionDeDisparoActual = duracionesDeDisparos[SeleccionDisparo()];
                        
                        disparosObjetos[SeleccionDisparo()].SetActive((true));
                        anguloActual = anguloAperturaCargados[SeleccionDisparo()];
                        actualDistanciaMax = distanciaMaxCargado[SeleccionDisparo()];
                    }
                    else
                    {  charging = false;
                        canMove = true;
                        auxTiempoCarga = 0;
                        tiempoActualDeCarga = 0;
                    }
                }
                else
                {
                    charging = false;
                    canMove = true;
                    auxTiempoCarga = 0;
                    tiempoActualDeCarga = 0; 
                }
            }

            if (shooting)
            {  CheckBossCollision(anguloActual, actualDistanciaMax, out bool espalda, out bool hit);
                if (hit)
                {
                   
           
                        int ataqueSelecc = SeleccionDisparo();
                        float multip = 1;
                        if (espalda) multip = scriptBoss.multipEspaldaDistancia;
                    
                        if(scriptBoss.stunned)   scriptBoss.ReceiveDamage(dañosCargados[ataqueSelecc]*Time.deltaTime);
                        scriptBoss.ReceiveDamage(dañosCargados[ataqueSelecc]*multip*Time.deltaTime);
                        
                    
                }
                duracionDeDisparoActual -= Time.deltaTime;
                if (duracionDeDisparoActual < 0)
                {
                    duracionDeDisparoActual = 0;
                    disparosObjetos[SeleccionDisparo()].SetActive((false));
                    shooting = false;
                    piezasListas.Remove(piezasListas[0]);
                    UpdatePiezas();
                    canMove = true;
                    timerAuxDisp = cooldownTrasDisparo;
                    fullPiezas = false;
                }
            }
        }
           
    }

   public int SeleccionDisparo()
    { int value = 0;
        if (piezasListas.Count>=0)
        {
             if (piezasListas[0].fusionAa)
                    {
                        value = 0;
                    } else if (piezasListas[0].fusionAb)
                    {
                        value = 1;
                    }
                   else 
                    if (piezasListas[0].fusionBb)
                    {
                        value = 2;
                    }
        }
       
       
       
        return value;
    }

    [Space(10)] [Header("DAÑOS")] [Space(10)]
 
    public float[] dañosNormales;
    public float[] dañosCargados;
   
    
    
    
    [Space(10)] [Header("PIEZAS")] [Space(10)]
    public List<Pieza> piezasRecogidas = new List<Pieza>();

    public List<Pieza> piezasListas = new List<Pieza>();
    public bool fullPiezas = false;
    public int maxCapacidadPiezas=3;
    public GameObject[] slots;
    public Sprite[] spritePiezas;
    public Sprite[] spritesFusiones;
    public Sprite defaultSlotSprite;
    public void CogerPieza(Pieza pieza)
    {
        if (piezasRecogidas.Count == 0)
        {
            
            if (piezasListas.Count <3)
            {
          
                
            
               
                
                
                
                piezasRecogidas.Add((pieza));
                AddVisualSlot(pieza);
            }
        }
        else
        {
            if (piezasListas.Count <= maxCapacidadPiezas)
            {
                piezasRecogidas.Add((pieza));
                AddVisualSlot(pieza);
                CalculaPiezaFinal();
                piezasRecogidas.Clear();
                fullPiezas = false;
                
                if (piezasListas.Count == maxCapacidadPiezas)
                {
                    fullPiezas = true;
                }
            }
            else
            {
                
            }
        }
        
      
    }

    void UpdatePiezas()
    {
        slots[0].transform.GetChild(0).GetComponent<Image>().sprite =
            slots[1].transform.GetChild(0).GetComponent<Image>().sprite;
        slots[0].transform.GetChild(1).GetComponent<Image>().sprite =
            slots[1].transform.GetChild(1).GetComponent<Image>().sprite;
        slots[1].transform.GetChild(0).GetComponent<Image>().sprite =
            slots[2].transform.GetChild(0).GetComponent<Image>().sprite; 
        slots[1].transform.GetChild(1).GetComponent<Image>().sprite =
            slots[2].transform.GetChild(1).GetComponent<Image>().sprite;
        slots[2].transform.GetChild(0).GetComponent<Image>().sprite = defaultSlotSprite;
            slots[2].transform.GetChild(1).GetComponent<Image>().sprite = defaultSlotSprite;

    }
    void AddVisualSlot(Pieza p)
    {
        
        if (piezasListas.Count == 0)
        {
            if (piezasRecogidas.Count == 1)
            {
                if (p.piezaA)
                {
                    slots[0].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[0].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[1];
                }
              
            }
            else if (piezasRecogidas.Count == 2)
            {
                if (p.piezaA)
                {
                    slots[0].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[0].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[1];
                }
            }
        }else if (piezasListas.Count == 1)
        {
            if (piezasRecogidas.Count == 1)
            {
                if (p.piezaA)
                {
                    slots[1].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[1].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[1];
                }
            }
            else if (piezasRecogidas.Count == 2)
            {
                if (p.piezaA)
                {
                    slots[1].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[1].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[1];
                }
            }
        }else if (piezasListas.Count == 2)
        {
            if (piezasRecogidas.Count == 1)
            {
                if (p.piezaA)
                {
                    slots[2].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[2].transform.GetChild(0).GetComponent<Image>().sprite = spritePiezas[1];
                }
            }
            else if (piezasRecogidas.Count == 2)
            {
                if (p.piezaA)
                {
                    slots[2].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[0];
                }
                else
                {
                    slots[2].transform.GetChild(1).GetComponent<Image>().sprite = spritePiezas[1];
                }
            }
        }else if (piezasListas.Count == 3)
        {
         
        }

       
    }
    void CalculaPiezaFinal()
    {
        if(piezasRecogidas[0].piezaA&&piezasRecogidas[1].piezaA)
        {
            
            GameObject piezaObject = new GameObject();
            piezaObject.AddComponent<Pieza>();
           Pieza p= piezaObject.GetComponent<Pieza>();
            p.fusionAa = true;
            piezasListas.Add(p);
            p.gameObject.SetActive(false);

        }
        if(!piezasRecogidas[0].piezaA&&!piezasRecogidas[1].piezaA)
        {    GameObject piezaObject = new GameObject();
            piezaObject.AddComponent<Pieza>();
            Pieza p= piezaObject.GetComponent<Pieza>();
            p.fusionBb = true;
            piezasListas.Add(p);
            p.gameObject.SetActive(false);
        }
        if(piezasRecogidas[0].piezaA!=piezasRecogidas[1].piezaA)
        {     GameObject piezaObject = new GameObject();
            piezaObject.AddComponent<Pieza>();
            Pieza p= piezaObject.GetComponent<Pieza>();
            p.fusionAb = true;
            piezasListas.Add(p);
            p.gameObject.SetActive(false);
        
        }

        if (piezasListas.Count >= maxCapacidadPiezas)
        {
            fullPiezas = true;
            foreach (var VARIABLE in piezasListas)
            {
                  print(VARIABLE.fusionAb+"ISAB"+VARIABLE.fusionAa+"ESAA"+VARIABLE.fusionBb+"ESBB");
            }
          
        }
        else
        {
            fullPiezas = false;
        }
    }
    [Space(10)]
    [Header("ROTATION")] [Space(10)]
    public GameObject bodySpinning;
    public float bodyRotationSpeed = 300;
    public float hipStartingY;
    public Quaternion lastAttackRotation= new Quaternion(0,0,0,0);
    
    private void BodySpin()
    {
       
            lastAttackRotation = new Quaternion(0, 0, 0, 0);
            var desPos = new Vector3(cursor.transform.position.x, hipStartingY,
                cursor.transform.position.z);
            var rotDesired = Quaternion.LookRotation(desPos - rotatingPart.transform.position, Vector3.up);
            rotatingPart.transform.rotation = Quaternion.RotateTowards(rotatingPart.transform.rotation, rotDesired,
                bodyRotationSpeed * Time.deltaTime); 
      
       
    }
    [Space(10)]
    [Header("Dash")] [Space(10)]
    public bool canDash = true;
    public bool dashing = false;
    public float maxDashSpeed = 10f;

    public float dashTime = 1f;

    private bool dashOnCd = false;
    public float dashCdTime = 3f;
    private float auxDashCD = 0;

    private float t = 0;
    public AnimationCurve dashCurve;

    public Vector3 cachedSpeed { get; set; }

    private void Dash()
    {
        if (dashing)
        {
            if (t < dashTime)
            {
                if (t >= dashTime * 0.9f) canMove = true;
                t += Time.deltaTime;
                rb.velocity = Vector3.Lerp(cachedSpeed, maxDashSpeed * dashCurve.Evaluate(t) * lastHeading,
                    t / dashTime);
            }
            else
            {
                currentSpeedMult = 0;
                t = 0;
                dashing = false;
                dashOnCd = true;
                canMove = true;
            }
        }

        if (dashOnCd)
        {
            auxDashCD += Time.deltaTime;
            if (auxDashCD >= dashCdTime)
            {
                auxDashCD = 0;
                dashOnCd = false;
            }
        }

        if (canDash && !dashing && !dashOnCd)
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                canMove = false;
                dashing = true;

                t = 0;
                cachedSpeed = rb.velocity;
            }
        if (dashing )
        {
            canShoot = false;
            
        }
        else if(!shooting)
        {
            canShoot = true;
        }
    }
    
    [Header("ROTATIONSPEED")]
    [Space(10)]
    public float speedToLookAtDir = 45;

    private void HandleRotation(Vector3 heading)
    {
        var rotDesired = Quaternion.LookRotation(heading, Vector3.up);
        bodyRotatingPart.transform.rotation =
            Quaternion.RotateTowards(  bodyRotatingPart.transform.rotation, rotDesired, speedToLookAtDir * Time.deltaTime);
    }
    [Space(10)]
    [Header("MOVEMENT")]
    [Space(10)]
    public float moveSpeedBase = 4f;
    public float accelValue = 1.3f;
    public float deccelValue = 1.3f;
    public float baseSpeedMult = 2;
    public float currentSpeedMult;
    public float maxSpeedMult;
    public float inertiaValue = 1f;
    public Vector3 desiredHeading;
    public Vector3 lastHeading;
    public bool canMove = true;
    private Vector3 movX;
    private Vector3 movY;

    private void MovementXY()
    {
        movX = rightWorld * (moveSpeedBase * Time.deltaTime * Input.GetAxisRaw("Horizontal"));
        movY = forwardWorld * (moveSpeedBase * Time.deltaTime * Input.GetAxisRaw("Vertical"));

        var heading = Vector3.Normalize(movX + movY);

        if (heading != Vector3.zero)
        {
            if (currentSpeedMult < baseSpeedMult) currentSpeedMult = baseSpeedMult;
            if (currentSpeedMult < maxSpeedMult) currentSpeedMult += Time.deltaTime * accelValue;
            if (currentSpeedMult > maxSpeedMult) currentSpeedMult = maxSpeedMult;
        }
        else
        {
            if (currentSpeedMult > 0) currentSpeedMult -= Time.deltaTime * deccelValue;
            if (currentSpeedMult < 0) currentSpeedMult = 0;
        }

        desiredHeading = heading;
        if (heading != Vector3.zero)
        {
            desiredHeading = Vector3.MoveTowards(transform.forward, heading, inertiaValue * Time.deltaTime);
            lastHeading = desiredHeading;
          //  if (!attacking&&!shooting)
             HandleRotation(desiredHeading);
        }
        else
        {
            desiredHeading = Vector3.MoveTowards(transform.forward, lastHeading, inertiaValue * Time.deltaTime);
           // if (!attacking&&!shooting)
             HandleRotation(desiredHeading);
        }

        rb.velocity = desiredHeading * (moveSpeedBase * currentSpeedMult * Time.deltaTime);
      
    }
}