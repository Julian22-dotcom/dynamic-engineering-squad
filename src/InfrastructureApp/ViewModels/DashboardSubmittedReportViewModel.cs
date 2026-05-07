namespace InfrastructureApp.ViewModels;

public class DashboardSubmittedReportViewModel
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
