using GymClient.Models;

namespace GymClient.Interfaces
{
    public interface IExportService
    {
        void ExportToPdf(IEnumerable<VisitDto> visits);
        void ExportToExcel(IEnumerable<VisitDto> visits);
    }
}
