//#define TEST_DATA
#define SHOULD_GAUSS
#define VERTICAL_GAUSS

using Accord.Math;
using DTEngine.Contracts;
using DTEngine.Entities;
using DTEngine.Entities.ComputingDomain;
using DTEngine.Entities.CurrentFlow;
using DTEngine.Entities.FVector;
using DTEngine.Entities.FVector.FAlpha;
using DTEngine.Entities.Gauss;
using DTEngine.Helpers;

namespace DTEngine
{
    public class SimulationContext
    {
        public NodeMap? nodeMap;
        public ComputationalDomainParams? domainParams;
        private CurrentHeatSource? currentHeatSourceGenerator;
        private FiQFactory? fiQFactory;
        private CalculusSolver? solver;

        private decimal[,]? f;
        private decimal[,]? KSum;

        public bool shouldSimulate = false;



        public string Initialize(InputData input)
        {
#if TEST_DATA
InputFileLocation = "C:\\Repos\\DTEngine\\InputFileTest.json";
input = FileHandler.ReadFromFile(InputFileLocation);
#endif
            domainParams = new ComputationalDomainParams(input);

#if VERBOSE
Console.WriteLine("Grid params:");
Console.WriteLine($"Number of elements: {domainParams.NumberOfElements}");
Console.WriteLine($"Number of nodes: {domainParams.NumberOfNodes}");
#endif

            nodeMap = new NodeMap(domainParams);

            //input.HeatSourcePower = 20 * input.Density * input.HeatCapacity;

            var stiffnessMatrix = new StiffnessMatrix(input.ConductingFactorX, domainParams, nodeMap);
            stiffnessMatrix.GlobalStiffnessMatrix.Dump("GLOBAL STIFFNESS MATRIX:", 1, domainParams.NumberOfNodes + 1);

            #region ALFA_MATRIXES

            var alfa12 = new decimal[5, 5];
            alfa12[1, 1] = 2;
            alfa12[1, 2] = 1;
            alfa12[2, 2] = 2;
            alfa12[2, 1] = 1;
            alfa12.Dump("ALFA 12:", 1, 5);

            var alfa23 = new decimal[5, 5];
            alfa23[2, 2] = 2;
            alfa23[2, 3] = 1;
            alfa23[3, 2] = 1;
            alfa23[3, 3] = 2;
            alfa23.Dump("ALFA 23:", 1, 5);

            var alfa34 = new decimal[5, 5];
            alfa34[3, 3] = 2;
            alfa34[3, 4] = 1;
            alfa34[4, 3] = 1;
            alfa34[4, 4] = 2;
            alfa34.Dump("ALFA 34:", 1, 5);

            var alfa41 = new decimal[5, 5];
            alfa41[1, 1] = 2;
            alfa41[1, 4] = 1;
            alfa41[4, 1] = 1;
            alfa41[4, 4] = 2;
            alfa41.Dump("ALFA 41:", 1, 5);

            #endregion ALFA_MATRIXES

            var globalAlpha = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
            var clipsDown = new int[] { 1, 2, 19, 20 };
            var clipsUp = new int[] { 21, 22, 39, 40 };

            for (int elementId = 1; elementId <= domainParams.NumberOfElements; elementId++)
            {
                decimal[,] localAlfa = new decimal[5, 5];
                decimal delta = 0;

                var element = nodeMap.GetElementById(elementId);

#if TEST_DATA
    if (elementId == 1 || elementId == 2)              //boundary conditions, bottom TEST
    {
        //heat exchange - local numeration 1-2 side
        delta = (input.HeatExchangingFactor1 * (element.Node2.PosX - element.Node1.PosX)) / 6;
        localAlfa = alfa12;
    }
#else
                if (clipsDown.Contains(elementId))              //boundary conditions, bottom, short
                {
                    //heat exchange - local numeration 1-2 side
                    delta = (input.HeatExchangingFactor1 * (element.Node2.PosX - element.Node1.PosX)) / 6;
                    localAlfa = alfa12;
                }
                else if (clipsUp.Contains(elementId))         //boundary conditions, top short             
                {
                    //heat exchange - local numeration 3-4 side
                    delta = (input.HeatExchangingFactor1 * (element.Node2.PosX - element.Node1.PosX)) / 6;
                    localAlfa = alfa34;
                }
                else if (elementId > 22)         //boundary conditions, top long             
                {
                    //heat exchange - local numeration 3-4 side
                    delta = (input.HeatExchangingFactor2 * (element.Node2.PosX - element.Node1.PosX)) / 6;
                    localAlfa = alfa34;
                }
                else              //boundary conditions, bottom, long
                {
                    //heat exchange - local numeration 1-2 side
                    delta = (input.HeatExchangingFactor2 * (element.Node2.PosX - element.Node1.PosX)) / 6;
                    localAlfa = alfa12;
                }
#endif

                for (int i = 1; i <= 4; i++)
                {
                    int ii = nodeMap.GetNodeByLocalAddress(elementId, i).GlobalId;
                    for (int j = 1; j <= 4; j++)
                    {
                        int jj = nodeMap.GetNodeByLocalAddress(elementId, i).GlobalId;

                        var alpha = localAlfa[i, j];
                        if (jj > 0 && alpha != 0)
                            globalAlpha[ii, jj] = globalAlpha[ii, jj] + delta * alpha;
                    }
                }
            }

            globalAlpha.Dump("ALPHA MATRIX:", 1, domainParams.NumberOfNodes + 1);

            KSum = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
            for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
            {
                for (int j = 1; j < domainParams.NumberOfNodes + 1; j++)
                {
                    KSum[i, j] = stiffnessMatrix.GlobalStiffnessMatrix[i, j] + globalAlpha[i, j];
                }
            }

            KSum.Dump("Main Global K matrix:", 1, domainParams.NumberOfNodes + 1);


            var FQFactory = new FQFactory(domainParams, nodeMap);
            var fQ = FQFactory.Generate(input.HeatSourcePower);

            IFAlphaFactory? falphaFactory = null;
#if TEST_DATA
falphaFactory = new FAlphaFactoryForTest(domainParams,nodeMap);
#else
            falphaFactory = new FAlphaFactory(domainParams, nodeMap);
#endif

            var falpha = falphaFactory.Generate(input.TemperatureTop, input.TemperatureBottom, clipsUp, clipsDown,
                input.HeatExchangingFactor1, input.HeatExchangingFactor2);

            var fqFactory = new FQSmallFactory(domainParams, nodeMap);
            var fq = fqFactory.Generate(input.HeatStream);


            // f vector
            f = new decimal[251, 2];
            for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
            {
                f[i, 1] = fq[i, 1] + fQ[i, 1] + falpha[i, 1];
            }
            f.DumpVector("F VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);

#if SHOULD_GAUSS
            var initialValues = new Dictionary<int, decimal> { };
            var a = new AMatrix(initialValues, KSum, f, domainParams);
            var gaussResult = GaussSolver.Solve(domainParams.NumberOfNodes, a);

#endif

#if TEST_DATA
initialValues = new Dictionary<int, decimal> { { 3, 100m }, { 6, 100m }, { 9, 100m } };
#endif

#if VERTICAL_GAUSS
            Console.WriteLine("\nGAUSS RESULTS:");
            for (int j = 0; j < domainParams.HorizontalElementsQuantity; j++)
            {
                int firstElement = j;
                int secondElement = j + domainParams.HorizontalElementsQuantity;
                int thirdElement = j + 2 * domainParams.HorizontalElementsQuantity;

                Console.WriteLine($"{(firstElement + 1):D2}={gaussResult[firstElement]:F4}\t" +
                    $"{(secondElement + +1):D2}={gaussResult[secondElement]:F4}\t" +
                    $"{(thirdElement + 1):D2}={gaussResult[thirdElement]:F4}\t");
            }
#endif

            var heatCapacityMatixFactory = new HeatCapacityMatrixFactory(input.HeatCapacity, input.Density, domainParams, nodeMap);

            var heatMatrix = heatCapacityMatixFactory.GenerateGlobalMatrix();

            heatMatrix = heatMatrix.RemoveColumn(0);
            heatMatrix = heatMatrix.RemoveRow(0);
            var invHeatMatrix = heatMatrix.Inverse();

            var resizedInvHeatMatrix = new decimal[64, 64];

            for (int i = 1; i < 64; i++)
            {
                for (int j = 1; j < 64; j++)
                {
                    resizedInvHeatMatrix[i, j] = invHeatMatrix[i - 1, j - 1];
                }
            }

            resizedInvHeatMatrix.Dump("INVERSED HEAT CAPACITY MATRIX:", 1, domainParams.NumberOfNodes + 1);
            
            //beggining T Value
            var t0 = new decimal[64, 2];
            t0.Set(100m);

            solver = new CalculusSolver(domainParams, resizedInvHeatMatrix, KSum, t0);
            currentHeatSourceGenerator = new CurrentHeatSource(domainParams);
            fiQFactory = new FiQFactory(domainParams, nodeMap);

            return ResultStringifier.GetString(solver.Result,domainParams,nodeMap);
        }

        public void CalculateStep(decimal dt_time, int step)
        {
            var heatSource = currentHeatSourceGenerator.GetHeatSourceValues(dt_time * step, solver.Result);
            var fiq = fiQFactory.Generate(heatSource);
            var fsum = new decimal[domainParams.NumberOfNodes + 1, 2];

            for (int i = 1; i <= domainParams.NumberOfNodes; i++)
            {
                fsum[i, 1] = f[i, 1] + fiq[i, 1];
            }

            solver.SolveStep(dt_time, fsum);
        }
        public decimal[,] GetStepData() => solver.Result;
    }
}
