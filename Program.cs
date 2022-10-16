using DTEngine.Contracts;
using DTEngine.Helpers;

string InputFileLocation = "C:\\Repos\\DTEngine\\DTEngine\\InputFile.json";
string OutputFileLocation = "C:\\Repos\\DTEngine\\DTEngine\\OutputFile.json";

var input = FileHandler.ReadFromFile(InputFileLocation);


float dx = 0;
float dy = 0;

var dv = input.Height / (input.HorizontalElementsQuantity - 1);
var dh = input.Width / (input.VerticalElementsQuantity - 1);
var nw = input.HorizontalElementsQuantity * input.VerticalElementsQuantity;
var ne = (input.HorizontalElementsQuantity - 1) * (input.VerticalElementsQuantity - 1);

Console.WriteLine("PARAMETRY SIATKI:");
Console.WriteLine($"Ilość elementów: {ne}");
Console.WriteLine($"Ilość węzłów: {nw}");
Console.WriteLine("Naciśnij dowolny przycisk by zacząć. \n");

Console.ReadKey();


//Grid coordinates
//Possible real phisical coordinates
var wx = new float[1000];
var wy = new float[1000];

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
int licznik = 1;
int l = 0;
float[] x = new float[1000];
float[] y = new float[1000];

for (int i = 1; i <= nw; i++)
{
    x[i] = wx[licznik];
    l++;
    if (l == input.HorizontalElementsQuantity)
    {
        licznik++;
        l = 0;
    }
}

licznik = 1;
for (int i = 1; i <= nw; i++)
{
    y[i] = wy[licznik];
    licznik++;
    if (licznik == input.HorizontalElementsQuantity + 1)
    {
        licznik = 1;
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

FileHandler.WriteToFile(OutputFileLocation, new OutputData(ie, ne));