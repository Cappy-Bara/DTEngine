//#define VERBOSE

using DTEngine.Entities;
using DTEngine.Entities.ComputingDomain;
using DTEngine.Entities.Gauss;
using DTEngine.Helpers;
using DTEngine.Utilities;
using System.Security.Cryptography.X509Certificates;

string InputFileLocation = "C:\\Repos\\DTEngine\\InputFileFull.json";
string OutputFileLocation = "C:\\Repos\\DTEngine\\OutputFile.json";

var input = FileHandler.ReadFromFile(InputFileLocation);
var domainParams = new ComputationalDomainParams(input);

#if VERBOSE
Console.WriteLine("Grid params:");
Console.WriteLine($"Number of elements: {domainParams.NumberOfElements}");
Console.WriteLine($"Number of nodes: {domainParams.NumberOfNodes}");
#endif

var nodeMap = new NodeMap(domainParams);
var stiffnessMatrix = new StiffnessMatrix(input.ConductingFactorX, domainParams, nodeMap);

#if VERBOSE
Console.WriteLine("\nLocal stiffness matrix");
PrintMatrix(stiffnessMatrix.LocalStiffnessMatrix, 5);

Console.WriteLine("\nGlobal stiffness matrix");
PrintMatrix(stiffnessMatrix.GlobalStiffnessMatrix, domainParams.NumberOfNodes+1);
#endif

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

