//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RemoteControl
{
    [Serializable]
    public class CollisionBoxMsg : Message
    {
        public const string k_RosMessageName = "remote_control/CollisionBox";
        public override string RosMessageName => k_RosMessageName;

        public string id;
        public Shape.SolidPrimitiveMsg primitive;
        public Geometry.PoseMsg pose;

        public CollisionBoxMsg()
        {
            this.id = "";
            this.primitive = new Shape.SolidPrimitiveMsg();
            this.pose = new Geometry.PoseMsg();
        }

        public CollisionBoxMsg(string id, Shape.SolidPrimitiveMsg primitive, Geometry.PoseMsg pose)
        {
            this.id = id;
            this.primitive = primitive;
            this.pose = pose;
        }

        public static CollisionBoxMsg Deserialize(MessageDeserializer deserializer) => new CollisionBoxMsg(deserializer);

        private CollisionBoxMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.id);
            this.primitive = Shape.SolidPrimitiveMsg.Deserialize(deserializer);
            this.pose = Geometry.PoseMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.id);
            serializer.Write(this.primitive);
            serializer.Write(this.pose);
        }

        public override string ToString()
        {
            return "CollisionBoxMsg: " +
            "\nid: " + id.ToString() +
            "\nprimitive: " + primitive.ToString() +
            "\npose: " + pose.ToString();
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