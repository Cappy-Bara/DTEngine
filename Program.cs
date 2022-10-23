using DTEngine.Helpers;

string InputFileLocation = "C:\\Repos\\DTEngine\\InputFile.json";
string OutputFileLocation = "C:\\Repos\\DTEngine\\OutputFile.json";

var input = FileHandler.ReadFromFile(InputFileLocation);

decimal dx = 0;
decimal dy = 0;

decimal dv = input.Height / (input.HorizontalElementsQuantity - 1);
decimal dh = input.Width / (input.VerticalElementsQuantity - 1);
decimal nw = input.HorizontalElementsQuantity * input.VerticalElementsQuantity;
decimal ne = (input.HorizontalElementsQuantity - 1) * (input.VerticalElementsQuantity - 1);

Console.WriteLine("Grid params:");
Console.WriteLine($"Number of elements: {ne}");
Console.WriteLine($"Number of nodes: {nw}");

//Grid coordinates
//Possible real phisical coordinates
var wx = new decimal[1000];
var wy = new decimal[1000];

for (int i = 1; i <= input.VerticalElementsQuantity; i++)
{
    wx[i] = dx;
    dx += dh;
}
for (int j = 1; j <= input.HorizontalElementsQuantity; j++)
{
    wy[j] = dy;
    dy += dv;
}


//physical nodes coordinates.
decimal[] x = new decimal[1000];
decimal[] y = new decimal[1000];

int rowsCounter = 1;

for (int i = 1; i <= nw; i++)
{
    x[i] = wy[rowsCounter];
    rowsCounter++;
    if (rowsCounter == input.VerticalElementsQuantity + 1)
    {
        rowsCounter = 1;
    }
}

rowsCounter = 1;
int columnsCounter = 1;
for (int i = 1; i <= nw; i++)
{
    y[i] = wx[rowsCounter];
    columnsCounter++;
    if (columnsCounter == input.HorizontalElementsQuantity + 1)
    {
        rowsCounter++;
        columnsCounter = 1;
    }
}


//connection matrix
//x - node number
//y - local node index
//ie[x,y] - global node index

int[,] ie = new int[1000,5];
for (int verticalPosition=1,horizontalPosition=1,elementNumber=1; 
    elementNumber <= ne; 
    elementNumber++)
{
    ie[elementNumber, 1] = verticalPosition;
    ie[elementNumber, 2] = 1 + verticalPosition;
    ie[elementNumber, 3] = 1 + verticalPosition + input.HorizontalElementsQuantity;
    ie[elementNumber, 4] = verticalPosition + input.HorizontalElementsQuantity;

    horizontalPosition++;
    verticalPosition++;

    if (horizontalPosition == input.HorizontalElementsQuantity)
    {
        horizontalPosition = 1;
        verticalPosition++;
    }
}

var KLocal = new int[5,5];
var lambda = (int)input.ConductingFactorX/6;

#region LOCAL_K_MATRIX

KLocal[1,1] = lambda * 4;
KLocal[1,2] = lambda * -1;
KLocal[1,3] = lambda * -2;
KLocal[1,4] = lambda * -1;

KLocal[2, 1] = lambda * -1;
KLocal[2, 2] = lambda * 4;
KLocal[2, 3] = lambda * -1;
KLocal[2, 4] = lambda * -2;

KLocal[3, 1] = lambda * -2;
KLocal[3, 2] = lambda * -1;
KLocal[3, 3] = lambda * 4;
KLocal[3, 4] = lambda * -1;

KLocal[4, 1] = lambda * -1;
KLocal[4, 2] = lambda * -2;
KLocal[4, 3] = lambda * -1;
KLocal[4, 4] = lambda * 4;

#endregion LOCAL_K_MATRIX

