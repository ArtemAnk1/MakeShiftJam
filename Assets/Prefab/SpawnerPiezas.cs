using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerPiezas : MonoBehaviour
{
    public GameObject piezaA;

    public GameObject piezaB;

    public float tiempoEntrePiezas = 3f;
    public float auxTiempo;

    public float distanciaSpawneo = 5f;

    public float posibilidadPiezaA = 70;
    // Start is called before the first frame update
    void Start()
    {
        auxTiempo = tiempoEntrePiezas;
    }

    // Update is called once per frame
    void Update()
    {
        if (auxTiempo > 0) auxTiempo -= Time.deltaTime;
        if (auxTiempo < 0)
        {
            auxTiempo = tiempoEntrePiezas;
            if (Random.Range(0, 100) > posibilidadPiezaA)
            {
                Instantiate(piezaB,
                    new Vector3(this.transform.position.x + Random.Range(-distanciaSpawneo, distanciaSpawneo),
                        this.transform.position.y,
                        this.transform.position.z + Random.Range(-distanciaSpawneo, distanciaSpawneo)),
                    Quaternion.identity);
            }
            else
            {
                Instantiate(piezaA,
                    new Vector3(this.transform.position.x + Random.Range(-distanciaSpawneo, distanciaSpawneo),
                        this.transform.position.y,
                        this.transform.position.z + Random.Range(-distanciaSpawneo, distanciaSpawneo)),
                    Quaternion.identity);
            }
        }
    }
}
