using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AtaqueBoss1_", menuName = "AtaquesBoss1", order = 100)]
public class Ataques : ScriptableObject
{
    public string nameAttack;
        public float[] duracionAttack;
        public float[] distanciaMax;
        public float[] speedAttack;
       
        public GameObject pieza;
        public GameObject prefab1;
        public GameObject prefab2;
        public GameObject prefab3;
     
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
