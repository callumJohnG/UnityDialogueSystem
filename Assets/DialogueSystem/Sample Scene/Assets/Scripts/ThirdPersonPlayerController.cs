using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonPlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;


    private PlayerControls controls;


    // Start is called before the first frame update
    void Awake()
    {
        ConfigureControlListeners();
    }

    private void OnEnable(){
        controls.Enable();
    }

    private void OnDisable(){
        controls.Disable();
    }

    private void ConfigureControlListeners(){
        controls = new PlayerControls();

        //Horizontal 3d Movement (Left/Right)
        controls.Player.HorizontalMovement.performed += ctx => horizontalMovement = ctx.ReadValue<float>();
        controls.Player.HorizontalMovement.canceled += _ => horizontalMovement = 0;

        //Vertical 3d Movement (Forwards/Backwards)
        controls.Player.VerticalMovement.performed += ctx => verticalMovement = ctx.ReadValue<float>();
        controls.Player.VerticalMovement.canceled += _ => verticalMovement = 0;

        //Interacting
        controls.Player.Interact.performed += _ => AttemptToInteract();
    }

    // Update is called once per frame
    void Update()
    { 
        if(!isActive)return;

        ProcessMovement();

        UpdateVisuals();
    }

    #region Player Movement

    private void ProcessMovement(){
        if(!isActive)return;

        UpdateMovementVector();
        PerformMovement();
    }

    private float horizontalMovement;
    private float verticalMovement;
    private Vector3 movementVector = new Vector3();
    [SerializeField] private float movementSpeed;

    private void UpdateMovementVector(){
        movementVector.x = horizontalMovement;
        movementVector.z = verticalMovement;
        movementVector *= movementSpeed;
    }


    private void PerformMovement(){
        //if(movementVector == Vector3.zero)return;

        rb.velocity = new Vector3(movementVector.x, rb.velocity.y, movementVector.z);

        rb.velocity = movementVector * movementSpeed;
    }

    #endregion

    #region Player Active Toggle

    private bool isActive = true;

    public void ToggleActive(){
        isActive = !isActive;

        if(isActive){
            Activate();
        } else{
            Deactivate();
        }
    }

    public void Activate(){
        isActive = true;
    }

    public void Deactivate(){
        isActive = false;
        KillMomentum();
    }

    private void KillMomentum(){
        rb.velocity = Vector3.zero;
    }

    #endregion

    #region Player Interaction

    [Header("Interacting")]
    [SerializeField] private float interactionRange = 1;
    [SerializeField] private Vector3 interactionCenter;

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionCenter + transform.position, interactionRange);
    }

    private void AttemptToInteract(){
        if(!isActive)return;

        //Do an overlap sphere to get all objects in the range
        Collider[] hitColliders = Physics.OverlapSphere(interactionCenter + transform.position, interactionRange);


        Interactable closestInteractable = null;
        float closestDistance = Mathf.Infinity;
        
        foreach(Collider collider in hitColliders){
            if(!collider.CompareTag("Interactable"))continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if(distance >= closestDistance)continue;

            //Found a new closest interactable
            closestInteractable = collider.GetComponent<Interactable>();
            closestDistance = distance;
        }

        if(closestInteractable == null){
            return;
        }
        //We have an interactable to interact with
        closestInteractable.Interact();
    }

    #endregion

    #region Visuals

    [Header("Visuals")]
    [SerializeField] private GameObject visualsContainer;
    [SerializeField] private float visualsRotateSpeed = 1;

    private void UpdateVisuals(){
        if(!isActive)return;

        FaceWalkDirection();
    }

    private void FaceWalkDirection(){
        //Only rotate the player if they are moving
        if(movementVector == Vector3.zero)return;

        Quaternion newRotation = Quaternion.LookRotation(movementVector, Vector3.up);
        
        visualsContainer.transform.rotation = Quaternion.Lerp(visualsContainer.transform.rotation, newRotation, Time.deltaTime * visualsRotateSpeed);
        
    }

    #endregion
}
