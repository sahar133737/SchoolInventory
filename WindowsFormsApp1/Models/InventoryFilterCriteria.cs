using System;

namespace WindowsFormsApp1.Models
{
    public class InventoryFilterCriteria
    {
        public string SearchText { get; set; }
        public int? CategoryId { get; set; }
        public int? ClassroomId { get; set; }
        public int? ResponsiblePersonId { get; set; }
        public string CurrentState { get; set; }
        public string Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchText) ||
            CategoryId.HasValue ||
            ClassroomId.HasValue ||
            ResponsiblePersonId.HasValue ||
            !string.IsNullOrWhiteSpace(CurrentState) ||
            !string.IsNullOrWhiteSpace(Status) ||
            DateFrom.HasValue ||
            DateTo.HasValue ||
            PriceFrom.HasValue ||
            PriceTo.HasValue;
    }
}