for (int elementId = 1; elementId <= domainParams.NumberOfElements; elementId++)
{
    decimal[,] localAlfa = new decimal[5, 5];
    decimal delta = 0;

    var element = nodeMap.GetElementById(elementId);

    if (false)      //boundary conditions, right side
    {
        //heat exchange - local numeration 2-3 side
        delta = (input.HeatExchangingFactor2 * (element.Node4.PosY - element.Node1.PosY)) / 6;
        localAlfa = alfa23;
    }

    if (false)             //boundary conditions, left side        
    {
        //heat exchange - local numeration 4-1 side
        delta = (input.HeatExchangingFactor4 * (element.Node4.PosY - element.Node1.PosY)) / 6;
        localAlfa = alfa41;
    }

    if ((elementId > 2 && elementId <= 18))              //boundary conditions, bottom, long
    {
        //heat exchange - local numeration 1-2 side
        delta = (input.HeatExchangingFactor2 * (element.Node2.PosX - element.Node1.PosX)) / 6;
        localAlfa = alfa12;
    }
    else if ((elementId <= 2 || elementId > 18) && elementId < 20)              //boundary conditions, bottom, short
    {
        //heat exchange - local numeration 1-2 side
        delta = (input.HeatExchangingFactor1 * (element.Node2.PosX - element.Node1.PosX)) / 6;
        localAlfa = alfa12;
    }
    else if ((elementId > 22 && elementId < 39))         //boundary conditions, top long             
    {
        //heat exchange - local numeration 3-4 side
        delta = (input.HeatExchangingFactor2 * (element.Node2.PosX - element.Node1.PosX)) / 6;
        localAlfa = alfa34;
    }
    else if ((elementId < 22 || elementId >= 39))         //boundary conditions, top short             
    {
        //heat exchange - local numeration 3-4 side
        delta = (input.HeatExchangingFactor1 * (element.Node2.PosX - element.Node1.PosX)) / 6;
        localAlfa = alfa34;
    }

    for (int i = 1; i <= 4; i++)          //building global Kalpha
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

#if VERBOSE
Console.WriteLine("\nAlpha matrix:");
PrintMatrix(globalAlpha, domainParams.NumberOfNodes + 1);
#endif


var KSum = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
{
    for (int j = 1; j < domainParams.NumberOfNodes + 1; j++)
    {
        KSum[i, j] = stiffnessMatrix.GlobalStiffnessMatrix[i, j] + globalAlpha[i, j];
    }
}

#if VERBOSE
Console.WriteLine("\nMain Global K matrix:");
PrintMatrix(KSum, domainParams.NumberOfNodes + 1);
#endif

var fQ = new decimal[251, 2];
var fQElement = new decimal[5, 2];
decimal fQValue;

for (int element = 1; element <= domainParams.NumberOfElements; element++)
{
    var xe = new decimal[5];
    var ye = new decimal[5];

    for (int j = 1; j <= 4; j++)                                //TO REMOVE?
    {
        xe[j] = nodeMap.GetNodeByLocalAddress(element, j).PosX;
        ye[j] = nodeMap.GetNodeByLocalAddress(element, j).PosY;
    }

    fQValue = ((input.HeatSourcePower) * ((xe[2] - xe[1]) * (ye[4] - ye[1]))) / 4.0m;
    fQElement[1, 1] = fQValue * 1m; fQElement[2, 1] = fQValue * 1m;
    fQElement[3, 1] = fQValue * 1m; fQElement[4, 1] = fQValue * 1m;
    
    for (int i = 1; i <= 4; i++)
    {
        var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
        if (ii > 0)
            fQ[ii, 1] = fQ[ii, 1] + fQElement[i, 1];
    }
}

#if VERBOSE
Console.WriteLine("\nfQ vector:");
PrintVector(fQ, 9);
#endif

//falfa

decimal falfaValue;
var falfa_e = new decimal[5, 2];
var falfa = new decimal[251, 2];

for (int element = 1; element <= domainParams.NumberOfElements; element++)
{
    var xe = new decimal[5];
    var ye = new decimal[5];

    for (int j = 1; j <= 4; j++)                                            //to remove
    {
        xe[j] = nodeMap.GetNodeByLocalAddress(element, j).PosX;
        ye[j] = nodeMap.GetNodeByLocalAddress(element, j).PosY;
    }

    if (false)   //boundary conditions right
    {
        falfaValue = ((input.HeatExchangingFactor2 * input.TemperatureRight) * (ye[4] - ye[1])) / 2;
        falfa_e[1, 1] = 0;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = falfaValue * 1;
        falfa_e[4, 1] = 0;
    }
    if (false)       //boundary conditions left     
    {
        falfaValue = ((input.HeatExchangingFactor4 * input.TemperatureLeft) * (ye[4] - ye[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = 0;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = falfaValue * 1;
    }
    if (element < 3 || (element > 18 && element < 21))       //boundary conditions bottom       
    {
        falfaValue = ((input.HeatExchangingFactor1 * input.TemperatureBottom) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = 0;
    }
    else if ((element < 23 && element > 20) || element < 39)       //boundary conditions top        
    {
        falfaValue = ((input.HeatExchangingFactor1 * input.TemperatureTop) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = 0;
        falfa_e[2, 1] = 0;
        falfa_e[3, 1] = falfaValue * 1;
        falfa_e[4, 1] = falfaValue * 1;
    }
    else if (element > 2 && element < 18)       //boundary conditions bottom       
    {
        falfaValue = ((input.HeatExchangingFactor2 * input.TemperatureBottom) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = 0;
    }
    else if (element > 22 && element < 39)       //boundary conditions top        
    {
        falfaValue = ((input.HeatExchangingFactor2 * input.TemperatureTop) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = 0;
        falfa_e[2, 1] = 0;
        falfa_e[3, 1] = falfaValue * 1;
        falfa_e[4, 1] = falfaValue * 1;
    }
    if (false)       //other elements
    {
        falfa_e[1, 1] = 0; falfa_e[2, 1] = 0;
        falfa_e[3, 1] = 0; falfa_e[4, 1] = 0;
    }
    for (int i = 1; i <= 4; i++)          //falfa building
    {
        var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
        if (ii > 0)
            falfa[ii, 1] = falfa[ii, 1] + falfa_e[i, 1];
    }
}

#if VERBOSE
Console.WriteLine("\nFAlfa vector:");
PrintVector(falfa, 9);
#endif

//fq
decimal fq_value;
var fq_e = new decimal[5, 2];
var fq = new decimal[251, 2];

for (int element = 1; element <= domainParams.NumberOfElements; element++)          //to one loop
{
    var xe = new decimal[5];
    var ye = new decimal[5];

    for (int j = 1; j <= 4; j++)            //to remove
    {
        xe[j] = nodeMap.GetNodeByLocalAddress(element, j).PosX;
        ye[j] = nodeMap.GetNodeByLocalAddress(element, j).PosY;
    }

    if (false)   //boundary conditions right
    {
        fq_value = (input.HeatStream * (ye[4] - ye[1])) / 2;
        fq_e[1, 1] = 0;
        fq_e[2, 1] = fq_value * 1;
        fq_e[3, 1] = fq_value * 1;
        fq_e[4, 1] = 0;
    }
    if (element == 1 || element == 3)       //boundary conditions left     
    {
        fq_value = (input.HeatStream * (ye[4] - ye[1])) / 2;
        fq_e[1, 1] = fq_value * 1;
        fq_e[2, 1] = 0;
        fq_e[3, 1] = 0;
        fq_e[4, 1] = fq_value * 1;
    }
    if (false)       //boundary conditions bottom       
    {
        fq_value = (input.HeatStream * (xe[2] - xe[1])) / 2;
        fq_e[1, 1] = fq_value * 1;
        fq_e[2, 1] = fq_value * 1;
        fq_e[3, 1] = 0;
        fq_e[4, 1] = 0;
    }
    if (false)       //boundary conditions top        
    {
        fq_value = (input.HeatStream * (xe[2] - xe[1])) / 2;
        fq_e[1, 1] = 0;
        fq_e[2, 1] = 0;
        fq_e[3, 1] = fq_value * 1;
        fq_e[4, 1] = fq_value * 1;
    }
    if (element == 2 || element == 4)       //other elements
    {
        fq_e[1, 1] = 0; fq_e[2, 1] = 0;
        fq_e[3, 1] = 0; fq_e[4, 1] = 0;
    }
    for (int i = 1; i <= 4; i++)          //falfa building
    {
        var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
        if (ii > 0)
            fq[ii, 1] = fq[ii, 1] + fq_e[i, 1];
    }
}

#if VERBOSE
Console.WriteLine("\nfq vector:");
PrintVector(fq, domainParams.NumberOfNodes);
#endif


// f vector
var f = new decimal[251, 2];
for (int i = 1; i < domainParams.NumberOfNodes + 1; i++)
{
    f[i, 1] = fq[i,1] + fQ[i,1] + falfa[i,1];
}

#if VERBOSE
Console.WriteLine("\nf vector:");
PrintVector(f, domainParams.NumberOfNodes);
#endif

//var initialValues = new Dictionary<int, decimal> { {3, 100m}, { 6, 100m }, { 9, 100m } };
var initialValues = new Dictionary<int, decimal> { };
var a = new AMatrix(initialValues, KSum, f, domainParams);

#if VERBOSE
Console.WriteLine("\nA Matrix:");
PrintMatrixWithSizes(a, domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes, 1);
#endif

var gaussResult = GaussSolver.Solve(domainParams.NumberOfNodes, a);

Console.WriteLine("\nGAUSS RESULTS:");
for (int i = domainParams.VerticalElementsQuantity-1; i >= 0; i--)
{
    for (int j = i* domainParams.HorizontalElementsQuantity; j < (i+1) * domainParams.HorizontalElementsQuantity; j++)
    {
        Console.Write($"{(j + 1):D2}={gaussResult[i]:F4} ");
    }
    Console.Write($"\n");
}



static void PrintMatrix<T>(T[,] matrix, int size)
{
    for (int i = 1; i < size; i++)
    {
        for (int j = 1; j < size; j++)
        {
            Console.Write($"\t{matrix[i, j]} ");
        }
        Console.WriteLine();
    }
}

static void PrintMatrixNew<T>(IMatrix<T> matrix, int size, int fromIndex = 1)
{
    for (int i = fromIndex; i < size; i++)
    {
        for (int j = fromIndex; j < size; j++)
        {
            Console.Write($"\t{matrix[i, j]} ");
        }
        Console.WriteLine();
    }
}

static void PrintMatrixWithSizes<T>(IMatrix<T> matrix, int RowSize, int ColumnSize, int fromIndex = 1)
{
    for (int i = fromIndex; i < ColumnSize; i++)
    {
        for (int j = fromIndex; j < RowSize; j++)
        {
            Console.Write($"\t{matrix[i, j]} ");
        }
        Console.WriteLine();
    }
}

static void PrintVector<T>(T[,] matrix, int size)
{
    for (int j = 1; j <= size; j++)
    {
        Console.Write($"\t{matrix[j, 1]} \n");
    }
}