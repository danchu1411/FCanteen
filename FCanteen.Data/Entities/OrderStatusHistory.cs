namespace FCanteen.Data.Entities;

public class OrderStatusHistory
{
    public int OrderStatusHistoryId { get; set; }

    public int OrderTicketId { get; set; }

    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.Now;

    public OrderTicket OrderTicket { get; set; } = null!;
}