


using Data.ThirdPartyModels;

namespace ServerLibrary.Services.Interfaces
{
    public interface IImportLogger
    {
        Task<string> SaveImportErrorsToFile<T>(List<ImportError<T>> errors, string fileName);
    }

}