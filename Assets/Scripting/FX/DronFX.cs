using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronFX : MonoBehaviour
{
    public float acceleration = 10.0f;
    public float distanciaMaxima;
    public float velocity = 1;
    public float startTime;
    float t = 0;
    public List<GameObject> slots = new List<GameObject>();
    public GameObject mochila;
    public int disparosActuales;
    public GameObject pieza1;
    public GameObject pieza2;
    bool moving;int numAtaque;
    public Transform player;
    GameObject pieza1Inst;
    GameObject pieza2Inst;

    Vector3 savedPos1;
    Vector3 savedPos2;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Reload(1);
        }

        if (moving)
        {
            t += Time.deltaTime;
            if (t < 0.5f)
            {
                print(mochila.transform.right);
                pieza1Inst.transform.position = Vector3.Lerp(mochila.transform.position, (mochila.transform.right + Vector3.up) * distanciaMaxima / (velocity), t / startTime);
                pieza1Inst.transform.position = Vector3.Lerp(mochila.transform.position, (-mochila.transform.right + Vector3.up) * distanciaMaxima / (velocity), t / startTime);
                //pieza2Inst.transform.position = Vector3.Lerp(pieza2Inst.transform.position, (player.right + player.up + player.transform.position) * distanciaMaxima / (velocity), t / startTime);
                savedPos1 = pieza1Inst.transform.position;
                savedPos2 = pieza2Inst.transform.position;
            }
            else
            {
                pieza1Inst.transform.position = Vector3.Lerp(savedPos1, slots[disparosActuales].transform.position, t / startTime);
                pieza2Inst.transform.position = Vector3.Lerp(savedPos2, slots[disparosActuales].transform.position, t / startTime);
            }
            if (t > startTime)
            {
                pieza1Inst.transform.parent = slots[disparosActuales].transform;
                pieza2Inst.transform.parent = slots[disparosActuales].transform;
                disparosActuales++;
                if (disparosActuales == 5) { disparosActuales = 0; }
                moving = false;
                velocity = 1;
                t = 0;
            }
        }
    }
    public void Reload(int i)
    {
        velocity *= acceleration;
        if (i == 1)
        {
            mochila.GetComponent<Animator>().SetTrigger("Reload");
            pieza1Inst = GameObject.Instantiate(pieza1, mochila.transform.position, Quaternion.identity);
            pieza2Inst = GameObject.Instantiate(pieza1, mochila.transform.position, Quaternion.identity);
            pieza1.transform.parent = null;
            pieza2.transform.parent = null;

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
