using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * Attributions
 * FPS controller referenced from https://sharpcoderblog.com/blog/unity-3d-fps-controller
 */

[RequireComponent(typeof(CharacterController))]

public class FPSController : MonoBehaviour
{
    [Header("Player Controls")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public string flyKey = "f";
    public string upKey = "e";
    public string downKey = "q";
    public string viewKey = "x";

    private bool canFly = false;
    private Vector3 pos;

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera buildingCamera;

    private bool onBuildCam = false;

    [Header("GUI Objects")]
    // Info Panel
    public TextMeshProUGUI positionOverlay;
    public GameObject flyModeOverlay;
    public GameObject bInfoPanel;
    public TextMeshProUGUI bNameOverlay;
    public TextMeshProUGUI bMapOverlay;
    public TextMeshProUGUI bLatentOverlay;
    public TextMeshProUGUI bCodeOverlay;

    // Selection Panel
    public GameObject bSelectionPanel;
    public TextMeshProUGUI b1Name;
    public TextMeshProUGUI b2Name;
    public TextMeshProUGUI geoDistance;
    public TextMeshProUGUI latentDistance;

    // Building Inspector Panel
    public GameObject bInspectorPanel;

    [Header("Additional Variables")]
    public float latentPosShift = 2.0f;
    public GameObject playerCapsule;
    public GameObject teleportMarker;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Locate in space position
        if (GameManager.S.GetCurrBuildingIndex() != -1)
        {
            // Determine latent or map
            if (LevelManager.S.latentSpace)
            {
                pos = GameManager.S.buildingLatentCoords[GameManager.S.GetCurrBuildingIndex()];

                SwitchCameras();

                // Marker
                GameObject marker = Instantiate(teleportMarker);
                marker.transform.localPosition = pos + Vector3.up;
            }
            else
            {
                pos = GameManager.S.buildingMapCoords[GameManager.S.GetCurrBuildingIndex()];
            }

            // Set player position
            characterController.enabled = false;
            PlayerStartPos(pos);
            InitPlayerLook();
            characterController.enabled = true;
        }

        // Set fly in latent space
        if (LevelManager.S.latentSpace)
        {
            canFly = true;
            flyModeOverlay.SetActive(true);
        }

        // Player-building physics
        int layerBuilding = LayerMask.NameToLayer("Building");
        int layerPlayer = LayerMask.NameToLayer("Player");
        if (LevelManager.S.latentSpace)
        {
            Physics.IgnoreLayerCollision(layerBuilding, layerPlayer, true);
            Debug.Log(Physics.GetIgnoreLayerCollision(layerBuilding, layerPlayer));
            gravity = 0f;
        }
        else
        {
            Physics.IgnoreLayerCollision(layerBuilding, layerPlayer, false);
        }
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded && !canFly)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Camera switching
        if (Input.GetKeyDown(viewKey) && LevelManager.S.latentSpace)
        {
            SwitchCameras();
        }

        // Player and Camera rotation
        if (canMove && !onBuildCam)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Check Flying Mode
        if (!LevelManager.S.latentSpace && Input.GetKeyDown(flyKey))
        {
            canFly = !canFly;
            Debug.Log("Fly Mode: " + canFly);
            flyModeOverlay.SetActive(!flyModeOverlay.activeSelf);
        }

        if (canFly)
        {
            transform.TransformDirection(Vector3.up);
            transform.TransformDirection(Vector3.down);

            moveDirection.y = 0;

            if (Input.GetKey(upKey))
            {
                moveDirection.y = (isRunning ? runningSpeed : walkingSpeed);
            }

            if (Input.GetKey(downKey))
            {
                moveDirection.y = -(isRunning ? runningSpeed : walkingSpeed);
            }
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Update HUD text
        /// Rounded to 1 decimal place
        float posX = Mathf.Round(transform.position.x * 10.0f) * 0.1f;
        float posY = Mathf.Round(transform.position.z * 10.0f) * 0.1f;
        float posZ = Mathf.Round(transform.position.y * 10.0f) * 0.1f;

        positionOverlay.text = "X:" + posX + ", Y:" + posY + ", Z:" + posZ;

        /// TODO: take these out of update and only called when button pressed
        BuildingInfoPanel();
        BuildingSelectionPanel();

        // Check for quit
        /*
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("Closing Application");
            Application.Quit();
        }
        */
    }

    // Start Position
    private void PlayerStartPos(Vector3 buildingPos)
    {
        Debug.Log(buildingPos);

        /// Pseudo randomly place player a little away from building
        //float shiftX = Random.Range(0, 2) * 2 - 1 * latentPosShift;
        //float shiftZ = Random.Range(0, 2) * 2 - 1 * latentPosShift;

        buildingPos.x += latentPosShift;
        //buildingPos.y = 0;
        buildingPos.z += latentPosShift;
        transform.localPosition = buildingPos;

        Debug.Log("Player Pos" + transform.localPosition);
    }

    private void InitPlayerLook()
    {
        Vector3 buildingPos = GameManager.S.buildingLatentCoords[GameManager.S.GetCurrBuildingIndex()];
        //playerCamera.transform.LookAt(buildingPos);

        Vector3 lookPos = buildingPos - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
        //transform.LookAt(buildingPos);
    }

    // Camera controls/switching
    private void SwitchCameras()
    {
        // Camera Changing
        Debug.Log("Changing Cameras");
        canMove = !canMove;
        onBuildCam = !onBuildCam;
        playerCamera.enabled = !playerCamera.enabled;
        playerCamera.transform.GetComponentInParent<Raycast>().enabled = !playerCamera.transform.GetComponentInParent<Raycast>().enabled;
        buildingCamera.enabled = !buildingCamera.enabled;
        buildingCamera.GetComponent<BuildingCamera>().MakeActive(pos);

        // Panel Update
        bInspectorPanel.SetActive(!bInspectorPanel.activeSelf);
    }


    // GUI
    private void BuildingInfoPanel()
    {
        Raycast raySelection = playerCamera.GetComponent<Raycast>();

        if (raySelection.selectBuildingPrefab.activeSelf)
        {
            int buildingIndex = raySelection.GetLastBuilding().GetComponent<Building>().GetBuildingIndex();
            bInfoPanel.SetActive(true);
            bNameOverlay.text = raySelection.GetLastBuilding().transform.gameObject.name;
            bMapOverlay.text = "" + GameManager.S.buildingMapCoords[buildingIndex];
            bLatentOverlay.text = "" + GameManager.S.buildingLatentCoords[buildingIndex];
            bCodeOverlay.text = "" + GameManager.S.buildingLatentCodes[buildingIndex];
        }
        else
        {
            bInfoPanel.SetActive(false);
        }
    }

    private void BuildingSelectionPanel()
    {
        List<GameObject> selectedBuildings = GameManager.S.GetBuildingSelection();
        if (selectedBuildings.Count == 2)
        {
            bSelectionPanel.SetActive(true);
            // need building gameObjects
            // need distance values
            b1Name.text = selectedBuildings[0].gameObject.name;
            b2Name.text = selectedBuildings[1].gameObject.name;

            float[] distances = GameManager.S.GetSelectedDistance();
            float geoVal = Mathf.Round(distances[0] * 10.0f) * 0.1f;
            float latentVal = Mathf.Round(distances[1] * 10.0f) * 0.1f;
            geoDistance.text = "" + geoVal;
            latentDistance.text = "" + latentVal;
        }
        else
        {
            bSelectionPanel.SetActive(false);
        }
    }
}