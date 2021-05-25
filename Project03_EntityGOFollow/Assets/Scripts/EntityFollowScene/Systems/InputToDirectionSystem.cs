using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATM.DOTS.Project03
{
    public class InputToDirectionSystem : SystemBase
    {
        private InputAction _movementInput;
        private InputAction _jumpInput;

        protected override void OnCreate()
        {
            _movementInput = new InputAction(name: "Horizontal Movement",
                                             type: InputActionType.PassThrough);
            _movementInput.AddCompositeBinding("Dpad")
                          .With("Up", "<Keyboard>/w")
                          .With("Down", "<Keyboard>/s")
                          .With("Left", "<Keyboard>/a")
                          .With("Right", "<Keyboard>/d");

            _jumpInput = new InputAction(name: "Jump",
                                         type: InputActionType.Button,
                                         binding: "<Keyboard>/Space");
        }

        protected override void OnStartRunning()
        {
            _movementInput.Enable();
            _jumpInput.Enable();
        }

        protected override void OnStopRunning()
        {
            _movementInput.Disable();
            _jumpInput.Disable();
        }

        protected override void OnUpdate()
        {
            Vector2 moveDirection = _movementInput.ReadValue<Vector2>();
            float jumpDirection = (_jumpInput.phase == InputActionPhase.Started) ? 1f : 0f;

            Entities
                .ForEach((ref DirectionDataComponent directionData) =>
                {
                    directionData.Value = new float3(moveDirection.x, jumpDirection, moveDirection.y);
                })
                .Run();
        }
    }
}