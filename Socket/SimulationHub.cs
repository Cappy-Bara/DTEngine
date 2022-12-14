using DTEngine.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace DTEngine.Socket
{
    public class SimulationHub : Hub
    {
        private readonly SimulationContext simulationContext;

        public SimulationHub(SimulationContext simulationContext)
        {
            this.simulationContext = simulationContext;
        }

        public async Task Simulate()
        {
            simulationContext.shouldSimulate = true;

            var dt_time = 0.005m;
            int printTime = (int)(1.0m / dt_time);

            int step = 0;
            while(!Context.ConnectionAborted.IsCancellationRequested)
            {
                simulationContext.CalculateStep(dt_time,step);

                if (step % printTime == 0)
                {
                    var data = simulationContext.GetStepData();
                    var outputData = ResultStringifier.GetString(data, simulationContext.domainParams, simulationContext.nodeMap);

                    await Clients.All.SendAsync("simulation_data",outputData);
                }
                step++;
            }
        }
    }
}
