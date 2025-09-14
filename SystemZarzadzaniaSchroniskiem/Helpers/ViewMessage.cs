
namespace SystemZarzadzaniaSchroniskiem.Helpers
{
  public class ViewMessage(string Type, string Content)
  {
    public string Type { get; set; } = Type;
    public string Content { get; set; } = Content;
  }
}
