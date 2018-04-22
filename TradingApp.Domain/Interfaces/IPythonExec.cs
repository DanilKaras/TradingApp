namespace TradingApp.Domain.Interfaces
{
    public interface IPythonExec
    {
        void RunPython(string path, int periods,  bool seasonalityHourly, bool seasonalityDaily);
    }
}