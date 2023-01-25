using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Movement : MonoBehaviour
{ private Rigidbody rb;
    void Start()
    {
        currentSpeedMult = baseSpeedMult;
        GetWorldsForward();
        rb = GetComponent<Rigidbody>();
    }

    
     Vector3 forwardWorld, rightWorld;
    private void GetWorldsForward()
    {
        forwardWorld = Camera.main.transform.forward;
        forwardWorld.y = 0;
        forwardWorld = Vector3.Normalize(forwardWorld);
        rightWorld = Quaternion.Euler(new Vector3(0, 90, 0)) * forwardWorld;
    }
    void FixedUpdate ()
    {
      if(rb==null) rb = GetComponent<Rigidbody>();
      if(canMove) MovementXY();
        Dash();



    }

    public bool canDash = true;
    public bool dashing = false;
    public float maxDashSpeed = 10f; 
      public float baseDashSpeed=3;
      public float dashTime = 1f;
    public float dashAux=0;
    private bool dashOnCd = false;
    public float dashCdTime = 3f;
    private float auxDashCD = 0;
    public float currentDashSpeed = 0;
  
    private void Dash()
    {
        
        if (dashing)
        { DashCoroutine();
          
        //     if (dashAux < dashTime * 0.1f)
        //     {
        //         currentDashSpeed += 60000*Time.deltaTime;
        //         if (currentDashSpeed > maxDashSpeed)
        //         {
        //             currentDashSpeed = maxDashSpeed;
        //         }
        //     }
        //     if (dashAux > dashTime * 0.35f)
        //     {
        //         currentDashSpeed -= 25000*Time.deltaTime;
        //         if (currentDashSpeed < baseDashSpeed)
        //         {
        //             currentDashSpeed = baseDashSpeed;
        //         }
        //     }
        //     dashAux += Time.deltaTime;
        //     if (dashAux >= dashTime * 0.9f)canMove = true;
        //     
        //     if (dashAux >= dashTime)
        //     {
        //         dashing = false;
        //         dashOnCd = true;
        //         canMove = true;
        //         dashAux = 0; 
        //     }
        //     rb.velocity= Vector3.Lerp(rb.velocity,lastHeading * (maxDashSpeed * Time.deltaTime),)
        //     rb.velocity= lastHeading * (currentDashSpeed * Time.deltaTime);
        //     HandleRotation(lastHeading,speedToLookAtDir);
        //
     
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

        if (canDash && !dashing && !dashOnCd) if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                canMove = false;
                dashing = true;
                currentDashSpeed = baseDashSpeed;
                t = 0;
               cachedSpeed = rb.velocity; 
            }
        
        
        
        
        
   
        
        
        
        
        
    }

    public Vector3 cachedSpeed { get; set; }

    private float t = 0;
    public AnimationCurve dashCurve;
   void DashCoroutine()
    {
       
        
       
       
        if (t < dashTime)
        { 
            if (t >= dashTime * 0.9f)canMove = true;
            t += Time.deltaTime;
            rb.velocity = Vector3.Lerp(cachedSpeed, maxDashSpeed*dashCurve.Evaluate(t) *lastHeading, t / dashTime);
           
        }
        else
        {
              currentSpeedMult = 0;
              t = 0;
                       dashing = false;
                       dashOnCd = true;
                       canMove = true;
                       dashAux = 0; 
        }

     
           
        
    }
    public float speedToLookAtDir = 45;
    private void HandleRotation(Vector3 heading)
    {
        Quaternion rotDesired = Quaternion.LookRotation(heading, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotDesired, speedToLookAtDir * Time.deltaTime);
     
    }
    private void HandleRotation(Vector3 heading,float dashMult)
    {
        Quaternion rotDesired = Quaternion.LookRotation(heading, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotDesired, speedToLookAtDir+dashMult * Time.deltaTime);
     
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
          
        Vector3 heading = Vector3.Normalize(movX+movY);
      
        if (heading != Vector3.zero)
        {   
            if (currentSpeedMult <baseSpeedMult) currentSpeedMult = baseSpeedMult;
            if(currentSpeedMult<maxSpeedMult) currentSpeedMult += Time.deltaTime * accelValue;
            if (currentSpeedMult > maxSpeedMult) currentSpeedMult = maxSpeedMult;
        }
        else
        {
            if(currentSpeedMult>0) currentSpeedMult -= Time.deltaTime * deccelValue;
            if (currentSpeedMult <0) currentSpeedMult = 0;
          
          
        }

        desiredHeading = heading;
        if (heading != Vector3.zero)
        {
             desiredHeading=Vector3.MoveTowards(this.transform.forward, heading, inertiaValue * Time.deltaTime);
             lastHeading = desiredHeading;
             HandleRotation(desiredHeading);
        }
        else
        {
            desiredHeading=Vector3.MoveTowards(this.transform.forward, lastHeading, inertiaValue * Time.deltaTime);
            HandleRotation(desiredHeading);
        }
        rb.velocity= desiredHeading * (moveSpeedBase*currentSpeedMult * Time.deltaTime);
     
        
    }

    
}
