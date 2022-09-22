using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingBuilder : MonoBehaviour
{
    public GameObject[] buildings;
    public float latentScale = 200f;
    public float latentTranparency = 0.7f;

    // Start is called before the first frame update
    void Start()
    {
        LoadBuildings();

        if (!GameManager.S.heightColor)
        {
            GameManager.S.heightColor = true;
            SetHeightColor();
        }

        SetBuildingColors();
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

            // If latent set rotation, scale, turn off collision
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

                maxFoot /= 10;
                float scaleAdj = latentScale / maxFoot;

                // Set scale
                mesh.transform.localScale = new Vector3(scaleAdj, scaleAdj, scaleAdj);
            }
            else
            {
                // Adjust Y in map view
                // TODO: Adjust code to actually work
                float yAdj = meshBounds.center.y - meshBounds.min.y / 2;
                mesh.transform.position = new Vector3(mesh.transform.position.x, yAdj, mesh.transform.position.z);
            }

            // Set position
            //Debug.Log(indexCount);
            float thirdD = meshBounds.size.y;
            SetThirdDimension(thirdD, indexCount);
            SetBuildingPos(building, indexCount);

            // Update Index
            indexCount++;
        }
    }
    
    private void SetThirdDimension(float height, int i)
    {
        Vector3 latentCoord = GameManager.S.buildingLatentCoords[i];
        latentCoord.y = height;
        GameManager.S.buildingLatentCoords[i] = latentCoord;
    }

    private void SetHeightColor()
    {
        float yMax = GameManager.S.buildingLatentCoords.Max(v => v.y);
        float yMin = GameManager.S.buildingLatentCoords.Min(v => v.y);

        int idx = 0;
        foreach (Vector3 coord in GameManager.S.buildingLatentCoords)
        {
            float yNorm = (coord.y - yMin) / (yMax - yMin);

            Color currColor = GameManager.S.buildingColors[idx];
            currColor.g = yNorm;
            GameManager.S.buildingColors[idx] = currColor;

            idx++;
        }
    }

    private void SetBuildingColors()
    {
        // Colors based on latent vector

        int idx = 0;
        foreach (Transform child in transform)
        {
            Color buildingColor = GameManager.S.buildingColors[idx];
            if (LevelManager.S.latentSpace)
            {
                buildingColor.a = latentTranparency;
            }

            GameObject mesh = child.GetChild(0).gameObject;
            mesh.GetComponent<Renderer>().material.color = buildingColor;

            // Update index
            idx++;
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
}
