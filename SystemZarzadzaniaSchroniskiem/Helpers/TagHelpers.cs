using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.IdentityModel.Tokens;

namespace SystemZarzadzaniaSchroniskiem.Helpers
{
  [HtmlTargetElement("page-layout")]
  public class PageLayoutTagHelper : TagHelper
  {
    public string? Title { get; set; }
    public string? TitleIcon { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
      var inner = output.GetChildContentAsync().Result.GetContent();
      output.TagName = "div";
      output.Attributes.Add("class", "container d-flex flex-column gap-4");
      output.TagMode = TagMode.StartTagAndEndTag;
      if (!Title.IsNullOrEmpty())
      {
        output.Content.AppendHtml($@"<div class=""row""><div class=""col""><h1 class=""page-title"">");
        if (!TitleIcon.IsNullOrEmpty())
        {
          string iconSet = "";
          if (TitleIcon!.StartsWith("fa-"))
          {
            iconSet = "fas";
          }
          else if (TitleIcon.StartsWith("bi-"))
          {
            iconSet = "bi";
          }
          output.Content.AppendHtml($@"<i class=""{iconSet} {TitleIcon}""></i>");
        }
        output.Content.AppendHtml($@"{Title}");
        output.Content.AppendHtml("</h1></div></div>");
      }

      output.Content.AppendHtml(inner);
    }
  }

  [HtmlTargetElement("icon")]
  public class IconTagHelper : TagHelper
  {
    public string? Name { get; set; }
    public string? Class { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
      if (Name.IsNullOrEmpty())
      {
        return;
      }

      output.TagName = "i";
      output.TagMode = TagMode.StartTagAndEndTag;
      string iconSet = "";
      if (Name!.StartsWith("fa-"))
      {
        iconSet = "fas";
      }
      else if (Name.StartsWith("bi-"))
      {
        iconSet = "bi";
      }

      // output.TagName = "div";
      output.Attributes.Add("class", $"{iconSet} {Name} {Class}");
      //   output.Content.AppendHtml($@"<div class=""row""><div class=""col""><h1 class=""page-title"">");
      //     output.Content.AppendHtml($@"<i class=""fas {TitleIcon}""></i>");
      //   output.Content.AppendHtml($@"{Title}");
      //   output.Content.AppendHtml("</h1></div></div>");

    }
  }
}
