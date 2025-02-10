namespace WmsHub.AzureFunctions.Services;
public interface IFunctionService
{
  Task<string> ProcessAsync();
}
