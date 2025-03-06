using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WindowsServerManager.Libraries.Docker
{
    public class DockerCli : IDisposable
    {
        private NamedPipeClientStream _dockerPipe;
        public delegate void DockerStatUpdate(List<DockerStat> stats);
        public event DockerStatUpdate? OnContainerUpdate;

        private readonly SynchronizationContext _synchronizationContext;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public List<DockerStat> Containers { get; set; } = null!;

        public DockerCli(SynchronizationContext context, CancellationTokenSource cancellationTokenSource)
        {
            _synchronizationContext = context;
            _dockerPipe = new NamedPipeClientStream(".", "docker_engine", PipeDirection.InOut);
            _cancellationTokenSource = cancellationTokenSource;

            try
            {
                _dockerPipe.Connect(2000);
            }
            catch
            {
                throw new Exception(); // Don't remove 
            }
            finally
            {
                Task.Run(UpdateStats, _cancellationTokenSource.Token);
            }

        }


        private string? SendAndReceive(string data, string httpType = "GET", bool returnStatusCode = false)
        {
            string endpoint = $"{httpType} {data} HTTP/1.0\r\n\r\n";
            byte[] requestBytes = Encoding.ASCII.GetBytes(endpoint);
            
            if (!_dockerPipe.IsConnected)
            {
                _dockerPipe = new NamedPipeClientStream(".", "docker_engine", PipeDirection.InOut);
                _dockerPipe.Connect(2000);
            }

            _dockerPipe.Write(requestBytes, 0, requestBytes.Length);
            _dockerPipe.Flush();
            
            using StreamReader reader = new(_dockerPipe);

            string line = "";

            while (_dockerPipe.IsConnected) line += reader.ReadLine();

            if (returnStatusCode)
                line = line[(line.IndexOf(' ') + 1)..(line.IndexOf(' ') + 4)]; // This could be improved, but it shouldn't ever fail
            else
            {
                /*
                 * This is so we can check if it starts as a JArray or JObject and substring accordingly
                 */

                int firstPass = line.IndexOf('[');
                int secondPass = line.IndexOf('{');

                if (firstPass == -1 && secondPass == -1) line = "";
                else
                {
                    int startIndex = firstPass == -1 ?
                        secondPass :
                        secondPass == -1 ? firstPass :
                        Math.Min(firstPass, secondPass);

                    line = line[startIndex..];
                }

            }

            Console.WriteLine(line);

            if (_dockerPipe.IsConnected) return line;

            _dockerPipe = new NamedPipeClientStream(".", "docker_engine", PipeDirection.InOut);
            _dockerPipe.Connect(2000);

            return line;
        }

        public List<DockerStat> GetContainers()
        {
            List<DockerStat> containersFound = new (); // Converting to [] breaks execution do not modify
            string? containerList = SendAndReceive("/containers/json?all=true");
            if (string.IsNullOrEmpty(containerList) || !containerList.Contains("ImageID")) return containersFound;
            List<JsonNode>? containerObject = JsonSerializer.Deserialize<List<JsonNode>>(containerList);
            if (containerObject == null) return containersFound;

            foreach (JsonNode? container in containerObject)
            {
                if (_cancellationTokenSource.IsCancellationRequested) break;

                bool active = container?["State"]?.ToString() == "running";
                string name = container?["Names"]?[0]?.ToString() ?? "N/A";
                string containerId = container?["Id"]?.ToString() ?? "0";
                string image = container?["Image"]?.ToString() ?? "N/A";
                string ports = "";
                if (container?["Ports"] is JsonArray { Count: > 0 } portsArray)
                {
                    ports = $"{portsArray[0]?["PublicPort"]}:{portsArray[0]?["PrivatePort"] ?? "N/A"}";
                }


                string lastStarted = container?["Status"]?.ToString() ?? "N/A";

                containersFound.Add(new DockerStat(active, name, containerId, image, ports, lastStarted));
            }

            Containers = containersFound;
            
            return containersFound;
        }

        public bool StartContainer(string containerId) => (SendAndReceive($"/containers/{containerId}/start", "POST", true) ?? "500") == "204";

        public bool StopContainer(string containerId) => (SendAndReceive($"/containers/{containerId}/stop", "POST", true) ?? "500") == "204";

        public bool DeleteContainer(string containerId) => (SendAndReceive($"/containers/{containerId}", "DELETE", true) ?? "500") == "204";

        private StatApiReturn? GetContainerStats(string containerId) => JsonSerializer.Deserialize<StatApiReturn>(SendAndReceive($"/containers/{containerId}/stats?stream=false") ?? "[]");

        private async Task UpdateStats()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    List<DockerStat> containers = GetContainers(); // Update Containers
                    foreach (DockerStat container in containers)
                    {
                        if (_cancellationTokenSource.IsCancellationRequested) break;

                        StatApiReturn? containerData = GetContainerStats(container.ContainerId);
                        if (containerData == null) continue;
                        CpuInternalStats stats = DockerStatsFormatter.FormatStats(containerData);

                        container.Cpu = stats.Cpu;
                        container.MemUsage = stats.MemUsage;
                        container.MemPercent = stats.MemPercent;
                        container.Pid = stats.Pid;
                        container.NetIo = stats.NetIo;
                    }

                    _synchronizationContext.Post(_ =>
                    {
                        OnContainerUpdate?.Invoke(containers);
                    }, null);
                }
                catch (Exception ex)
                {
                   // Console.WriteLine(ex);
                }

                await Task.Delay(1500);
            }
        }

        public void Dispose() => _dockerPipe.Dispose();


    }
}
