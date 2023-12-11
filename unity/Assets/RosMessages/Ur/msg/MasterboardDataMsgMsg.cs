//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Ur
{
    [Serializable]
    public class MasterboardDataMsgMsg : Message
    {
        public const string k_RosMessageName = "ur_msgs/MasterboardDataMsg";
        public override string RosMessageName => k_RosMessageName;

        //  This data structure contains the MasterboardData structure
        //  used by the Universal Robots controller
        // 
        //  MasterboardData is part of the data structure being send on the 
        //  secondary client communications interface
        //  
        //  This data structure is send at 10 Hz on TCP port 30002
        //  
        //  Documentation can be found on the Universal Robots Support site, article
        //  number 16496.
        public uint digital_input_bits;
        public uint digital_output_bits;
        public sbyte analog_input_range0;
        public sbyte analog_input_range1;
        public double analog_input0;
        public double analog_input1;
        public sbyte analog_output_domain0;
        public sbyte analog_output_domain1;
        public double analog_output0;
        public double analog_output1;
        public float masterboard_temperature;
        public float robot_voltage_48V;
        public float robot_current;
        public float master_io_current;
        public byte master_safety_state;
        public byte master_onoff_state;

        public MasterboardDataMsgMsg()
        {
            this.digital_input_bits = 0;
            this.digital_output_bits = 0;
            this.analog_input_range0 = 0;
            this.analog_input_range1 = 0;
            this.analog_input0 = 0.0;
            this.analog_input1 = 0.0;
            this.analog_output_domain0 = 0;
            this.analog_output_domain1 = 0;
            this.analog_output0 = 0.0;
            this.analog_output1 = 0.0;
            this.masterboard_temperature = 0.0f;
            this.robot_voltage_48V = 0.0f;
            this.robot_current = 0.0f;
            this.master_io_current = 0.0f;
            this.master_safety_state = 0;
            this.master_onoff_state = 0;
        }

        public MasterboardDataMsgMsg(uint digital_input_bits, uint digital_output_bits, sbyte analog_input_range0, sbyte analog_input_range1, double analog_input0, double analog_input1, sbyte analog_output_domain0, sbyte analog_output_domain1, double analog_output0, double analog_output1, float masterboard_temperature, float robot_voltage_48V, float robot_current, float master_io_current, byte master_safety_state, byte master_onoff_state)
        {
            this.digital_input_bits = digital_input_bits;
            this.digital_output_bits = digital_output_bits;
            this.analog_input_range0 = analog_input_range0;
            this.analog_input_range1 = analog_input_range1;
            this.analog_input0 = analog_input0;
            this.analog_input1 = analog_input1;
            this.analog_output_domain0 = analog_output_domain0;
            this.analog_output_domain1 = analog_output_domain1;
            this.analog_output0 = analog_output0;
            this.analog_output1 = analog_output1;
            this.masterboard_temperature = masterboard_temperature;
            this.robot_voltage_48V = robot_voltage_48V;
            this.robot_current = robot_current;
            this.master_io_current = master_io_current;
            this.master_safety_state = master_safety_state;
            this.master_onoff_state = master_onoff_state;
        }

        public static MasterboardDataMsgMsg Deserialize(MessageDeserializer deserializer) => new MasterboardDataMsgMsg(deserializer);

        private MasterboardDataMsgMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.digital_input_bits);
            deserializer.Read(out this.digital_output_bits);
            deserializer.Read(out this.analog_input_range0);
            deserializer.Read(out this.analog_input_range1);
            deserializer.Read(out this.analog_input0);
            deserializer.Read(out this.analog_input1);
            deserializer.Read(out this.analog_output_domain0);
            deserializer.Read(out this.analog_output_domain1);
            deserializer.Read(out this.analog_output0);
            deserializer.Read(out this.analog_output1);
            deserializer.Read(out this.masterboard_temperature);
            deserializer.Read(out this.robot_voltage_48V);
            deserializer.Read(out this.robot_current);
            deserializer.Read(out this.master_io_current);
            deserializer.Read(out this.master_safety_state);
            deserializer.Read(out this.master_onoff_state);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.digital_input_bits);
            serializer.Write(this.digital_output_bits);
            serializer.Write(this.analog_input_range0);
            serializer.Write(this.analog_input_range1);
            serializer.Write(this.analog_input0);
            serializer.Write(this.analog_input1);
            serializer.Write(this.analog_output_domain0);
            serializer.Write(this.analog_output_domain1);
            serializer.Write(this.analog_output0);
            serializer.Write(this.analog_output1);
            serializer.Write(this.masterboard_temperature);
            serializer.Write(this.robot_voltage_48V);
            serializer.Write(this.robot_current);
            serializer.Write(this.master_io_current);
            serializer.Write(this.master_safety_state);
            serializer.Write(this.master_onoff_state);
        }

        public override string ToString()
        {
            return "MasterboardDataMsgMsg: " +
            "\ndigital_input_bits: " + digital_input_bits.ToString() +
            "\ndigital_output_bits: " + digital_output_bits.ToString() +
            "\nanalog_input_range0: " + analog_input_range0.ToString() +
            "\nanalog_input_range1: " + analog_input_range1.ToString() +
            "\nanalog_input0: " + analog_input0.ToString() +
            "\nanalog_input1: " + analog_input1.ToString() +
            "\nanalog_output_domain0: " + analog_output_domain0.ToString() +
            "\nanalog_output_domain1: " + analog_output_domain1.ToString() +
            "\nanalog_output0: " + analog_output0.ToString() +
            "\nanalog_output1: " + analog_output1.ToString() +
            "\nmasterboard_temperature: " + masterboard_temperature.ToString() +
            "\nrobot_voltage_48V: " + robot_voltage_48V.ToString() +
            "\nrobot_current: " + robot_current.ToString() +
            "\nmaster_io_current: " + master_io_current.ToString() +
            "\nmaster_safety_state: " + master_safety_state.ToString() +
            "\nmaster_onoff_state: " + master_onoff_state.ToString();
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