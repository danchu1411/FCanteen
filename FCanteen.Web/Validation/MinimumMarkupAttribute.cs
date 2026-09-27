using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FCanteen.Web.Validation;

[AttributeUsage(
    AttributeTargets.Property,
    AllowMultiple = false)]
public sealed class MinimumMarkupAttribute
    : ValidationAttribute,
      IClientModelValidator
{
    public MinimumMarkupAttribute(
        string costProperty,
        double percentage)
    {
        CostProperty =
            costProperty;

        Percentage =
            Convert.ToDecimal(
                percentage);
    }

    public string CostProperty
    {
        get;
    }

    public decimal Percentage
    {
        get;
    }

    protected override ValidationResult?
        IsValid(
            object? value,
            ValidationContext validationContext)
    {
        if (value is not decimal price)
        {
            return ValidationResult.Success;
        }

        PropertyInfo? costProperty =
            validationContext
                .ObjectType
                .GetProperty(
                    CostProperty);

        if (costProperty is null)
        {
            return new ValidationResult(
                $"Không tìm thấy property " +
                $"'{CostProperty}'.");
        }

        var costValue =
            costProperty.GetValue(
                validationContext
                    .ObjectInstance);

        if (costValue is not decimal cost)
        {
            return ValidationResult.Success;
        }

        /*
         * Một món chưa được gán nguyên liệu
         * sẽ có Cost = 0.
         *
         * YC5 sau này sẽ cho phép gán
         * nguyên liệu trực tiếp trên UI.
         */
        if (cost <= 0)
        {
            return ValidationResult.Success;
        }

        var minimumPrice =
            cost *
            (1m +
             Percentage / 100m);

        if (price < minimumPrice)
        {
            return new ValidationResult(
                ErrorMessage ??
                $"Giá bán phải ít nhất " +
                $"{Percentage:N0}% cao hơn " +
                $"giá vốn. " +
                $"Giá vốn: {cost:N0} VND, " +
                $"giá tối thiểu: " +
                $"{minimumPrice:N0} VND.");
        }

        return ValidationResult.Success;
    }

    public void AddValidation(
        ClientModelValidationContext context)
    {
        MergeAttribute(
            context.Attributes,
            "data-val",
            "true");

        MergeAttribute(
            context.Attributes,
            "data-val-minimummarkup",
            ErrorMessage ??
            $"Giá bán phải lớn hơn " +
            $"giá vốn ít nhất " +
            $"{Percentage:N0}%.");

        MergeAttribute(
            context.Attributes,
            "data-val-minimummarkup-percent",
            Percentage.ToString(
                CultureInfo.InvariantCulture));

        MergeAttribute(
            context.Attributes,
            "data-val-minimummarkup-costfield",
            CostProperty);
    }

    private static bool MergeAttribute(
        IDictionary<string, string> attributes,
        string key,
        string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }

        attributes.Add(
            key,
            value);

        return true;
    }
}
