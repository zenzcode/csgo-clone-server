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
        public Vector3 EulerAngles;
        public Vector3 EndEulerAngles;
        public float MouseDeltaX;
        public float MouseDeltaY;
        public float DeltaTime;

        public void Deserialize(Message message)
        {
            ClientId = message.GetUShort();
            Tick = message.GetUInt();
            StartPosition = message.GetVector3();
            EndPosition = message.GetVector3();
            Input = message.GetInt();
            EulerAngles = message.GetVector3();
            EndEulerAngles = message.GetVector3();
            MouseDeltaX = message.GetFloat();
            MouseDeltaY = message.GetFloat();
            DeltaTime = message.GetFloat();
        }

        public void Serialize(Message message)
        {
            Debug.Log("This shouldnt be sent back to client.");
            throw new System.InvalidOperationException();
        }
    }
}