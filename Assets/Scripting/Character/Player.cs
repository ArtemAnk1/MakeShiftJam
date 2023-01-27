using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    [Header("STARTVARS")] [Space(10)]
    private Rigidbody rb;
    private float yOffset;
    private float cameraOffset;
    private Camera cam;
    private GameObject cursor;
    public GameObject rotatingPart;
 
    private void Start()
    {
        rotatingPart = GameObject.Find("HipUpper");
        cursor = FindObjectOfType<CursorPointer>().gameObject;
        currentSpeedMult = baseSpeedMult;
        GetWorldsForward();
        rb = GetComponent<Rigidbody>();
        yOffset = transform.position.y;
        cam = Camera.main;
        hipStartingY = rotatingPart.transform.position.y;
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
       
       
      if(!attacking)  BodySpin();
    
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
    [SerializeField] private float duracionDeAtaqueActual = 0;
    public GameObject[] ataquesObjetos;
    public float[] tiempoParaCombar;
    public float[] duracionesDeAtaques;
    public float cooldownAfterCombo = 2f;
    [SerializeField] private float timerAuxCD;
    [SerializeField] private bool finishedCombo = false;
    [SerializeField] private float tiempoActualParaCombar = 0;
    public bool waitingForCombo = false;
    public bool cachedAttack = false;
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
            {
                duracionDeDisparoActual -= Time.deltaTime;
                if (duracionDeDisparoActual < 0)
                {
                    duracionDeDisparoActual = 0;
                    disparosObjetos[SeleccionDisparo()].SetActive((false));
                    shooting = false;
                    piezasListas.Remove(piezasListas[0]);
                    canMove = true;
                    timerAuxDisp = cooldownTrasDisparo;
                    fullPiezas = false;
                }
            }
        }
           
    }

    int SeleccionDisparo()
    {
        int value = 0;
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
       
        return value;
    }
    [Space(10)] [Header("PIEZAS")] [Space(10)]
    public List<Pieza> piezasRecogidas = new List<Pieza>();

    public List<Pieza> piezasListas = new List<Pieza>();
    public bool fullPiezas = false;
    public int maxCapacidadPiezas=3;
    public void CogerPieza(Pieza pieza)
    {
        if (piezasRecogidas.Count == 0)
        {
            if (piezasListas.Count <3)
            {
                piezasRecogidas.Add((pieza));
            }
        }
        else
        {
            if (piezasListas.Count <= maxCapacidadPiezas)
            {
                piezasRecogidas.Add((pieza));
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

    void CalculaPiezaFinal()
    {
        if(piezasRecogidas[0].piezaA&&piezasRecogidas[1].piezaA)
        {
            Pieza p = new Pieza();
            p.fusionAa = true;
            piezasListas.Add(p);
        }
        if(!piezasRecogidas[0].piezaA&&!piezasRecogidas[1].piezaA)
        {  Pieza p = new Pieza();
            p.fusionBb = true;
            piezasListas.Add(p);
          
        }
        if(piezasRecogidas[0].piezaA!=piezasRecogidas[1].piezaA)
        {  Pieza p = new Pieza();
            p.fusionAb = true;
            piezasListas.Add(p);
        
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

    private void Attack()
    {
        if (duracionDeAtaqueActual == 0 && comboCount < maxComboCount && timerAuxCD == 0 &&
            Input.GetKeyDown(KeyCode.Mouse0) && waitingForCombo == false)
        {
            ataquesObjetos[comboCount].gameObject.SetActive(true);
            duracionDeAtaqueActual = duracionesDeAtaques[comboCount];

            attacking = true;
        }
        else if (attacking)
        {
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
    [Space(10)]
    [Header("ROTATION")] [Space(10)]
    public GameObject bodySpinning;
    public float bodyRotationSpeed = 300;
    public float hipStartingY;

    private void BodySpin()
    {
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
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, rotDesired, speedToLookAtDir * Time.deltaTime);
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
            HandleRotation(desiredHeading);
        }
        else
        {
            desiredHeading = Vector3.MoveTowards(transform.forward, lastHeading, inertiaValue * Time.deltaTime);
            HandleRotation(desiredHeading);
        }

        rb.velocity = desiredHeading * (moveSpeedBase * currentSpeedMult * Time.deltaTime);
    }
}