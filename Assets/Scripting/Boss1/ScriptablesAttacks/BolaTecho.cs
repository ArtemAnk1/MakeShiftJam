using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

public class BolaTecho : MonoBehaviour
{
    private Rigidbody rb;

    public GameObject prefabSombra;

    private GameObject sombra;

    public Vector3 initialScaleSombra;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 sombraPos=new Vector3(this.transform.position.x, 0.28f,
            this.transform.position.z);
        sombra=Instantiate(prefabSombra, sombraPos, UnityEngine.Quaternion.identity);
        initialScaleSombra = sombra.transform.localScale;
        sombra.transform.localScale *= 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        float t =  rb.velocity.magnitude*0.75f/Vector3.Distance(sombra.transform.position, this.transform.position) ;
        
        sombra.transform.localScale=    Vector3.Lerp(sombra.transform.localScale, initialScaleSombra, t*0.75f);
        if(rb.velocity.y<2&&Vector3.Distance(this.transform.position,new Vector3(this.transform.position.x,0.28f,this.transform.position.z))<3f)
        {
            Destroy((sombra.gameObject));
            Destroy(this.gameObject);
        }
    }
}
