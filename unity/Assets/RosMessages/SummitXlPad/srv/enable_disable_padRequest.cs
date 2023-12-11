//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.SummitXlPad
{
    [Serializable]
    public class enable_disable_padRequest : Message
    {
        public const string k_RosMessageName = "summit_xl_pad/enable_disable_pad";
        public override string RosMessageName => k_RosMessageName;

        public bool value;

        public enable_disable_padRequest()
        {
            this.value = false;
        }

        public enable_disable_padRequest(bool value)
        {
            this.value = value;
        }

        public static enable_disable_padRequest Deserialize(MessageDeserializer deserializer) => new enable_disable_padRequest(deserializer);

        private enable_disable_padRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.value);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.value);
        }

        public override string ToString()
        {
            return "enable_disable_padRequest: " +
            "\nvalue: " + value.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}