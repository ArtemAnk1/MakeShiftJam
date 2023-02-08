using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronFX : MonoBehaviour
{
    public float acceleration = 10.0f;
    public float acceleration2 = 10.0f;
    public float distanciaMaxima;
    public float velocity = 1;
    public float startTime;
    float t = 0;
    public List<GameObject> slots = new List<GameObject>();
    public GameObject mochila;
    public int disparosActuales;
    public GameObject pieza1;
    public GameObject pieza2;
    public float startScale;
    public float finalScale;
    bool moving;
    int numAtaque;
    public Transform player;
    GameObject pieza1Inst;
    GameObject pieza2Inst;
    bool resetVelocity;
    Vector3 savedPos1;
    Vector3 savedPos2;
    Vector3 savedOrientation;
    Vector3 directionResult;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Reload(1);
        }

        if (moving)
        {
            t += Time.deltaTime;

            
            if (t <  startTime/2)
            {
                print(mochila.transform.right);
                pieza1Inst.transform.position = Vector3.Lerp(mochila.transform.position, mochila.transform.position + (directionResult + Vector3.up+mochila.transform.forward) * distanciaMaxima/ (velocity), t / startTime);
                pieza2Inst.transform.position = Vector3.Lerp(mochila.transform.position, mochila.transform.position + (-directionResult + Vector3.up + mochila.transform.forward) * distanciaMaxima/(velocity), t / startTime);
                //pieza2Inst.transform.position = Vector3.Lerp(pieza2Inst.transform.position, (player.right + player.up + player.transform.position) * distanciaMaxima / (velocity), t / startTime);
                savedPos1 = pieza1Inst.transform.position;
                savedPos2 = pieza2Inst.transform.position;
                savedOrientation = pieza2Inst.transform.eulerAngles;
                velocity *= acceleration;
            }
            else
            {
                if (!resetVelocity) { velocity = .5f; resetVelocity = true; }
                pieza1Inst.transform.localScale = Vector3.Lerp(new Vector3(startScale, startScale, startScale), new Vector3(finalScale, finalScale, finalScale), t / startTime);
                pieza2Inst.transform.localScale = Vector3.Lerp(new Vector3(startScale, startScale, startScale), new Vector3(finalScale, finalScale, -finalScale), t / startTime);
                pieza1Inst.transform.position = Vector3.Lerp(savedPos1, slots[disparosActuales].transform.position, t * (velocity)/  startTime);
                pieza2Inst.transform.position = Vector3.Lerp(savedPos2, slots[disparosActuales].transform.position, t * (velocity) / startTime);
                pieza1Inst.transform.eulerAngles = Vector3.Lerp(savedOrientation, slots[disparosActuales].transform.eulerAngles, t * (velocity) / startTime);
                pieza2Inst.transform.eulerAngles = Vector3.Lerp(savedOrientation, slots[disparosActuales].transform.eulerAngles, t * (velocity) / startTime);
                //pieza2Inst.transform.eulerAngles = Vector3.Lerp(savedOrientation, slots[disparosActuales].transform.eulerAngles + new Vector3(0, 180, 0), t * (velocity) / startTime); 
                velocity *= acceleration2;
            }
            if (t > startTime)
            {
                pieza1Inst.transform.parent = slots[disparosActuales].transform;
                pieza2Inst.transform.parent = slots[disparosActuales].transform;
                disparosActuales++;
                if (disparosActuales == 6) { disparosActuales = 0; }
                moving = false;
                velocity = 1;
                t = 0;
                resetVelocity = false;

            }
        }
    }
    public void Reload(int i)
    {
        if (i == 1)
        {
            mochila.GetComponent<Animator>().SetTrigger("Reload");
            pieza1Inst = GameObject.Instantiate(pieza1, mochila.transform.position, Quaternion.identity);
            pieza2Inst = GameObject.Instantiate(pieza2, mochila.transform.position, Quaternion.identity);
            pieza1Inst.transform.parent = null;
            pieza2Inst.transform.parent = null;
            pieza1Inst.transform.localScale = new Vector3(startScale,startScale,startScale);
            pieza2Inst.transform.localScale = new Vector3(startScale,startScale,startScale);
            directionResult = mochila.transform.right;
            moving = true;
        }
        if (i == 2)
        {

        }
        if (i == 3)
        {

        }
    }
    private void OnDrawGizmos()
    {
        Debug.DrawLine(mochila.transform.position, mochila.transform.right * 1000, Color.red);
    }
}
