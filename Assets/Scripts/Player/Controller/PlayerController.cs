using Enums;
using Manager;
using Player.Movement;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Controller
{
    public class PlayerController : MonoBehaviour
    {
        public Player Owner { get; set; }

        private float _yaw = 0, _pitch = 0;

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

            CalculateLook(movementTick.MouseDeltaX, movementTick.MouseDeltaY, movementTick.DeltaTime, movementTick.Sensitivity);
            //CalculateMove();
            SendMovementResult(movementTick);
        }

        private void CalculateLook(float mouseDeltaX, float mouseDeltaY, float DeltaTime, float sensitivity)
        {
            _yaw += mouseDeltaX * DeltaTime * sensitivity;
            _pitch = Mathf.Clamp(_pitch - (mouseDeltaY * DeltaTime * sensitivity), -89, 89);
            Owner.transform.rotation = Quaternion.Euler(Owner.transform.rotation.x, _yaw, Owner.transform.rotation.z);
        }

        private void SendMovementResult(MovementTick movementTick)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.TickResult);

            var movementTickResult = new MovementTickResult
            {
                Tick = movementTick.Tick,
                ClientId = movementTick.ClientId,
                StartPosition = movementTick.StartPosition,
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
                Input = movementTick.Input
            };

            message.Add(movementTickResult);

            NetworkManager.Instance.Server.SendToAll(message);
        }
    }
}