using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public float latentTorqueRange = 0.0005f;
    private Vector3 torque;
    public int buildingIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateLatentTorque()
    {
        torque.x = Random.Range(-latentTorqueRange, latentTorqueRange);
        torque.y = Random.Range(-latentTorqueRange, latentTorqueRange);
        torque.z = Random.Range(-latentTorqueRange, latentTorqueRange);
        GetComponent<ConstantForce>().torque = torque;
    }

    // Getter Setter Functions
    public int GetBuildingIndex()
    {
        return buildingIndex;
    }

    public void SetBuildingIndex(int i)
    {
        buildingIndex = i;
    }

}
