//#define VERBOSE
//#define TEST_DATA
//#define SHOULD_GAUSS
//#define HORIZONTAL_GAUSS
//#define VERTICAL_GAUSS
//#define SINGLE_GAUSS

using Accord.Math;
using DTEngine.Entities;
using DTEngine.Entities.ComputingDomain;
using DTEngine.Entities.FVector;
using DTEngine.Entities.FVector.FAlpha;
using DTEngine.Helpers;

string InputFileLocation = "C:\\Repos\\DTEngine\\InputFileFull.json";
#if TEST_DATA
InputFileLocation = "C:\\Repos\\DTEngine\\InputFileTest.json";
#endif


var input = FileHandler.ReadFromFile(InputFileLocation);
var domainParams = new ComputationalDomainParams(input);

#if VERBOSE
Console.WriteLine("Grid params:");
Console.WriteLine($"Number of elements: {domainParams.NumberOfElements}");
Console.WriteLine($"Number of nodes: {domainParams.NumberOfNodes}");
#endif

var nodeMap = new NodeMap(domainParams);

input.HeatSourcePower = 20 * input.Density * input.HeatCapacity;

var stiffnessMatrix = new StiffnessMatrix(input.ConductingFactorX, domainParams, nodeMap);
stiffnessMatrix.GlobalStiffnessMatrix.Dump("GLOBAL STIFFNESS MATRIX:", 1, domainParams.NumberOfNodes + 1);

#region ALFA_MATRIXES

var alfa12 = new decimal[5, 5];
alfa12[1, 1] =  2;
alfa12[1, 2] =  1;
alfa12[2, 2] = 2;
alfa12[2, 1] = 1;

#if VERBOSE
Console.WriteLine("\nAlfa 12");
PrintMatrix(alfa12, 5);
#endif

var alfa23 = new decimal[5, 5];
alfa23[2, 2] = 2;
alfa23[2, 3] = 1;
alfa23[3, 2] = 1;
alfa23[3, 3] = 2;

#if VERBOSE
Console.WriteLine("\nAlfa 23");
PrintMatrix(alfa23, 5);
#endif

var alfa34 = new decimal[5, 5];
alfa34[3, 3] = 2;
alfa34[3, 4] = 1;
alfa34[4, 3] = 1;
alfa34[4, 4] = 2;

#if VERBOSE
Console.WriteLine("\nAlfa 34");
PrintMatrix(alfa34, 5);
#endif

var alfa41 = new decimal[5, 5];
alfa41[1, 1] = 2;
alfa41[1, 4] = 1;
alfa41[4, 1] = 1;
alfa41[4, 4] = 2;

#if VERBOSE
Console.WriteLine("\nAlfa 41");
PrintMatrix(alfa41, 5);
#endif

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

globalAlpha.Dump("ALPHA MATRIX:",1, domainParams.NumberOfNodes + 1);

var KSum = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
{
    for (int j = 1; j < domainParams.NumberOfNodes + 1; j++)
    {
        KSum[i, j] = stiffnessMatrix.GlobalStiffnessMatrix[i, j] + globalAlpha[i, j];
    }
}

KSum.Dump("Main Global K matrix:", 1, domainParams.NumberOfNodes + 1);


var FQFactory = new FQFactory(domainParams,nodeMap);
var fQ = FQFactory.Generate(input.HeatSourcePower);

IFAlphaFactory? falphaFactory = null;
#if TEST_DATA
falphaFactory = new FAlphaFactoryForTest(domainParams,nodeMap);
#else
falphaFactory = new FAlphaFactory(domainParams,nodeMap);
#endif

var falpha = falphaFactory.Generate(input.TemperatureTop,input.TemperatureBottom, clipsUp, clipsDown,
    input.HeatExchangingFactor1, input.HeatExchangingFactor2);

var fqFactory = new FQSmallFactory(domainParams,nodeMap);
var fq = fqFactory.Generate(input.HeatStream);


