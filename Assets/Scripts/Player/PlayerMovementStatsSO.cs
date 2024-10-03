using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "PlayerMovementStatsSO", menuName = "PlayerMovementStats", order = 0)]
public class PlayerMovementStatsSO : ScriptableObject 
{
    [Header("Walk")]
    [Range(1f,100f)]public float MaxWalkSpeed = 12.5f;
    [Range(0.25f,50f)]public float GroundAcceleration = 5f;
    [Range(0.25f,50f)]public float GroundDeceleration = 20f;
    [Range(0.25f,50f)]public float AirAcceleration = 5f;
    [Range(0.25f,50f)]public float AirDeceleration = 5f;

    [Header("Run")]
    [Range(1f,100f)]public float MaxRunSpeed = 20f;
    
    [Header("Grounded/Collision Checks")]
    public LayerMask GroundedLayer;
    public float GroundDetectionRayLenght = 0.02f;
    public float HeadDetectionRayLenght = 0.02f;
    [Range(0f, 1f)]public float HeadWidth = 0.75f;

    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)]public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)]public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1,5)]public int NumberofJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)]public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.05f, 1f)]public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)]public float ApexHangTime = 0.97f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)]public float JumpBufferTime = 0.125f;
    
    [Header("Jump Coyote Time")]
    [Range(0f, 1f)]public float JumpCoyoteTime = 0.125f;

    [Header("JumpVisualition Tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)]public int ArcResolution = 20;
    [Range(0, 500)]public int VisualizationSteps = 90;  

    public float Gravity {get; private set;}
    public float InitialJumpVelocity {get; private set;}
    public float AdjustedJumpHeight {get; private set;}

    private void OnValidate() 
    {
        CalculateValues();
    }

    private void OnEnable() 
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}

