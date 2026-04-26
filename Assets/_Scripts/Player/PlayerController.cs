using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IFlagCarrier
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float acceleration;

    [Header("Stamina")]
    public float maxStamina;
    public float staminaDrain;
    public float staminaRecovery;

    [Header("Rotation")]
    public LayerMask groundMask;

    private CharacterController controller;
    private Camera cam;
    private float currentSpeed;
    private float stamina;
    
    [Header("Flag Settings")]
    [SerializeField] private Team team;
    [SerializeField] private Transform holder;
    private bool isAlive = true;
    private Flag currentFlag;
    public Transform FlagHolder => holder; 
    public Transform Transform => transform;
    public Team Team => team;
    public bool notDead => isAlive;
    public bool HasFlag => currentFlag != null;
    public Flag CurrentFlag => currentFlag;


    public void SetFlag(Flag flag)
    {
        currentFlag = flag;
    }

    public void ClearFlag()
    {
        currentFlag = null;
    }

    public void Die()
    {
        isAlive = false;

        if (currentFlag != null)
        {
            currentFlag.Drop(transform.position);
            ClearFlag();
        }
    }


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        stamina = maxStamina;
    }

    void Update()
    {
        HandleMovement();
        RotateToMouse();
        HandleStamina();
        
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(-v, 0f, h);
        if (input.sqrMagnitude > 1) input.Normalize();

        bool isMoving = input.sqrMagnitude > 0.01f;
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

        // Determinar velocidad objetivo
        float targetSpeed = walkSpeed;

        if (wantsSprint && stamina > 0f && isMoving)
        {
            targetSpeed = sprintSpeed;
            stamina -= staminaDrain * Time.deltaTime;
        }

        // Interpolación suave de velocidad
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Movimiento
        controller.Move(input * currentSpeed * Time.deltaTime);
    }

    void HandleStamina()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && currentSpeed > walkSpeed;

        if (!isSprinting)
        {
            stamina += staminaRecovery * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    void RotateToMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector3 lookPos = hit.point - transform.position;
            lookPos.y = 0f;

            if (lookPos != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookPos);
            }
        }
    }
}
