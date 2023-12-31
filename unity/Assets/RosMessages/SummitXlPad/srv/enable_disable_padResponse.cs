//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.SummitXlPad
{
    [Serializable]
    public class enable_disable_padResponse : Message
    {
        public const string k_RosMessageName = "summit_xl_pad/enable_disable_pad";
        public override string RosMessageName => k_RosMessageName;

        public bool ret;

        public enable_disable_padResponse()
        {
            this.ret = false;
        }

        public enable_disable_padResponse(bool ret)
        {
            this.ret = ret;
        }

        public static enable_disable_padResponse Deserialize(MessageDeserializer deserializer) => new enable_disable_padResponse(deserializer);

        private enable_disable_padResponse(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.ret);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.ret);
        }

        public override string ToString()
        {
            return "enable_disable_padResponse: " +
            "\nret: " + ret.ToString();
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
