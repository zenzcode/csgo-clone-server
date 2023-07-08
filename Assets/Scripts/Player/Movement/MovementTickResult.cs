using Riptide;
using UnityEngine;

namespace Player.Movement
{
    public struct MovementTickResult : IMessageSerializable
    {
        public uint Tick;
        public ushort ClientId;
        public Vector3 StartPosition;
        public Vector3 PassedEndPosition;
        public Vector3 ActualEndPosition;
        public float StartYaw;
        public float PassedEndYaw;
        public float ActualEndYaw;
        public float StartPitch;
        public float PassedEndPitch;
        public float ActualEndPitch;
        public float DeltaTime;
        public float Sensitivity;
        public int Input;

        public void Deserialize(Message message)
        {
            Debug.Log("This shouldnt be deserialized on the server.");
            throw new System.InvalidOperationException();
        }

        public void Serialize(Message message)
        {
            message.AddUShort(ClientId);
            message.AddUInt(Tick);
            message.AddVector3(StartPosition);
            message.AddVector3(PassedEndPosition);
            message.AddVector3(ActualEndPosition);
            message.AddFloat(StartYaw);
            message.AddFloat(PassedEndYaw);
            message.AddFloat(ActualEndYaw);
            message.AddFloat(StartPitch);
            message.AddFloat(PassedEndPitch);
            message.AddFloat(ActualEndPitch);
            message.AddFloat(DeltaTime);
            message.AddFloat(Sensitivity);
            message.AddInt(Input);
        }
    }
}