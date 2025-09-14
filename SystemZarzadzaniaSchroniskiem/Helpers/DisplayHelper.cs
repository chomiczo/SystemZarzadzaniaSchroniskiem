using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Html;
using System.Globalization;
using SystemZarzadzaniaSchroniskiem.Models;
using SystemZarzadzaniaSchroniskiem.Controllers;
using Humanizer;

namespace SystemZarzadzaniaSchroniskiem.Helpers
{
    public static class DisplayHelper
    {
        public static string NumberSuffix(this int number, string singleSuffix, string fewSuffix, string manySuffix)
        {
            if (number % 10 == 1)
            {
                return $"{number} {singleSuffix}";
            }
            else if (number % 10 > 1 && number % 10 < 5)
            {
                return $"{number} {fewSuffix}";
            }
            else
            {
                return $"{number} {manySuffix}";
            }
        }

        public static string CultureDate(this DateTime dt, string format = "d MMMM yyyy r.")
        {
            var culture = CultureInfo.CreateSpecificCulture("pl-PL");
            return dt.ToString(format, culture);
        }

        public static string CultureDateTime(this DateTime dt, string format = "d MMMM yyyy r. HH:mm")
        {
            var culture = CultureInfo.CreateSpecificCulture("pl-PL");
            return dt.ToString(format, culture);
        }

        public static IHtmlContent Icon(this Species species)
        {
            var builder = new HtmlContentBuilder();
            var iconName = species == Species.Cat ? "fa-cat" : "fa-dog";
            builder.AppendHtml($@"<i class=""fas {iconName}""></i>");
            return builder;
        }

        public static string DisplayShortNameFor<TModel, TResult>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TResult>> expression)
        {
            var model = expression.Parameters.FirstOrDefault()?.Type;
            if (expression.Body is not MemberExpression expr)
            {
                return "";
            }

            var member = model?.GetMember(expr.Member.Name).FirstOrDefault();
            var attr = member?.GetCustomAttribute<DisplayAttribute>();
            return attr?.GetShortName() ?? attr?.GetName() ?? "";
        }