// f vector
var f = new decimal[251, 2];
for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
{
    f[i, 1] = fq[i,1] + fQ[i,1] + falpha[i,1];
}
f.DumpVector("F VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);

#if SHOULD_GAUSS
var initialValues = new Dictionary<int, decimal> { };
var a = new AMatrix(initialValues, KSum, f, domainParams);
var gaussResult = GaussSolver.Solve(domainParams.NumberOfNodes, a);


#if VERBOSE
Console.WriteLine("\nA Matrix:");
PrintMatrixWithSizes(a, domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes, 1);
#endif


#endif

#if TEST_DATA
initialValues = new Dictionary<int, decimal> { { 3, 100m }, { 6, 100m }, { 9, 100m } };
#endif



#if HORIZONTAL_GAUSS
Console.WriteLine("\nGAUSS RESULTS:");
for (int i = domainParams.VerticalElementsQuantity - 1; i >= 0; i--)
{
    for (int j = i * domainParams.HorizontalElementsQuantity; j < (i + 1) * domainParams.HorizontalElementsQuantity; j++)
    {
        Console.Write($"{(j + 1):D2}={gaussResult[j]:F4} ");
    }
    Console.Write($"\n");
}
#endif
#if VERTICAL_GAUSS
Console.WriteLine("\nGAUSS RESULTS:");
for (int j = 0; j < domainParams.HorizontalElementsQuantity; j++)
{
    int firstElement = j;
    int secondElement = j + domainParams.HorizontalElementsQuantity;
    int thirdElement = j + 2*domainParams.HorizontalElementsQuantity;

    Console.WriteLine($"{(firstElement + 1):D2}={gaussResult[firstElement]:F4}\t" +
        $"{(secondElement +  + 1):D2}={gaussResult[secondElement]:F4}\t" +
        $"{(thirdElement + 1):D2}={gaussResult[thirdElement]:F4}\t");
}
#endif
#if SINGLE_GAUSS
Console.WriteLine("\nGAUSS RESULTS:");
for (int i = 0; i < domainParams.NumberOfNodes; i++)
{
    Console.WriteLine($"X[{(i + 1):D2}] = {gaussResult[i]:F4}");
}
#endif

var heatCapacityMatixFactory = new HeatCapacityMatrixFactory(input.HeatCapacity, input.Density, domainParams, nodeMap);


var heatMatrix = heatCapacityMatixFactory.GenerateGlobalMatrix();

heatMatrix = heatMatrix.RemoveColumn(0);
heatMatrix = heatMatrix.RemoveRow(0);
var invHeatMatrix = heatMatrix.Inverse();

var invHeatMatrix2 = new decimal[64, 64];

for (int i = 1; i < 64; i++)
{
    for (int j = 1; j < 64; j++)
    {
        invHeatMatrix2[i, j] = invHeatMatrix[i-1,j-1];
    }
}

invHeatMatrix2.Dump("INVERSED HEAT CAPACITY MATRIX:",1,domainParams.NumberOfNodes+1);


// TIME INTEGRATION
var nw = 63;
var m1 = new decimal[64,2]; 
var m2 = new decimal[64,2]; 
var m3 = new decimal[64,2]; 
var m4 = new decimal[64,2]; 
var m5 = new decimal[64,2];
var t = new decimal[64,2];

var dt_time = 0.005m;
var timeS = 60m;
int steps = (int)(timeS / dt_time);
int printTime = (int)(1.0m / dt_time);
int sec = 0;

//beggining T Value
for (int i = 1; i <= nw; i++)
{
    t[i, 1] = 20m;
}

#region calculus

for (int step = 0; step <= steps; step++)
{
    //1 operation - (m-1*f)
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            m1[k,j] = 0;
            for (int i = 1; i <= nw; i++)
            {
                m1[k,j] = m1[k,j] + (invHeatMatrix2[k,i] * f[i,j]);
            }
        }
    }
    //2 operation
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            m2[k,j] = 0;
            for (int i = 1; i <= nw; i++)
            {
                m2[k,j] = m2[k,j] + (KSum[k,i] * t[i,j]);
            }
        }
    }
    //3 operation
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            m3[k,j] = 0;
            for (int i = 1; i <= nw; i++)
            {
                m3[k,j] = m3[k,j] + (invHeatMatrix2[k,i] * m2[i,j]);
            }
        }
    }
    //4 operation
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            m4[k,j] = m1[k,j] - m3[k,j];
        }
    }
    //5 operation
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            m5[k,j] = m4[k,j] * dt_time;
        }
    }
    //6 operation
    for (int k = 1; k <= nw; k++)
    {
        for (int j = 1; j <= 1; j++)
        {
            t[k,j] = t[k,j] + m5[k,j];
        }
    }


    if (step % printTime == 0) { 
        
        Console.WriteLine($"\nTime vector    -   {sec}sec   -> step {step}");
        for (int j = 1; j <= domainParams.HorizontalElementsQuantity; j++)
        {
            int firstElement = j;
            int secondElement = j + domainParams.HorizontalElementsQuantity;
            int thirdElement = j + 2 * domainParams.HorizontalElementsQuantity;

            Console.WriteLine($"{firstElement:D2}={t[firstElement, 1]:F4}\t" +
                $"{secondElement:D2}={t[secondElement, 1]:F4}\t" +
                $"{thirdElement:D2}={t[thirdElement, 1]:F4}\t");
        }
        sec++;
    }
}

#endregion calculus



static void PrintMatrix<T>(T[,] matrix, int size, int from = 1)
{
    for (int i = from; i < size; i++)
    {
        for (int j = from; j < size; j++)
        {
            Console.Write($"\t{matrix[i, j]} ");
        }
        Console.WriteLine();
    }
}