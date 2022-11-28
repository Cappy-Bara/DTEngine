//#define VERBOSE
//#define TEST_DATA
//#define HORIZONTAL_GAUSS
#define VERTICAL_GAUSS
//#define SINGLE_GAUSS

using Accord.Math;
using DTEngine.Entities;
using DTEngine.Entities.ComputingDomain;
using DTEngine.Entities.Gauss;
using DTEngine.Helpers;
using DTEngine.Utilities;

string InputFileLocation = "C:\\Repos\\DTEngine\\InputFileFull.json";
#if TEST_DATA
InputFileLocation = "C:\\Repos\\DTEngine\\InputFileTest.json";
#endif


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
var clipsDown = new int[] { 1, 2, 19, 20 };
var clipsUp = new int[] { 21, 22, 39, 40 };

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

    for (int i = 1; i <= 4; i++)          //building global Kalpha          //TU ŹLE?
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

#if TEST_DATA
    if (element == 1 || element == 2)       //boundary conditions bottom       
    {
        falfaValue = ((input.HeatExchangingFactor1 * input.TemperatureBottom) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = 0;
    }
    if (element == 3 || element == 4)       //other elements    TU MOŻE PROBLEM? NIE.
    {
        falfa_e[1, 1] = 0; falfa_e[2, 1] = 0;
        falfa_e[3, 1] = 0; falfa_e[4, 1] = 0;
    }
#else
    if (clipsDown.Contains(element))       //boundary conditions bottom       
    {
        falfaValue = ((input.HeatExchangingFactor1 * input.TemperatureBottom) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = 0;
    }
    else if (clipsUp.Contains(element))       //boundary conditions top        
    {
        falfaValue = ((input.HeatExchangingFactor1 * input.TemperatureTop) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = 0;
        falfa_e[2, 1] = 0;
        falfa_e[3, 1] = falfaValue * 1;
        falfa_e[4, 1] = falfaValue * 1;
    }
    else if (element > 22)       //boundary conditions top        
    {
        falfaValue = ((input.HeatExchangingFactor2 * input.TemperatureTop) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = 0;
        falfa_e[2, 1] = 0;
        falfa_e[3, 1] = falfaValue * 1;
        falfa_e[4, 1] = falfaValue * 1;
    }
    else       //boundary conditions bottom       
    {
        falfaValue = ((input.HeatExchangingFactor2 * input.TemperatureBottom) * (xe[2] - xe[1])) / 2;
        falfa_e[1, 1] = falfaValue * 1;
        falfa_e[2, 1] = falfaValue * 1;
        falfa_e[3, 1] = 0;
        falfa_e[4, 1] = 0;
    }
#endif

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

if(input.HeatStream != 0.0m)
{
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

var initialValues = new Dictionary<int, decimal> { };
#if TEST_DATA
initialValues = new Dictionary<int, decimal> { { 3, 100m }, { 6, 100m }, { 9, 100m } };
#endif

var a = new AMatrix(initialValues, KSum, f, domainParams);

#if VERBOSE
Console.WriteLine("\nA Matrix:");
PrintMatrixWithSizes(a, domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes, 1);
#endif

var gaussResult = GaussSolver.Solve(domainParams.NumberOfNodes, a);


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

var heatCapacityMatrix = new decimal[4, 4];
var heatCapacityFactor = input.HeatCapacity * input.Density * input.Width * input.Height;
var heatCapacityMatixNew = new HeatCapacityMatrix(input.HeatCapacity, input.Density, input.Width,input.Height, domainParams,nodeMap);

var baseHeatCapacityMatrix = new decimal[,] { {0,0,0,0,0 }, {0, 4, 2, 1, 2 }, {0, 2, 4, 2, 1 }, { 0, 1, 2, 4, 2 }, {0,2, 1, 2, 4 }};

#if VERBOSE
Console.WriteLine("\nBASE HEAT CAPACITY MATRIX:");
PrintMatrix(baseHeatCapacityMatrix, 5);
#endif

for (int i = 0; i < 4; i++)
{
    for (int j = 0; j < 4; j++)
    {
        heatCapacityMatrix[i,j] = baseHeatCapacityMatrix[i+1,j+1] * heatCapacityFactor;
    }
}

//#if VERBOSE
Console.WriteLine("\nHEAT CAPACITY MATRIX:");
PrintMatrix(heatCapacityMatrix, 4, 0);
//#endif


//var invHeatMatrix = heatCapacityMatrix.Inverse();
///
var invHeatMatrix = heatCapacityMatixNew.GenerateGlobalMatrix();
invHeatMatrix = invHeatMatrix.RemoveColumn(0);
invHeatMatrix = invHeatMatrix.RemoveRow(0);
invHeatMatrix = invHeatMatrix.Inverse();
///


var invHeatMatrix2 = new decimal[64, 64];

for (int i = 1; i < 64; i++)
{
    for (int j = 1; j < 64; j++)
    {
        invHeatMatrix2[i, j] = invHeatMatrix[i-1,j-1];
    }
}



//var invHeatMatrix = MatrixInverter.Invert(heatCapacityMatrix, 4);

#if VERBOSE
Console.WriteLine("\nINVERSED HEAT CAPACITY MATRIX:");
PrintMatrix(invHeatMatrix, 63, 0);
#endif



// TIME INTEGRATION
//węzły? - 63
var nw = 63;
var m1 = new decimal[64,2]; 
var m2 = new decimal[64,2]; 
var m3 = new decimal[64,2]; 
var m4 = new decimal[64,2]; 
var m5 = new decimal[64,2];
var dt_time = 0.005m;                                     
var t = new decimal[64,2];

//beggining T Value
for (int i = 1; i <= nw; i++)
{
    t[i, 1] = 20m;
}


for (int step = 0; step < 100000; step++)
{

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

}


Console.WriteLine("\nTime vector");
PrintVector(t, 63);


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