//building global K matrix
var KGlobal = new int[10,10];
for (int element = 1; element <= ne; element++)
{
    for (int firstNodeLocal = 1; firstNodeLocal <= 4; firstNodeLocal++)
    {
        var firstNodeGlobal = ie[element, firstNodeLocal];
        for (int secondNodeLocal = 1; secondNodeLocal <= 4; secondNodeLocal++)
        {
            var secondNodeGlobal = ie[element, secondNodeLocal];
                
            if (secondNodeGlobal > 0)
                KGlobal[firstNodeGlobal, secondNodeGlobal] = KGlobal[firstNodeGlobal, secondNodeGlobal] + KLocal[firstNodeLocal,secondNodeLocal];
        }
    }
}


Console.WriteLine("\nLocal stiffness matrix");
PrintMatrix(KLocal, 5);


Console.WriteLine("\nGlobal stiffness matrix");
PrintMatrix(KGlobal, 10);

#region ALFA_MATRIXES

var alfa12 = new decimal[5, 5];
alfa12[1, 1] =  2;
alfa12[1, 2] =  1;
alfa12[2, 2] = 2;
alfa12[2, 1] = 1;
Console.WriteLine("\nAlfa 12");
PrintMatrix(alfa12, 5);

var alfa23 = new decimal[5, 5];
alfa23[2, 2] = 2;
alfa23[2, 3] = 1;
alfa23[3, 2] = 1;
alfa23[3, 3] = 2;
Console.WriteLine("\nAlfa 23");
PrintMatrix(alfa23, 5);

var alfa34 = new decimal[5, 5];
alfa34[3, 3] = 2;
alfa34[3, 4] = 1;
alfa34[4, 3] = 1;
alfa34[4, 4] = 2;
Console.WriteLine("\nAlfa 34");
PrintMatrix(alfa34, 5);

var alfa41 = new decimal[5, 5];
alfa41[1, 1] = 2;
alfa41[1, 4] = 1;
alfa41[4, 1] = 1;
alfa41[4, 4] = 2;
Console.WriteLine("\nAlfa 41");
PrintMatrix(alfa41, 5);

#endregion ALFA_MATRIXES

var globalAlpha = new decimal[10, 10];

for (int u = 1; u <= ne; u++)
{
    decimal[,] localAlfa = new decimal[5, 5];
    decimal delta = 0;

    if (false)      //boundary conditions, right side
    {
        //heat exchange - local numeration 2-3 side
        delta = (input.HeatExchangingFactor2 * (y[4] - y[1])) / 6;
        localAlfa = alfa23;
    }

    if (false)             //boundary conditions, left side        
    {
        //heat exchange - local numeration 4-1 side
        delta = (input.HeatExchangingFactor4 * (y[4] - y[1])) / 6;
        localAlfa = alfa41;
    }

    if (u == 1 || u == 2)              //boundary conditions, bottom
    {
        //heat exchange - local numeration 1-2 side
        delta = (input.HeatExchangingFactor1 * (x[2] - x[1])) / 6;
        localAlfa = alfa12;
    }

    if (false)         //boundary conditions, top             
    {
        //heat exchange - local numeration 3-4 side
        delta = (input.HeatExchangingFactor3 * (x[2] - x[1])) / 6;
        localAlfa = alfa34;
    }

    for (int i = 1; i <= 4; i++)          //building global Kalpha
    {
        int ii = ie[u, i];
        for (int j = 1; j <= 4; j++)
        {
            int jj = ie[u, j];

            var alpha = localAlfa[i, j];
            if (jj > 0 && alpha != 0)
                globalAlpha[ii, jj] = globalAlpha[ii, jj] + delta * alpha;
        }
    }
}

Console.WriteLine("\nAlpha matrix:");
PrintMatrix(globalAlpha, 10);

var KSum = new decimal[10, 10];
for (int i = 1; i < 10; i++)
{
    for (int j = 1; j < 10; j++)
    {
        KSum[i, j] = KGlobal[i, j] + globalAlpha[i, j];
    }
}

Console.WriteLine("\nMain Global K matrix:");
PrintMatrix(KSum, 10);

static void PrintMatrix<T>(T[,] matrix, int size)
{
    for (int i = 1; i < size; i++)
    {
        for (int j = 1; j < size; j++)
        {
            Console.Write($"\t{matrix[i,j]} ");
        }
        Console.WriteLine();
    }
}