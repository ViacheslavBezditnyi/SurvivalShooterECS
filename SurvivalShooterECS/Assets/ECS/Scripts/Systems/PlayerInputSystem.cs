using Google.Api;
using GoogleCloudStreamingSpeechToText;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[DisableAutoCreation]
public partial class PlayerInputSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    private float2 moveInput;
    private float2 lookInput;
    private float shootInput;

    private float _leftLimit = 0.01f;
    private float _rightLimit = 0.01f;
    private float _shootLimit = 0.01f;


    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
   
    protected override void OnStartRunning()
    {
        moveAction = new InputAction("move", binding: "<Gamepad>/rightStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        SpeechRecognizer.onFinalResult.AddListener(ApplySpeechInput);

        moveAction.performed += context =>
        {
            moveInput = context.ReadValue<Vector2>();
        };
        moveAction.canceled += context =>
        {
            moveInput = context.ReadValue<Vector2>();
        };
        moveAction.Enable();

        lookAction = new InputAction("look", binding: "<Mouse>/position");
        lookAction.performed += context =>
        {
            lookInput = context.ReadValue<Vector2>();
        };
        lookAction.canceled += context =>
        {
            lookInput = context.ReadValue<Vector2>();
        };
        lookAction.Enable();

        shootAction = new InputAction("shoot", binding: "<Mouse>/leftButton");
        shootAction.performed += context =>
        {
            shootInput = context.ReadValue<float>();
        };
        shootAction.canceled += context =>
        {
            shootInput = context.ReadValue<float>();
        };
        shootAction.Enable();
    }

    private void ApplySpeechInput(string command)
    {
        switch (command)
        {
            case "up":
                moveInput = new float2(0f, _rightLimit);
                Debug.Log("<color = blue> UP");
                break;
            case "down":
                moveInput = new float2(0f, -_rightLimit);
                Debug.Log("<color = blue> DOWN");
                break;
            case "right":
                moveInput = new float2(_leftLimit, 0f);
                Debug.Log("<color = blue> RIGHT");
                break;
            case "left":
                moveInput = new float2(-_leftLimit, 0f);
                Debug.Log("<color = blue> LEFT");
                break;
            case "stop":
                moveInput = new float2(0f, 0f);
                Debug.Log("<color = blue> LEFT");
                break;
            case "shoot":
                shootInput = _shootLimit;
                break;
            default:
                Debug.Log("<color = red> Unknown command");
                break;
        }
    }

    protected override void OnStopRunning()
    {
        shootAction.Disable();
        lookAction.Disable();
        moveAction.Disable();
        SpeechRecognizer.onFinalResult.RemoveListener(ApplySpeechInput);
    }

    protected override void OnUpdate()
    {
        var moveInputCopy = moveInput;
        var lookInputCopy = lookInput;
        var shootInputCopy = shootInput;

        Entities
            .ForEach((Entity entity, ref PlayerInputData inputData) =>
        {
            inputData.Move = moveInputCopy;
            inputData.Look = lookInputCopy;
            inputData.Shoot = shootInputCopy;
        }).ScheduleParallel();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
