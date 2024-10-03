using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;
    public InputMap inputs;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;


    void Awake()
    {
        inputs = new InputMap();
        inputs.Keyboard.Enable();
        playerInput = GetComponent<PlayerInput>();

        moveAction = inputs.FindAction("Movement");
        jumpAction = inputs.FindAction("Jump");
        runAction = inputs.FindAction("Run");
        
    }

    private void Update() 
    {
        Movement = moveAction.ReadValue<Vector2>();

        JumpWasPressed = jumpAction.WasPressedThisFrame();
        JumpIsHeld = jumpAction.IsPressed();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();

        RunIsHeld = runAction.IsPressed();  
    }




}
