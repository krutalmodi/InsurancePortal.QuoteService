using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InsurancePortal.QuoteService.API.Validation
{
    public sealed class ProductQuoteValidationMetadataProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (context.Key.ContainerType != typeof(CreateQuoteRequest))
                return;

            var productType = context.Key.Name switch
            {
                nameof(CreateQuoteRequest.AutoQuote) => ProductType.Auto,
                nameof(CreateQuoteRequest.HomeQuote) => ProductType.Home,
                nameof(CreateQuoteRequest.HealthQuote) => ProductType.Health,
                _ => (ProductType?)null
            };

            if (productType.HasValue)
                context.ValidationMetadata.PropertyValidationFilter = new ProductQuoteValidationFilter(productType.Value);
        }

        private sealed class ProductQuoteValidationFilter : IPropertyValidationFilter
        {
            private readonly ProductType _productType;

            public ProductQuoteValidationFilter(ProductType productType)
            {
                _productType = productType;
            }

            public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry)
            {
                return parentEntry.Model is not CreateQuoteRequest request || request.ProductType == _productType;
            }
        }
    }
}
