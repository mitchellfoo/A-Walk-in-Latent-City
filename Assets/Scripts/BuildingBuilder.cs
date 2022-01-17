using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBuilder : MonoBehaviour
{
    public GameObject[] buildings;
    public float latentScale = 200f;
    
    // Start is called before the first frame update
    void Start()
    {
        LoadBuildings();
    }

    private void LoadBuildings()
    {
        //string objFolder = "Montreal_obj";
        //string objFolder = "Montreal_obj_small";
        string objFolder = "Montreal_obj_smallest";

        buildings = Resources.LoadAll<GameObject>(objFolder);

        Debug.Log(buildings.Length + " buildings loaded");

        // Attach components to
        int indexCount = 0;
        foreach (GameObject b in buildings)
        {
            GameObject building = Instantiate(b, transform);
            GameObject mesh = building.transform.GetChild(0).gameObject;

            // Building component
            Building bComp = mesh.AddComponent<Building>();
            bComp.SetBuildingIndex(indexCount);

            mesh.AddComponent<Outline>().eraseRenderer = true;
            mesh.AddComponent<MeshCollider>().convex = true;

            if (LevelManager.S.latentSpace)
            {
                mesh.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            }
            else
            {
                mesh.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            mesh.AddComponent<ConstantForce>();
            mesh.gameObject.tag = "Building";
            mesh.gameObject.layer = LayerMask.NameToLayer("Building");

            // If latent set rotation and scale
            Bounds meshBounds = mesh.GetComponent<MeshFilter>().mesh.bounds;
            if (LevelManager.S.latentSpace)
            {
                // Turn of collision
                //mesh.GetComponent<MeshCollider>().enabled = false;
                
                // Make mesh spin
                bComp.ActivateLatentTorque();

                // Scale calc
                float maxFoot;
                float boundsX = meshBounds.size.x;
                float boundsZ = meshBounds.size.z;
                if (boundsX > boundsZ)
                {
                    maxFoot = boundsX;
                }
                else
                {
                    maxFoot = boundsZ;
                }

                float scaleAdj = latentScale / maxFoot;

                // Set scale
                mesh.transform.localScale = new Vector3(scaleAdj, scaleAdj, scaleAdj);
            }
            else
            {
                // Adjust Y in map view
                float yAdj = meshBounds.center.y - meshBounds.min.y/2;
                mesh.transform.position = new Vector3(mesh.transform.position.x, yAdj, mesh.transform.position.z);
            }

            // Set position
            //Debug.Log(indexCount);
            SetBuildingPos(building, indexCount);

            // Update index
            indexCount++;
        }
    }
    
    private void SetBuildingPos(GameObject built, int i)
    {
        if (LevelManager.S.latentSpace)
        {
            built.transform.position = GameManager.S.buildingLatentCoords[i];
        }
        else
        {
            built.transform.position = GameManager.S.buildingMapCoords[i];
        }
    }

    /*
    private void SetBuildingPos()
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            GameObject built = transform.GetChild(i).gameObject;

            if (LevelManager.S.latentSpace)
            {
                built.transform.position = GameManager.S.buildingLatentCoords[i];
            }
            else
            {
                built.transform.position = GameManager.S.buildingMapCoords[i];
                Debug.Log(i);
            }
        }
    }
    */
}
