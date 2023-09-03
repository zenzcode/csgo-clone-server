using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
    public struct MovementTick : IMessageSerializable
    {
        public ushort ClientId;
        public uint Tick;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public int Input;
        public float Yaw;
        public float Pitch;
        public float EndYaw;
        public float EndPitch;
        public float MouseDeltaX;
        public float MouseDeltaY;
        public float DeltaTime;
        public float Sensitivity;
        public bool CrouchDown;
        public bool SlowWalkDown;

        public void Deserialize(Message message)
        {
            ClientId = message.GetUShort();
            Tick = message.GetUInt();
            StartPosition = message.GetVector3();
            EndPosition = message.GetVector3();
            Input = message.GetInt();
            Yaw = message.GetFloat();
            EndYaw = message.GetFloat();
            Pitch = message.GetFloat();
            EndPitch = message.GetFloat();
            MouseDeltaX = message.GetFloat();
            MouseDeltaY = message.GetFloat();
            DeltaTime = message.GetFloat();
            Sensitivity = message.GetFloat();
            CrouchDown = message.GetBool();
            SlowWalkDown = message.GetBool();
        }

        public void Serialize(Message message)
        {
            Debug.Log("This shouldnt be sent back to client.");
            throw new System.InvalidOperationException();
        }
    }
}