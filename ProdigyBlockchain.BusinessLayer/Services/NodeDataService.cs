using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Networks;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Services
{
    public interface INodeDataService
    {
        Guid CertCreated(DocumentCreateCommand command);
        bool JoinNetwork();
        NodeJoinResponse RequestToJoinNetwork(NodeJoinCommand command);
        bool NodePing(Guid node_id);
        PingResponse Ping();

    }

    public class NodeDataService : INodeDataService
    {
        private INetwork _Network;
        private IDocumentBlockchain _Blockchain;
        private RestClient _Client;

        private CryptoService _CryptoService;

        private IPAddress _NodeIPAddress;
        private Guid _NodeId;

        public NodeDataService(Guid node_id, INetwork network, IPAddress current_ip_address, IDocumentBlockchain blockchain, string privateKey)
        {
            _Network = network;
            _Blockchain = blockchain;

            _NodeIPAddress = current_ip_address;
            _NodeId = node_id;

            _CryptoService = new CryptoService(privateKey);
        }

        public Guid CertCreated(DocumentCreateCommand command)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Got cert: " + command.identifier1);
            Console.ForegroundColor = ConsoleColor.White;
            return _Blockchain.AddDataFromNode(command);
        }

        public bool NodePing(Guid node_id)
        {
            var node_exists = _Network.NodeList.FirstOrDefault(m => m.node_id == node_id);

            if(node_exists != null)
            {
                node_exists.last_ack = DateTimeOffset.UtcNow;

                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.WriteLine(node_id.ToString() + " checking if still there...");
                //Console.ForegroundColor = ConsoleColor.White;

                var url = "http://" + node_exists.ip_address.ToString() + ":" + _Network.DefaultAPIPort + "/";

                _Client = new RestClient(url);
                var request = new RestRequest("api/Node/Ping", Method.GET, DataFormat.Json);
                request.AddHeader("Authorization", "JWT " + _Network.NodePassword);

                var response = _Client.Execute<PingResponse>(request);
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    if(response.Data != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }

            return false;
        }

        public PingResponse Ping()
        {
            PingResponse response = new PingResponse();
            response.now = DateTimeOffset.UtcNow;

            return response;
        }

        public NodeJoinResponse RequestToJoinNetwork(NodeJoinCommand command)
        {
            var node_exists = _Network.NodeList.FirstOrDefault(m => m.ip_address.ToString() == command.ip_address);

            if(node_exists == null)
            {
                if (command.difficulty == _Network.Difficulty)
                {
                    List<NodeRegisteryDto> nodes = new List<NodeRegisteryDto>();
                    nodes.Add(new NodeRegisteryDto()
                    {
                        ip_address = _NodeIPAddress.ToString(),
                        node_id = _NodeId
                    });

                    if (_Network.NodeList.Count() > 0)
                    {
                        var rand = new Random((int)DateTime.UtcNow.Ticks);
                        var random_node = _Network.NodeList.ElementAt(rand.Next(_Network.NodeList.Count()));
                        nodes.Add(new NodeRegisteryDto()
                        {
                            ip_address = random_node.ip_address.ToString(),
                            node_id = random_node.node_id,
                            first_seen = random_node.first_seen
                        });
                    }

                    _Network.NodeList.Add(new NodeRegistery()
                    {
                        node_id = command.node_id,
                        ip_address = IPAddress.Parse(command.ip_address),
                    });

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(command.ip_address + " joined the network.");
                    Console.ForegroundColor = ConsoleColor.White;

                    return new NodeJoinResponse() { accepted = true, difficulty = _Network.Difficulty, nodes = nodes};
                }
                else
                {
                    return new NodeJoinResponse() { accepted = false };
                }
            }
            else
            {
                _Network.NodeList.Remove(node_exists);
                return new NodeJoinResponse() { accepted = false };
            }
        }

        public bool JoinNetwork()
        {

            foreach (var address in _Network.IPSeeds.Where(m=> m.ToString() != _NodeIPAddress.ToString()))
            {
                if(_NodeIPAddress.ToString() != address.ToString())
                {
                    var command = new NodeJoinCommand()
                    { 
                        node_id = _NodeId,
                        difficulty = _Network.Difficulty,
                        ip_address = _NodeIPAddress.ToString(),
                    };


                    var url = "http://" + address.ToString() + ":" + _Network.DefaultAPIPort + "/";

                    _Client = new RestClient(url);
                    var request = new RestRequest("api/Node/RequestToJoinNetwork", DataFormat.Json);
                    request.AddHeader("Authorization", "JWT " + _Network.NodePassword);
                    request.AddJsonBody(command);

                    var response = _Client.Execute(request, Method.POST);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var node_response = JsonConvert.DeserializeObject<NodeJoinResponse>(response.Content);
                        if(node_response.accepted)
                        {
                            foreach(var node in node_response.nodes)
                            {
                                _Network.NodeList.Add(new NodeRegistery()
                                {
                                    node_id = node.node_id,
                                    ip_address = IPAddress.Parse(node.ip_address),
                                    first_seen = node.first_seen
                                });
                            }

                            return node_response.accepted;
                        }
                    }
                }
            }

            // CHecking if this is the seed node
            if(_Network.IPSeeds.Count() == 1 && _Network.IPSeeds.First().ToString() == _NodeIPAddress.ToString())
            {
                return true; // Sending a true so the node doesn't shut down
            }

            return false;
        }


    }
}
