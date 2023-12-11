//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Messages
{
    [Serializable]
    public class ScanningResponse : Message
    {
        public const string k_RosMessageName = "Messages/Scanning";
        public override string RosMessageName => k_RosMessageName;

        public bool success;
        public string message;
        public long r;
        public long g;
        public long b;

        public ScanningResponse()
        {
            this.success = false;
            this.message = "";
            this.r = 0;
            this.g = 0;
            this.b = 0;
        }

        public ScanningResponse(bool success, string message, long r, long g, long b)
        {
            this.success = success;
            this.message = message;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public static ScanningResponse Deserialize(MessageDeserializer deserializer) => new ScanningResponse(deserializer);

        private ScanningResponse(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.success);
            deserializer.Read(out this.message);
            deserializer.Read(out this.r);
            deserializer.Read(out this.g);
            deserializer.Read(out this.b);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.success);
            serializer.Write(this.message);
            serializer.Write(this.r);
            serializer.Write(this.g);
            serializer.Write(this.b);
        }

        public override string ToString()
        {
            return "ScanningResponse: " +
            "\nsuccess: " + success.ToString() +
            "\nmessage: " + message.ToString() +
            "\nr: " + r.ToString() +
            "\ng: " + g.ToString() +
            "\nb: " + b.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}