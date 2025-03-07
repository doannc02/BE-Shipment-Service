using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Products.Handlers;

public class GetProductDetailQuery : IRequest<BaseEntity<ProductDetailResponse>>
{
    public Guid ProductId { get; set; }
}

public record ProductDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    public string Code { get; set; }
    public string SKU { get; set; }
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public string MetaTitle { get; set; }
    public bool IsHasVariant { get; set; }
    public decimal Price { get; set; }
    public DateTime CreateAt { get; set; }
    public List<ProductAttributeDetail> Attributes { get; set; } = new();
    public List<ProductVariantDetail> Variants { get; set; } = new();
}

public record ProductAttributeDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<string> Values { get; set; } = new();
}

public record ProductVariantDetail
{
    public Guid Id { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public long? StockQty { get; set; }
    public Dictionary<string, string> AttributeValues { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
}

public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, BaseEntity<ProductDetailResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetProductDetailQueryHandler> _logger;

    public GetProductDetailQueryHandler(ShipmentDbContext dbContext, ILogger<GetProductDetailQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<ProductDetailResponse>> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.Values)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning($"Product with Id '{request.ProductId}' not found.");
                return new BaseEntity<ProductDetailResponse>
                {
                    Status = false,
                    Message = "Product not found.",
                    Data = null
                };
            }

            var response = new ProductDetailResponse
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Code = product.Code,
                SKU = product.SKU,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                MetaTitle = product.MetaTitle,
                IsHasVariant = (bool)product.IsHasVariant,
                Price = (decimal)product.Price,
                CreateAt = product.CreateAt,
                Attributes = product.ProductAttributes.Select(pa => new ProductAttributeDetail
                {
                    Id = pa.Id,
                    Name = pa.Name,
                    Values = pa.Values.Select(v => v.Value).ToList()
                }).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantDetail
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Price = v.Price,
                    Weight = v.Weight,
                    Length = v.Length,
                    Width = v.Width,
                    Height = v.Height,
                    StockQty = v.StockQty,
                    AttributeValues = v.Attributes.ToDictionary(
                        a => a.ProductAttributeValue.ProductAttribute.Name,
                        a => a.ProductAttributeValue.Value
                    ) ?? new Dictionary<string, string>(),
                    ImageUrls = v.Images.Select(i => i.ImageUrl).ToList()
                }).ToList() ?? new List<ProductVariantDetail>()
            };

            return new BaseEntity<ProductDetailResponse>
            {
                Status = true,
                Message = "Product detail retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving product detail: {ex.Message}");
            return new BaseEntity<ProductDetailResponse>
            {
                Status = false,
                Message = "Error occurred while retrieving the product detail.",
                Data = null
            };
        }
    }
}