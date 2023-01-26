using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Movement : MonoBehaviour
{
    private Rigidbody rb;
    private float yOffset;
    private float cameraOffset;
    private Camera cam;
    private GameObject cursor;
    public GameObject rotatingPart;

    public bool canShoot = true;
    public bool charging = false;
    public bool shooting = false;

    private void Start()
    {
        rotatingPart = GameObject.Find("HipUpperRig");
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
        if (canMove) MovementXY();
       
        BodySpin();
    
    }

    void Update()
    {
        if (canAttack && !dashing && !shooting) Attack();
     Dash();
     
    }

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
    }

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
    }

    public float speedToLookAtDir = 45;

    private void HandleRotation(Vector3 heading)
    {
        var rotDesired = Quaternion.LookRotation(heading, Vector3.up);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, rotDesired, speedToLookAtDir * Time.deltaTime);
    }

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