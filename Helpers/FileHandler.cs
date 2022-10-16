using DTEngine.Contracts;
using Newtonsoft.Json;
using System.Text;

namespace DTEngine.Helpers
{
    public static class FileHandler
    {
        public static void WriteToFile(string outputFileLocation, OutputData outputData)
        {
            var serialized = JsonConvert.SerializeObject(outputData);
            var data = Encoding.ASCII.GetBytes(serialized);
            var filestream = File.Open(outputFileLocation, FileMode.Create);
            filestream.Write(data);
            filestream.Close();
        }

        public static InputData ReadFromFile(string inputFileLocation)
        {
            var bytes = File.ReadAllBytes(inputFileLocation);
            var decoded = Encoding.ASCII.GetString(bytes);
            return JsonConvert.DeserializeObject<InputData>(decoded);
        }
    }
}
