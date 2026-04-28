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

    [Header("Flag Settings")]
    [SerializeField] private Team team;
    [SerializeField] private Transform holder;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPivot; 
    [SerializeField] private Transform weapon;      
    [SerializeField] private float attackSpeed = 360f;
    [SerializeField] private float attackAngle = 180f;

    private CharacterController controller;
    private Camera cam;
    private float currentSpeed;
    private float stamina;
    private bool isAlive = true;
    private Flag currentFlag;
    private bool swingLeftToRight = true; 
    private bool isAttacking;
    private float currentAngle;
    private int attackDirection = 1;

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
        weapon.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
        RotateToMouse();
        HandleStamina();
        HandleAttackInput();

        if (isAttacking)
        {
            UpdateAttack();
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(-v, 0f, h);
        if (input.sqrMagnitude > 1) input.Normalize();

        bool isMoving = input.sqrMagnitude > 0.01f;
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

        float targetSpeed = walkSpeed;

        if (wantsSprint && stamina > 0f && isMoving)
        {
            targetSpeed = sprintSpeed;
            stamina -= staminaDrain * Time.deltaTime;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

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

    // ----------------- ATTACK -----------------

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        currentAngle = 0f;

        weapon.gameObject.SetActive(true);

        attackPivot.rotation = transform.rotation;

        if (swingLeftToRight)
        {
            // empieza desde la izquierda (-90)
            attackPivot.Rotate(Vector3.up, -attackAngle / 2f);
            attackDirection = 1; // va hacia la derecha
        }
        else
        {
            // empieza desde la derecha (+90)
            attackPivot.Rotate(Vector3.up, attackAngle / 2f);
            attackDirection = -1; // va hacia la izquierda
        }

        // alternar para el próximo click
        swingLeftToRight = !swingLeftToRight;
    }

    void UpdateAttack()
    {
        float step = attackSpeed * Time.deltaTime;
        float delta = step * attackDirection;

        currentAngle += Mathf.Abs(delta);

        attackPivot.Rotate(Vector3.up * delta);

        if (currentAngle >= attackAngle)
        {
            isAttacking = false;

            // resetear pivot
            attackPivot.localRotation = Quaternion.identity;

            // ocultar arma
            weapon.gameObject.SetActive(false);
        }
    }
}
