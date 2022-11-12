using DTEngine.Contracts;

namespace DTEngine.Entities.ComputingDomain
{
    public class ComputationalDomainParams
    {
        public decimal WidthStep { get; }
        public decimal HeightStep { get; }

        public int NumberOfElements { get; }
        public int NumberOfNodes { get; }

        public int HorizontalElementsQuantity { get; set; }
        public int VerticalElementsQuantity { get; set; }

        public ComputationalDomainParams(InputData input)
        {
            WidthStep = input.Height / (input.HorizontalNodesQuantity - 1);
            HeightStep = input.Width / (input.VerticalNodesQuantity - 1);
            NumberOfNodes = input.HorizontalNodesQuantity * input.VerticalNodesQuantity;
            NumberOfElements = (input.HorizontalNodesQuantity - 1) * (input.VerticalNodesQuantity - 1);
            HorizontalElementsQuantity = input.HorizontalNodesQuantity;
            VerticalElementsQuantity = input.VerticalNodesQuantity;
        }
    }
}
