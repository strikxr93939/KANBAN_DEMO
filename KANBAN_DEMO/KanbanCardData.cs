using System;

namespace канбанчик
{
    public class KanbanCardData
    {
        public string JobNumber { get; set; }
        public string Suffix { get; set; }
        public int Pages { get; set; }
        public string Item { get; set; }
        public string ItemDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Storage { get; set; }
        public decimal RealisedQuantity { get; set; }
        public decimal CompleteQuantity { get; set; }
        public string Lot { get; set; }
        public string Status { get; set; }
        public string KO { get; set; }
        public string Logo { get; set; }
        public string KU { get; set; }
        public string Tag { get; set; }
        public string Plata { get; set; }
        public string Folder { get; set; }
        public string Color { get; set; }
        public int FIFO { get; set; }
        public string TU { get; set; }
        public string VPOReason { get; set; }
        public string VPO { get; set; }
        public string PrintStatus { get; set; }
        public bool IsA6Format { get; set; }

        public string FullJobNumber => $"{JobNumber}-{Suffix}";
        public bool CanPrint => Status == "Запущено" && PrintStatus == "GOOD";
    }
}