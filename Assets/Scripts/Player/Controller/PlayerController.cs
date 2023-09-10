using Enums;
using Manager;
using Player.Movement;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Controller
{
    public class PlayerController : MonoBehaviour
    {
        public Player Owner { get; set; }

        private float _yaw = 0, _pitch = 0;

        [SerializeField] private float defaultMovementSpeed = 6;

        [SerializeField] private float crouchMovementSpeed = 3;

        [SerializeField] private float slowWalkMovementSpeed = 3;

        [SerializeField] private LayerMask playerLayer;

        private Rigidbody _rigidbody;

        private CapsuleCollider _capsuleCollider;

        private Vector3 _lastStartPos = Vector3.zero;

        private Collider[] collisions = new Collider[10];

        private PlayerMovementState _playerMovementState = PlayerMovementState.Default;

        private float _movementSpeed = 0;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _movementSpeed = defaultMovementSpeed;
        }

        [MessageHandler((ushort)ClientToServerMessages.Tick)]
        private static void ReceivedTick(ushort sender, Message message)
        {
            var player = PlayerManager.Instance.GetPlayer(sender);
            if(!player)
            {
                return;
            }

            var playerController = player.GetComponentInChildren<PlayerController>();

            if(!playerController)
            {
                return;
            }

            playerController.HandleTick(message);
        }

        public void HandleTick(Message message)
        {
            if(!Owner)
            {
                return;
            }

            var movementTick = message.GetSerializable<MovementTick>();

            if(movementTick.CrouchDown)
            {
                _playerMovementState = PlayerMovementState.Crouching;
                _movementSpeed = crouchMovementSpeed;
            }
            else if(movementTick.SlowWalkDown)
            {
                _playerMovementState = PlayerMovementState.SlowWalk;
                _movementSpeed = slowWalkMovementSpeed;
            }
            else
            {
                _playerMovementState = PlayerMovementState.Default;
                _movementSpeed = defaultMovementSpeed;
            }

            _lastStartPos = Owner.transform.position;
            CalculateLook(movementTick.MouseDeltaX, movementTick.MouseDeltaY, movementTick.DeltaTime, movementTick.Sensitivity);
            CalculateMove( movementTick.Input, movementTick.DeltaTime);
            SendMovementResult(movementTick);
        }

        private void CalculateLook(float mouseDeltaX, float mouseDeltaY, float deltaTime, float sensitivity)
        {
            _yaw += mouseDeltaX * deltaTime * sensitivity;
            _pitch = Mathf.Clamp(_pitch - (mouseDeltaY * deltaTime * sensitivity), -89, 89);
            Owner.transform.rotation = Quaternion.Euler(Owner.transform.rotation.x, _yaw, Owner.transform.rotation.z);
        }

        private void CalculateMove(int input, float deltaTime)
        {
            Vector2 pressedInputs = GetVectorFromInput(input);

            Vector3 moveVector = Owner.transform.forward * pressedInputs.y + Owner.transform.right * pressedInputs.x;
            
            Vector3 targetPosition = Owner.transform.position + moveVector * _movementSpeed * deltaTime;

            Array.Clear(collisions, 0, collisions.Length);

            int collisionNum = Physics.OverlapCapsuleNonAlloc(targetPosition, targetPosition + Vector3.up * _capsuleCollider.bounds.extents.y, 0.5f, collisions, playerLayer);

            if (collisionNum != 0)
            {
                foreach (Collider collision in collisions)
                {
                    //collision is invalid or we collided with ourselves
                    if (!collision || collision.transform.root == transform.root)
                    {
                        continue;
                    }

                    //collision with non-player somehow
                    if (!collision.TryGetComponent<PlayerController>(out PlayerController playerController))
                    {
                        continue;
                    }

                    return;
                }
            }


            Owner.transform.position = targetPosition;
        }

        private Vector2 GetVectorFromInput(int input)
        {
            Vector2 result = Vector2.zero;

            if((1 << 0 & input) != 0)
            {
                result.x = 1;
            }
            else if((1 << 1 & input) != 0)
            {
                result.x = -1;
            }
            
            if((1 << 2 & input) != 0)
            {
                result.y = 1;
            }
            else if((1 << 3 & input) != 0)
            {
                result.y = -1;
            }

            return result;
        }

        private void SendMovementResult(MovementTick movementTick)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.TickResult);

            var movementTickResult = new MovementTickResult
            {
                Tick = movementTick.Tick,
                ClientId = movementTick.ClientId,
                StartPosition = movementTick.StartPosition,
                ActualStartPosition = _lastStartPos,
                PassedEndPosition = movementTick.EndPosition,
                ActualEndPosition = Owner.transform.position,
                StartYaw = movementTick.Yaw,
                PassedEndYaw = movementTick.EndYaw,
                ActualEndYaw = _yaw,
                StartPitch = movementTick.Pitch,
                PassedEndPitch = movementTick.EndPitch,
                ActualEndPitch = _pitch,
                DeltaTime = movementTick.DeltaTime,
                Sensitivity = movementTick.Sensitivity,
                Input = movementTick.Input,
                PlayerMovementState = _playerMovementState
            };

            message.Add(movementTickResult);

            NetworkManager.Instance.Server.SendToAll(message);
        }
    }
}