        public static string InvalidClassNameFor<TModel, TResult>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TResult>> expression)
        {
            var model = expression.Parameters.FirstOrDefault()?.Type;
            if (expression.Body is not MemberExpression expr)
            {
                return "";
            }

            var valid = helper.ViewData.ModelState[expr.Member.Name];
            if (valid?.ValidationState == ModelValidationState.Invalid)
            {
                return "is-invalid";
            }
            return "";
        }

        public static IHtmlContent FormInputFor<TModel, TResult>(
            this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TResult>> expression,
            string inputType = "text", string? inputId = null)
        {
            if (expression.Body is not MemberExpression expr)
            {
                return HtmlString.Empty;
            }

            ModelErrorCollection? fieldErrors = null;
            var output = new HtmlContentBuilder();
            var requestMethod = helper.ViewContext.HttpContext.Request.Method;
            var fieldName = expr.Member.Name;

            string inputClass = "form-control";

            if (requestMethod == "POST"
                    && helper.ViewData.ModelState[fieldName] is ModelStateEntry field)
            {
                if (field.ValidationState == ModelValidationState.Invalid)
                {
                    inputClass += " is-invalid";
                }
                fieldErrors = field.Errors;
            }

            var value = inputType == "time" ? helper.ValueFor(expression, "{0:r}") : helper.ValueFor(expression);
            // helper.FormatValue()

            output.AppendHtml($@"
                    <div class=""input-group has-validation"">
                        <span class=""input-group-text"">
                            {helper.DisplayNameFor(expression)}
                        </span>
                        <input
                            class=""form-control {inputClass}""
                            type=""{inputType}""
                            name=""{fieldName}""
                            value=""{value}""");
            if (inputId != null)
            {
                output.AppendHtml($@" id=""{inputId}""");
            }

            output.AppendHtml(">");

            if (fieldErrors != null)
            {
                foreach (var err in fieldErrors)
                {
                    output.AppendHtml($@"
                    <span class=""invalid-feedback field-validation-error"" data-valmsg-for=""{fieldName}"">
                        {err.ErrorMessage}
                    </span>
                ");
                }
            }

            output.AppendHtml($@"
                </div>
            ");

            return output;
        }

        public static IHtmlContent FormSelectFor<TModel, TResult>(
            this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TResult>> expression,
            string? selectId = null
        )
        {
            if (expression.Body is not MemberExpression expr)
            {
                return HtmlString.Empty;
            }

            var output = new HtmlContentBuilder();
            var requestMethod = helper.ViewContext.HttpContext.Request.Method;
            var fieldName = expr.Member.Name;
            string id = selectId ?? "";

            output.AppendHtml($@"
                    <div class=""input-group has-validation"">
                        <span class=""input-group-text"">
                            {helper.DisplayNameFor(expression)}
                        </span>
                        <select name=""{fieldName}"" class=""form-control form-select"" id=""{id}"">");

            TResult? result = default(TResult);

            if (helper.ViewContext.ViewData.Model != null)
            {
                result = (TResult?)((LambdaExpression)expression).Compile().DynamicInvoke(helper.ViewContext.ViewData.Model);
            }

            var enumValues = Enum.GetValues(typeof(TResult));

            foreach (var item in helper.GetEnumSelectList(typeof(TResult)))
            {
                output.AppendHtml($@"<option value=""{item.Value}""");
                TResult? itemValue = (TResult?)enumValues.GetValue(int.Parse(item.Value));
                if (itemValue?.Equals(result) ?? false)
                {
                    output.AppendHtml(" selected");
                }
                output.AppendHtml($@">{item.Text}</option>");
            }

            output.AppendHtml("</select>");

            if (helper.ViewData.ModelState[fieldName] is ModelStateEntry field
                    && requestMethod == "POST")
            {
                foreach (var err in field.Errors)
                {
                    output.AppendHtml($@"
                        <span class=""invalid-feedback field-validation-error"" data-valmsg-for=""{fieldName}"">
                            {err.ErrorMessage}
                        </span>
                    ");
                }
            }

            output.AppendHtml($@"
                </div>
            ");

            return output;
        }

        public static IHtmlContent IconForRole(this IHtmlHelper helper, string role)
        {
            string iconId = "";

            switch (role)
            {
                case "Administrator": iconId = "fa-user-tie"; break;
                case "Employee": iconId = "fa-building-user"; break;
                case "Veterinarian": iconId = "fa-user-doctor"; break;
                case "Volunteer": iconId = "fa-handshake-angle"; break;
                case "Adopter": iconId = "fa-paw"; break;
            }

            var builder = new HtmlContentBuilder();
            builder.AppendHtml(@$"<i class=""fas {iconId}""></i>");
            return builder;
        }

        public static string RoleName(this IHtmlHelper helper, string role)
        {
            return role switch
            {
                "Employee" => "Pracownik",
                "Veterinarian" => "Weterynarz",
                "Volunteer" => "Wolontariusz",
                "Adopter" => "Adoptujący",
                _ => role,
            };
        }


        public static IHtmlContent ToggleButtonForRole(this ProfileWithRoles profile, string role)
        {
            var builder = new HtmlContentBuilder();
            builder.AppendHtml($@"<button onclick=""toggleRole({profile.Profile.Id}, '{role}')""class=""toggle-role-btn btn btn-outline-primary");
            if (profile.Roles.Contains(role))
            {
                builder.AppendHtml(" active");
            }
            builder.AppendHtml($@"""data-role=""{role}"" data-profile-id=""{profile.Profile.Id}"" data-bs-toggle=""tooltip"" data-bs-placement=""top"" ");
            switch (role)
            {
                case "Administrator":
                    builder.AppendHtml(@"data-bs-title=""Administrator""><i class=""fas fa-user-tie""></i>");
                    break;
                case "Employee":
                    builder.AppendHtml(@"data-bs-title=""Pracownik""><i class=""fas fa-building-user""></i>");
                    break;
                case "Veterinarian":
                    builder.AppendHtml(@"data-bs-title=""Weterynarz""><i class=""fas fa-user-doctor""></i>");
                    break;
                case "Volunteer":
                    builder.AppendHtml(@"data-bs-title=""Wolontariusz""><i class=""fas fa-handshake-angle""></i>");
                    break;
                case "Adopter":
                    builder.AppendHtml(@"data-bs-title=""Adoptujący""><i class=""fas fa-paw""></i>");
                    break;
            }
            builder.AppendHtml("</button>");
            return builder;
        }

        public static IHtmlContent ProfileName(this UserProfile? profile, string format = "{f} {l} ({e})")
        {
            var builder = new HtmlContentBuilder();

            if (profile == null)
            {
                builder.AppendHtml($@"<span style=""color: var(--sc-black-50);"">konto usunięte</span>");
            }
            else
            {
                var fmt = format
                    .Replace("{f}", $"{profile.FirstName}")
                    .Replace("{F}", $"{profile.FirstName.First()}.")
                    .Replace("{l}", $"{profile.LastName}")
                    .Replace("{e}", $"{profile.User.Email}");
                builder.AppendHtml(fmt);
            }

            return builder;
        }
    }
}
