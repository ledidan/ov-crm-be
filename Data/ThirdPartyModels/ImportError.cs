
namespace Data.ThirdPartyModels
{
    public class ImportError<T>
    {
        public bool Flag { get; set; } = false;
        public int Row { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}