using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Products.Commands;

public class CreateProductCommand : IRequest<BaseEntity<CreateProductCommandResponse>>
{
    public string Code { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsHasVariant { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }
    public string? MetaTitle { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    public ICollection<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();
    public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
}

public record AttributeDto
{
    public string Name { get; set; }
    public ICollection<string> Values { get; set; } = new List<string>();
}

public class ProductVariantDto
{
    public string Id { get; set; } = string.Empty;
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public long? StockQty { get; set; }
    public Dictionary<string, string> AttributeValues { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new List<string>();
}

public record CreateProductCommandResponse
{
    public Guid Id { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseEntity<CreateProductCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(ShipmentDbContext dbContext, ILogger<CreateProductCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<CreateProductCommandResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _dbContext.Products
            .Where(p => p.Code == request.Code || p.SKU == request.SKU).AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (existingProduct != null)
        {
            _logger.LogWarning($"Product with Code '{request.Code}' or SKU '{request.SKU}' already exists.");
            return new BaseEntity<CreateProductCommandResponse>
            {
                Status = false,
                Message = "Product with the same Code or SKU already exists.",
                Data = null
            };
        }

        //using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = request.Name,
                Brand = request.Brand,
                Category = request.Category,
                Code = request.Code,
                SKU = request.SKU,
                ImageUrl = request.ImageUrl,
                Description = request.Description,
                MetaTitle = request.MetaTitle,
                IsHasVariant = request.IsHasVariant,
                Price = request.Price,
                CreateAt = DateTime.UtcNow,
                ProductAttributes = new List<ProductAttribute>(),
                Variants = new List<ProductVariant>()
            };

            // Thêm ProductAttributes và ProductAttributeValues trước
            foreach (var attr in request.Attributes)
            {
                var productAttribute = new ProductAttribute
                {
                    Id = Guid.NewGuid(),
                    Name = attr.Name,
                    ProductId = productId,
                    Values = new List<ProductAttributeValue>()
                };

                foreach (var value in attr.Values)
                {
                    var productAttributeValue = new ProductAttributeValue
                    {
                        Id = Guid.NewGuid(),
                        Value = value
                    };
                    productAttribute.Values.Add(productAttributeValue);
                }

                product.ProductAttributes.Add(productAttribute);
            }

            // Thêm ProductVariants và ProductVariantAtttributeValues
            foreach (var variant in request.Variants)
            {
                var productVariant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    SKU = variant.SKU,
                    StockQty = variant.StockQty,
                    Price = variant.Price,
                    Weight = variant.Weight,
                    Length = variant.Length,
                    Width = variant.Width,
                    Height = variant.Height,
                    Attributes = new List<ProductVariantAtttributeValue>(),
                    Images = variant.ImageUrls.Select(url => new ProductVariantImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = url
                    }).ToList()
                };

                foreach (var attributeValue in variant.AttributeValues)
                {
                    var productVariantAttributeValue = new ProductVariantAtttributeValue
                    {
                        Id = Guid.NewGuid(),
                        ProductAttributeValueId = product.ProductAttributes
                            .SelectMany(attr => attr.Values)
                            .FirstOrDefault(v => v.Value == attributeValue.Value)?.Id ?? Guid.Empty,
                        ProductVariantId = productVariant.Id
                    };

                    productVariant.Attributes.Add(productVariantAttributeValue);
                }

                product.Variants.Add(productVariant);
            }

            // Thêm product vào database
            await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            //await transaction.CommitAsync(cancellationToken);

            return new BaseEntity<CreateProductCommandResponse>
            {
                Status = true,
                Message = "Product created successfully.",
                Data = new CreateProductCommandResponse { Id = product.Id }
            };
        }
        catch (Exception ex)
        {
            //await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"Error creating Product: {ex.Message}");
            return new BaseEntity<CreateProductCommandResponse>
            {
                Status = false,
                Message = "Error occurred while creating the product.",
                Data = null
            };
        }
    }
}
