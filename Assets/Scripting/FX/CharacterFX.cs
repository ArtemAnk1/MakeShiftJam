using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFX : MonoBehaviour
{
    public List<GameObject> efectosObj = new List<GameObject>();
    public List<Transform> puntosEfectos = new List<Transform>();
    public List<GameObject> efectosFijos = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EfectoAtaqueSimpleSinParent(string valor)
    {
        string[] valoresSplit = valor.Split();

        GameObject efecto = Instantiate(efectosObj[int.Parse(valoresSplit[0])], puntosEfectos[int.Parse(valoresSplit[1])]);
        
        if(int.Parse(valoresSplit[2]) == 0)
        {
            efecto.transform.parent = null;
        }

    }


}
