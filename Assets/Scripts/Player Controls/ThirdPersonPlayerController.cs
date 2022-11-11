using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonPlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;


    private PlayerControls controls;


    // Start is called before the first frame update
    void Start()
    {
        ConfigureControlListeners();
    }


    private void ConfigureControlListeners(){
        controls = new PlayerControls();

        //Horizontal 3d Movement (Left/Right)
        controls.Player.HorizontalMovement.performed += ctx => horizontalMovement = ctx.ReadValue<float>();
        controls.Player.HorizontalMovement.canceled += _ => horizontalMovement = 0;

        //Vertical 3d Movement (Forwards/Backwards)
        controls.Player.VerticalMovement.performed += ctx => verticalMovement = ctx.ReadValue<float>();
        controls.Player.VerticalMovement.canceled += _ => verticalMovement = 0;

        controls.Player.HorizontalMovement.canceled += _ => DebugLog();
    }

    // Update is called once per frame
    void Update()
    { 
        ProcessMovement();
    }

    #region Player Movement

    private void ProcessMovement(){
        UpdateMovementVector();
        PerformMovement();
    }

    private float horizontalMovement;
    private float verticalMovement;
    private Vector3 movementVector = new Vector3();

    private void UpdateMovementVector(){
        Debug.Log(horizontalMovement + "," + verticalMovement);
        movementVector.x = horizontalMovement;
        movementVector.y = verticalMovement;
    }

    [SerializeField] private float movementSpeed;

    private void PerformMovement(){
        Debug.Log("Performing Movement");
        Debug.Log("Vector:" + movementVector);
        rb.velocity = movementVector * movementSpeed;
    }


    #endregion

    private void DebugLog(){
        Debug.Log("HELLo");
    }
}
