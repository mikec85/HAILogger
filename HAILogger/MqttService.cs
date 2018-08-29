using HAI_Shared;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

namespace HAILogger
{
    public class MqttService
    {
        MqttClient client;
        public static clsHAC HAC2;

        public MqttService(clsHAC hac)
        {
            HAC2 = hac;
        }

        public void Start()
        {
            // create client instance 
            client = new MqttClient(Global.mqtt_address);

            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { Global.mqtt_prefix + "/cmd/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            Event.WriteInfo("MqttServer", " Mqtt Subsribe to " + Global.mqtt_prefix + "/cmd/# at " + Global.mqtt_address);
        }

        public void Stop()
        {
            
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var messagestr = Encoding.Default.GetString(e.Message);
            var topic = e.Topic;

            if (topic.Equals(Global.mqtt_prefix + "/cmd/unit"))
            {
                processunit(messagestr);
            }
            if (topic.Equals(Global.mqtt_prefix + "/cmd/button"))
            {
                processbutton(messagestr);
            }
            if (topic.Equals(Global.mqtt_prefix + "/cmd/arm"))
            {
                processarm(messagestr);
            }
            if (topic.Equals(Global.mqtt_prefix + "/cmd/disarm"))
            {
                //processdisarm(messagestr);
            }

        }

        static void processunit(string msg)
        {
            dynamic m = JsonConvert.DeserializeObject(msg);
            ushort id = m.id;
            string cmdstr = m.cmd;

            Event.WriteVerbose("Mqtt Message", msg);
            if (cmdstr.Equals("on"))
            {
                MqttService.HAC2.SendCommand(enuUnitCommand.On, 0, id);
            }
            else
            {
                MqttService.HAC2.SendCommand(enuUnitCommand.Off, 0, id);
            }

        }

        static void processbutton(string msg)
        {

            dynamic m = JsonConvert.DeserializeObject(msg);
            ushort id = m.id;
            string cmdstr = m.cmd;

            Event.WriteVerbose("Mqtt Message", msg);
            MqttService.HAC2.SendCommand(enuUnitCommand.Button, 0, id);
        }
        static void processarm(string msg)
        {
            dynamic m = JsonConvert.DeserializeObject(msg);
            string modestr = m.mode;
            if (modestr.Equals("Night"))
            {
                WebService.HAC.SendCommand(enuUnitCommand.SecurityNight, 0, 0);
            }
            if (modestr.Equals("Day"))
            {
                WebService.HAC.SendCommand(enuUnitCommand.SecurityDay, 0, 0);
            }
            if (modestr.Equals("Vacation"))
            {
                WebService.HAC.SendCommand(enuUnitCommand.SecurityVac, 0, 0);
            }
            if (modestr.Equals("Away"))
            {
                WebService.HAC.SendCommand(enuUnitCommand.SecurityAway, 0, 0);
            }

        }
        static void processdisarm(string msg)
        {
            WebService.HAC.SendCommand(enuUnitCommand.SecurityOff, 0, 0);
        }
    }
}