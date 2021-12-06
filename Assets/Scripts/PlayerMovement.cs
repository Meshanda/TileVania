using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Vector2 moveInput;
    Rigidbody2D myRigidbody2D;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider2D;
    BoxCollider2D myFeetCollider2D;
    float currentGravity;
    bool canDoubleJump;
    bool isAlive = true;

    bool hasDied = false;
    
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 25f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] private Vector2 deathKick = new Vector2(10f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider2D = GetComponent<CapsuleCollider2D>();
        myFeetCollider2D = GetComponent<BoxCollider2D>();
        currentGravity = myRigidbody2D.gravityScale;
    }

    void Update()
    {
        Run();
        ClimbLadder();
        FlipSprite();
    }
    
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);
    }
    
    void OnJump(InputValue value)
    {
        LayerMask groundMask = LayerMask.GetMask("Ground");
        
        if (value.isPressed)
        {
            if (myFeetCollider2D.IsTouchingLayers(groundMask))
            {
                myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, 0f);
                myRigidbody2D.velocity += new Vector2(0f, jumpSpeed);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, 0f);
                myRigidbody2D.velocity += new Vector2(0f, jumpSpeed);
                canDoubleJump = false;
            }
        }
    }
    
    void ClimbLadder()
    {
        int climbingMask = LayerMask.GetMask("Climbing");

        if (!myFeetCollider2D.IsTouchingLayers(climbingMask))
        {
            myAnimator.SetBool("isClimbing", false);
            myRigidbody2D.gravityScale = currentGravity;
            return;
        }

        Vector2 climbVelocity = new Vector2(myRigidbody2D.velocity.x, moveInput.y * climbSpeed);
        myRigidbody2D.velocity = climbVelocity;
        myRigidbody2D.gravityScale = 0;

        bool playerIsClimbing = Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        
        myAnimator.SetBool("isClimbing", playerIsClimbing);
    }
    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody2D.velocity.y);
        myRigidbody2D.velocity = playerVelocity;
        
        bool playerIsMoving = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;

        if (playerIsMoving)
        {
            myAnimator.SetBool("isRunning", true);
        }
        else
        {
            myAnimator.SetBool("isRunning", false);
        }
    }
    void FlipSprite()
    {
        bool playerIsMoving = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;

        if (playerIsMoving)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x), 1f);
        }
    }

    void OnFire(InputValue value)
    {
        Instantiate(bullet, gun.position, transform.rotation);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            Die();
        }   
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazards"))
        {
            Die();
        } 
    }

    void Die()
    {
        if (hasDied) return;
        
        hasDied = true;
        
        isAlive = false;
        
        PlayerInput input = GetComponent<PlayerInput>();
        input.actions.Disable();
        
        myAnimator.SetTrigger("Dying");

        myRigidbody2D.velocity = deathKick;

        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }
}