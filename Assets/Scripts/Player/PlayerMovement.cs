using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referances")]
    public PlayerMovementStatsSO playerMovementStats;
    [SerializeField]private Collider2D _feetColl;
    [SerializeField]private Collider2D _bodyColl;

    private Rigidbody2D rb;

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //jump vars
    public float VerticalVelocity {get; private set;}
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //jump buffer vars
    private float _jumpBufferTime;
    private bool _jumpReleaseDuringBuffer;

    //coyote time vars
    private float _coyoteTimer;


    void Awake()
    {
        _isFacingRight = true;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() 
    {
        JumpChecks();
        CountTimers();

    }

    private void FixedUpdate() 
    {
        CollisionChecks();


        Jump();

        if (_isGrounded)
        {
            Move(playerMovementStats.GroundAcceleration, playerMovementStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(playerMovementStats.AirAcceleration, playerMovementStats.AirDeceleration, InputManager.Movement);
        }
    }

 #region -Movement-
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurningCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * playerMovementStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * playerMovementStats.MaxWalkSpeed;
            }
            
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration *Time.fixedDeltaTime);
            rb.velocity = new Vector2(_moveVelocity.x, rb.velocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(_moveVelocity.x, rb.velocity.y);
        }
    }

    private void TurningCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if(!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
 #endregion

 #region -Jump-
    private void JumpChecks()
    {
        if(InputManager.JumpWasPressed)
        {
            _jumpBufferTime = playerMovementStats.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTime > 0f)
            {
                _jumpReleaseDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = playerMovementStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;

                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
            
        }


        if (_jumpBufferTime > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        //Double jump
        else if (_jumpBufferTime > 0f && _isJumping && _numberOfJumpsUsed < playerMovementStats.NumberofJumpsAllowed)
        {
            _isFalling = false;
            InitiateJump(1);
        }

        else if (_jumpBufferTime > 0f && _isFalling && _numberOfJumpsUsed < playerMovementStats.NumberofJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFalling = false;
        }


        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            VerticalVelocity = Physics2D.gravity.y;
            _numberOfJumpsUsed = 0;
        }

        
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTime = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = playerMovementStats.InitialJumpVelocity;
    }

    private void Jump()
    {   
        if (_isJumping)
        {
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(playerMovementStats.InitialJumpVelocity, 0f , VerticalVelocity);

                if (_apexPoint > playerMovementStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < playerMovementStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                else
                {
                    VerticalVelocity += playerMovementStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }

            }

            else if (!_isFastFalling)
            {
                VerticalVelocity += playerMovementStats.Gravity * playerMovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= playerMovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += playerMovementStats.Gravity * playerMovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < playerMovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / playerMovementStats.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!   _isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += playerMovementStats.Gravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -playerMovementStats.MaxFallSpeed, 50f);

        rb.velocity = new Vector2(rb.velocity.x, VerticalVelocity);
    }

 #endregion
 #region -Collision Check-
    private void IsGroundCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.center.x, playerMovementStats.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, playerMovementStats.GroundDetectionRayLenght, playerMovementStats.GroundedLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.center.x * playerMovementStats.HeadWidth, playerMovementStats.HeadDetectionRayLenght);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, playerMovementStats.HeadDetectionRayLenght, playerMovementStats.GroundedLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }


    private void CollisionChecks()
    {
        IsGroundCheck();
    }

 #endregion
 #region -Timers-
    private void CountTimers()
    {
        _jumpBufferTime -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = playerMovementStats.JumpCoyoteTime;
        }
    }

#endregion








}
