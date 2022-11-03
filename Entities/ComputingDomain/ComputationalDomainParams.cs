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
            WidthStep = input.Height / (input.HorizontalElementsQuantity - 1);
            HeightStep = input.Width / (input.VerticalElementsQuantity - 1);
            NumberOfNodes = input.HorizontalElementsQuantity * input.VerticalElementsQuantity;
            NumberOfElements = (input.HorizontalElementsQuantity - 1) * (input.VerticalElementsQuantity - 1);
            HorizontalElementsQuantity = input.HorizontalElementsQuantity;
            VerticalElementsQuantity = input.VerticalElementsQuantity;
        }
    }
}
