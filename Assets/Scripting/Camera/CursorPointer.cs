
using UnityEngine;

public class CursorPointer : MonoBehaviour
{
    [SerializeField] public LayerMask layerMask;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo,float.MaxValue,layerMask))
        {
            transform.position = hitInfo.point;
         
        }
    }
}
