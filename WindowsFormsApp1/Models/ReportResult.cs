using System.Data;

namespace WindowsFormsApp1.Models
{
    public class ReportResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DataTable Data { get; set; }
        public bool HasRowTypeColumn { get; set; }
    }

    public enum ReportKind
    {
        CategoryFinancialSummary = 1,
        ClassroomResponsibleSummary = 2,
        StateCategoryMatrix = 3,
        DetailedRegistryWithSubtotals = 4
    }
}
