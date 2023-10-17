using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>(Color.red);
    public BulletSpawner bulletSpawner;

    private Camera playerCamera;
    private GameObject playerBody;

    private void NetworkInit()
    {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        playerBody = transform.Find("PlayerBody").gameObject;
        ApplyColor();
        playerColorNetVar.OnValueChanged += OnPlayerColorChanged;
    }

    private void Awake()
    {
        NetworkHelper.Log(this, "Awake");
    }

    private void Start()
    {

        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn()
    {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (IsOwner)
        {
            OwnerHandleInput();
            /*if (Input.GetButtonDown("Fire1"))
            {
                NetworkHelper.Log("Requesting Fire");
                FireServerRpc();
            }*/
        } 
        if (!IsHost)
        {
            if (transform.position.x <= -7)
            {
                transform.position = new Vector3(-7, transform.position.y, transform.position.z);
            }
            else if (transform.position.x >= 7)
            {
                transform.position = new Vector3(7, transform.position.y, transform.position.z);
            }

            if (transform.position.z <= -7)
            {
               transform.position = new Vector3(transform.position.x, transform.position.y, -7);
            }
            else if (transform.position.z >= 7)
            {
               transform.position = new Vector3(transform.position.x, transform.position.y, 7);
            }
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

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyColor();
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation)
    {
        transform.Translate(movement);
        transform.Rotate(rotation);
    }

    [ServerRpc]
    private void FireServerRpc()
    {
        NetworkHelper.Log("Fire");
        bulletSpawner.Fire();
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
        //moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }
}
