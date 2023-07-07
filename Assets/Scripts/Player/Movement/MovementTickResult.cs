using Riptide;
using UnityEngine;

namespace Player.Movement
{
    public struct MovementTickResult : IMessageSerializable
    {
        public int Tick;
        public Vector3 StartPosition;
        public Vector3 PassedEndPosition;
        public Vector3 ActualEndPosition;
        public Vector3 StartEulerAngles;
        public Vector3 PassedEndEulerAngles;
        public Vector3 ActualEndEulerAngles;
        public float DeltaTime;
        public int Input;

        public void Deserialize(Message message)
        {
            Debug.Log("This shouldnt be deserialized on the server.");
            throw new System.InvalidOperationException();
        }

        public void Serialize(Message message)
        {
            message.AddInt(Tick);
            message.AddVector3(StartPosition);
            message.AddVector3(PassedEndPosition);
            message.AddVector3(ActualEndPosition);
            message.AddVector3(StartEulerAngles);
            message.AddVector3(PassedEndEulerAngles);
            message.AddVector3(ActualEndEulerAngles);
            message.AddFloat(DeltaTime);
            message.AddInt(Input);
        }
    }
}