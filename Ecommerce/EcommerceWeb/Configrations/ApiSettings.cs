namespace EcommerceWeb.Configrations;

public class ApiSettings
{
    public required string Url { get; set; }
    public int Timeout { get; set; } = 60;
}
