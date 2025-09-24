using Opc.UaFx;
using Opc.UaFx.Client;

namespace Agent.OpcUa
{
    public class OpcUaClient
    {
        private readonly OpcClient _client;

        public OpcUaClient(string endpointUrl)
        {
            _client = new OpcClient(endpointUrl);
        }

        public void Connect() => _client.Connect();

        public OpcValue ReadNode(string nodeId)
        {
            return _client.ReadNode(nodeId);
        }

        public void WriteNode(string nodeId, object value)
        {
            _client.WriteNode(nodeId, value);
        }

        public void InvokeMethod(string objectId, string methodId, params object[] args)
        {
            _client.CallMethod(objectId, methodId, args);
        }

        public void Disconnect() => _client.Disconnect();
    }
}