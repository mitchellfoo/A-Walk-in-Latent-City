using UnityEngine;
using UnityEngine.UI;

public class Raycast : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip viewSound;

    public GameObject selectBuildingPrefab;
    private GameObject lastBuilding;

    public string selectKey = "v";
    public string clearKey = "c";

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        // visualize the gaze direction
        Debug.DrawRay(transform.position, transform.forward * 100.0f, Color.cyan);

        // check for a valid view
        Transform cameraTransform = Camera.main.transform;

        // build my ray
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // Detect on view
        DetectBuildingView(ray);

        // Determine on click
        if (Input.GetMouseButtonDown(0))
        {
            // make the select noise
            //audio.PlayOneShot(gunshotSound);

            DetermineViewClick(ray);
        }

        // Add/Remove building selection
        if (Input.GetKeyDown(selectKey))
        {
            DetermineViewSelect(ray);
        }
        if (Input.GetKeyDown(clearKey))
        {
            GameManager.S.ClearSelectedBuildings();
        }
    }

    private void DetectBuildingView(Ray view)
    {
        // Turn on the outline if building is in the player's crosshair
        // TODO: Clean up repeated code
        RaycastHit hit;
        if (Physics.Raycast(view, out hit))
        {
            if (hit.transform.tag == "Building")
            {
                // View F select
                selectBuildingPrefab.SetActive(true);

                // Highlight building
                if (lastBuilding != null)
                {
                    if (!GameManager.S.BuildingIsSelected(lastBuilding))
                    {
                        lastBuilding.GetComponent<Outline>().eraseRenderer = true;
                    }
                }
                lastBuilding = hit.transform.gameObject;
                lastBuilding.GetComponent<Outline>().eraseRenderer = false;
            }
            else
            {
                selectBuildingPrefab.SetActive(false);
                if (lastBuilding != null)
                {
                    if (!GameManager.S.BuildingIsSelected(lastBuilding))
                    {
                        lastBuilding.GetComponent<Outline>().eraseRenderer = true;
                    }
                }
            }
        }
        else
        {
            selectBuildingPrefab.SetActive(false);
            if (lastBuilding != null)
            {
                if (!GameManager.S.BuildingIsSelected(lastBuilding))
                {
                    lastBuilding.GetComponent<Outline>().eraseRenderer = true;
                }
            }
        }
    }

    private void DetermineViewClick(Ray view)
    {
        RaycastHit hit;
        if (Physics.Raycast(view, out hit))
        {
            if (hit.transform.tag == "Building")
            {
                GameManager.S.SetCurrBuilding(hit.transform.gameObject.GetComponent<Building>().GetBuildingIndex());
                GameManager.S.ClearSelectedBuildings(); // Temporary solution; in future update selection will carry over
                LevelManager.S.ChangeScene();
            }
        }
    }

    private void DetermineViewSelect(Ray view)
    {
        RaycastHit hit;
        if (Physics.Raycast(view, out hit))
        {
            if (hit.transform.tag == "Building")
            {
                Debug.Log("Added to Selection");
                GameManager.S.SelectBuilding(hit.transform.gameObject);
            }
        }
    }

    // Getter Setter Functions
    public GameObject GetLastBuilding()
    {
        return lastBuilding;
    }

}