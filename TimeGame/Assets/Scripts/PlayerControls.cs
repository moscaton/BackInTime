using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    // Variables set in the inspector
    [Header("Stats")]
    [SerializeField] private float mWalkSpeed;
    [SerializeField] private float mRunSpeed;
    [SerializeField] private float mJumpForce;
    [SerializeField] int hp, maxHP = 5;

    [Header("Game components")]
    [SerializeField] private LayerMask mWhatIsGround;
    [SerializeField] private Transform mGroundCheck;
    [SerializeField] private Transform forkPoint;
    private float kGroundCheckRadius = 0.1f;
    [Range(0, .3f)] [SerializeField] private float mMovementSmoothing = .05f;
    private Vector3 mVelocity = Vector3.zero; //target for smoothings

    // Booleans used to coordinate with the animator's state machine
    private bool mRunning;
    private bool mMoving;
    private bool mGrounded;
    private bool mFalling;
    private bool mAttacking;
    private bool mTakingDamage;

    // References to Player's components
    private Animator mAnimator;
    private Rigidbody2D rb;
    private SpriteRenderer mSpriteRenderer;
    private bool facingRight = true;
    private float horizontal = 0f;
    private bool justJumped = false;

    //Dashing variables
    [Header("Dashing Settings")]
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float mDashForce;
    [SerializeField] private float mDashTime;
    [SerializeField] private float mDashCD;

    private void Start()
    {
        // Get references to other components and game objects
        mAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mSpriteRenderer = transform.GetComponent<SpriteRenderer>();

        hp = maxHP;
    }

    private void Update()
    {
        if (isDashing){
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        mMoving = !Mathf.Approximately(horizontal, 0f);
        

        if (mMoving)
        {
            gameObject.transform.SetParent(null);
        }


        UpdateGrounded();

        //JUMP
        if (mGrounded && Input.GetButtonDown("Jump")){
                justJumped = true;
                mAnimator.SetBool("isJumping", true);
        }
        
        // Run is [Left Shift]
        mRunning = Input.GetButton("Run");

        mAnimator.SetBool("isRunning", mRunning);
        mAnimator.SetBool("isMoving", mMoving);

        if (!mTakingDamage && Input.GetButtonDown("Attack") && !mAttacking)
        {
            mAttacking = true;
            mAnimator.SetBool("isAttacking", mAttacking);
            StartCoroutine(Attack());
        }


        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(Dash());
        }
        

    }
    
    private void FixedUpdate()
    {
        if (isDashing){
            return;
        }
        
        MoveCharacter();

    }


    private bool UpdateGrounded()
    {
        mGrounded = Physics2D.OverlapCircle(mGroundCheck.position, kGroundCheckRadius, mWhatIsGround);
        mAnimator.SetBool("isJumping", !mGrounded);
        return mGrounded;
    }

    private void MoveCharacter()
    {

        if (mMoving){
            
            //No smoothing:
            //rb.velocity = new Vector2(horizontal* (mRunning ? mRunSpeed : mWalkSpeed) * 10f * Time.fixedDeltaTime, rb.velocity.y)

            //Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(horizontal* (mRunning ? mRunSpeed : mWalkSpeed) * 10f * Time.fixedDeltaTime, rb.velocity.y);
            //And then smoothing it out and applying it to the character
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref mVelocity, mMovementSmoothing);

            FaceDirection(horizontal < 0f ? Vector2.left : Vector2.right);
        }

        //jump
        if (justJumped)
        {
            justJumped = false;
            rb.AddForce(new Vector2(rb.velocity.x, mJumpForce * 10f));//rb.velocity = new Vector2(rb.velocity.x, mJumpForce);

        }

    }

    private void FaceDirection(Vector2 direction)
    {
        // Flip the sprite
        mSpriteRenderer.flipX = direction == Vector2.right ? false : true;
        
        facingRight = direction == Vector2.right ? true : false;

        //Flip the direction of where the weapon collider is
        if (!facingRight && (forkPoint.localPosition.x > 0))
        {
            forkPoint.localPosition = new Vector2(forkPoint.localPosition.x * -1, transform.localPosition.y);
        }
        else if (facingRight && (forkPoint.localPosition.x < 0))
        {
            forkPoint.localPosition = new Vector2(forkPoint.localPosition.x * -1, transform.localPosition.y);
        }
    }


    public void TakeDamage(int damage)
    {
        if (mAttacking && mTakingDamage) return; //cant take damage while hitting or if just took
        mTakingDamage = true;
        hp -= damage;
        mAnimator.SetBool("isTakingDamage", mTakingDamage);

        StartCoroutine(TookDamage());
        if (hp <= 0)
        {
            Debug.Log("You would have died");
        }

    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        //TO PLAY WITH reset y speed
        // rb.velocity = new Vector2(rb.velocity.x, 0f);

        // rb.AddForce(new Vector2(mDashForce * (facingRight? 1f:-1f), 0f), ForceMode2D.Impulse);
        rb.velocity = new Vector2(mDashForce * (facingRight? 1f:-1f), 0f);
        
        //TODO anim
        yield return new WaitForSeconds(mDashTime);
        rb.gravityScale = originalGravity;
        isDashing = false;   

        //TO PLAY WITH reset 
        rb.velocity = new Vector2(0f, 0f);

        yield return new WaitForSeconds(mDashCD);
        canDash = true;
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1f);
        mAttacking = false;
        mAnimator.SetBool("isAttacking", mAttacking);
    }
    private IEnumerator TookDamage()
    {
        yield return new WaitForSeconds(1f);
        mTakingDamage = false;
        mAnimator.SetBool("isTakingDamage", false);
    }
}