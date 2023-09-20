using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    //public GameObject prefab = transform.Find("PlayerWithHat").gameObject;
    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>(Color.red);
    //public NetworkVariable<Player> playerNetVar = new NetworkVariable<Player>(Player.prefab);
    //public Arena1Game playerPrefab;

    private Camera playerCamera;
    private GameObject playerBody;
    //private GameObject playerWithHat;
    //private GameObject playerDefault;
    //private GameObject prefab;

    private void Start()
    {
        /*if (IsHost)
        {
            playerPrefab = transform.Find("PlayerDefault").gameObject;
        } else
        {
            playerPrefab = transform.Find("PlayerWithHat").gameObject;
        }*/

        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        playerBody = transform.Find("PlayerBody").gameObject;
        ApplyColor();
    }

    private void Update()
    {
        if (IsOwner)
        {
            OwnerHandleInput();
        }
    }

    private void OwnerHandleInput()
    {
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();

        if(movement != Vector3.zero || rotation != Vector3.zero)
        {
            MoveServerRpc(movement, rotation);
        }
    }

    private void ApplyColor()
    {
        playerBody.GetComponent<MeshRenderer>().material.color = playerColorNetVar.Value;
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation)
    {
        transform.Translate(movement);
        transform.Rotate(rotation);
    }


    // Rotate around the y axis when shift is not pressed
    private Vector3 CalcRotation()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown)
        {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }


    // Move up and back, and strafe when shift is pressed
    private Vector3 CalcMovement()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = 0.0f;

            z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        transform.Translate(moveVect * movementSpeed * Time.deltaTime);
        if (!IsHost)
        {
            if (transform.position.x <= -5)
            {
                transform.position = new Vector3(-5, transform.position.y, transform.position.z);
            }
            else if (transform.position.x >= 5)
            {
                transform.position = new Vector3(5, transform.position.y, transform.position.z);
            }

            if (transform.position.z <= -5)
            {
               transform.position = new Vector3(transform.position.x, transform.position.y, -5);
            }
            else if (transform.position.z >= 5)
            {
               transform.position = new Vector3(transform.position.x, transform.position.y, 5);
            }
        }
        //moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }
}
