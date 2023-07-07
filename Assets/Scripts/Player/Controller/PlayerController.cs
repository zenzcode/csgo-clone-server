using Enums;
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

            var result = CalculateLook(movementTick.MouseDeltaX, movementTick.MouseDeltaY, movementTick.DeltaTime, 10);
        }

        private Quaternion CalculateLook(float mouseDeltaX, float mouseDeltaY, float DeltaTime, float sensitivity)
        {
            _yaw += mouseDeltaX * DeltaTime * sensitivity;
            _pitch = Mathf.Clamp(_pitch - (mouseDeltaY * DeltaTime * sensitivity), -89, 89);
            Owner.transform.rotation = Quaternion.Euler(Owner.transform.rotation.x, _yaw, Owner.transform.rotation.z);

            return Owner.transform.rotation;
        }
    }
}