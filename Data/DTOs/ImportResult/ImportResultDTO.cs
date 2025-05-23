

using Data.ThirdPartyModels;

namespace Data.DTOs
{
    public class ImportResultDto<T>
    {
        public List<T> Added { get; set; } = new List<T>();
        public List<T> Updated { get; set; } = new List<T>();
        public List<ImportError<T>> Errors { get; set; } = new List<ImportError<T>>();

        // public string? ErrorLogUrl { get; set; }
    }
}