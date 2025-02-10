using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Factories;
public interface IProcessFactory
{
  IProcess Create();
}