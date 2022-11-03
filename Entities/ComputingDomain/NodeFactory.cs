using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.ComputingDomain
{
    public class NodeFactory
    {
        private readonly ComputationalDomainParams domainParams;
        public NodeFactory(ComputationalDomainParams domainParams)
        {
            this.domainParams = domainParams;
        }

        public Node CreateNode(int globalId)
        {
            var column = (globalId - 1) % domainParams.HorizontalElementsQuantity;
            var row = (globalId - 1) / domainParams.VerticalElementsQuantity;

            var posX = column * domainParams.WidthStep;
            var posY = row * domainParams.HeightStep;

            return new Node(globalId, posX, posY);
        }
    }
}
