using DTEngine.Entities.ComputingDomain;
using System.Text;

namespace DTEngine.Helpers
{
    public static class ResultStringifier
    {
        public static string GetString(decimal[,] data, ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < domainParams.VerticalElementsQuantity; i++)
            {
                for (int j = 0; j < domainParams.HorizontalElementsQuantity; j++)
                {
                    var horId = j + 1;
                    var verId = i;

                    var loc = verId * domainParams.HorizontalElementsQuantity + horId;

                    var node = nodeMap.GetNodeByGlobalAddress(loc);

                    sb.AppendLine($"{node.PosY}\t{node.PosX}\t{data[loc,1]}".Replace(',','.'));
                }
            }
            
            return sb.ToString();
        }
    }
}
