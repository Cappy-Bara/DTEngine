using DTEngine.Contracts;
using Newtonsoft.Json;
using System.Text;

namespace DTEngine.Helpers
{
    public static class FileHandler
    {
        public static InputData ReadFromFile(string inputFileLocation)
        {
            var bytes = File.ReadAllBytes(inputFileLocation);
            var decoded = Encoding.ASCII.GetString(bytes);
            return JsonConvert.DeserializeObject<InputData>(decoded);
        }
    }
}
