using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCamera : MonoBehaviour
{
    private bool isActive = false;
    private Camera cam;
    private Vector3 currTarget;
    private Vector3 posTarget;

    public float moveSpeed = 3.0f;
    public float positionAdjust = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {   
            transform.position = Vector3.MoveTowards(transform.position, posTarget, moveSpeed * Time.deltaTime);
            transform.LookAt(currTarget);
        }
    }

    public void MakeActive(Vector3 buildingPos)
    {
        isActive = !isActive;
        if (isActive)
        {
            currTarget = buildingPos;
            Vector3 posAdj = new Vector3(positionAdjust, 0, positionAdjust);
            posTarget = currTarget + posAdj;
        }
    }
    
}
