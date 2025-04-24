using Jumia_Clone.Data;
using Jumia_Clone.Helpers;
using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Jumia_Clone.Repositories.Implementation
{
    public class ProductAttributeRepository : IProductAttributeRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductAttributeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ProductAttribute Methods
        public async Task<ProductAttributeDto> CreateProductAttributeAsync(CreateProductAttributeDto attributeDto)
        {
            // Validate attribute type
            if (!ProductAttributeTypes.IsValidType(attributeDto.Type))
            {
                throw new ArgumentException($"Invalid attribute type: {attributeDto.Type}");
            }

            // Validate possible values based on type
            if (new[] { ProductAttributeTypes.Dropdown,
                ProductAttributeTypes.Multiselect,
                ProductAttributeTypes.Radio }.Contains(attributeDto.Type))
            {
                if (string.IsNullOrEmpty(attributeDto.PossibleValues))
                {
                    throw new ArgumentException($"{attributeDto.Type} type requires possible values");
                }
            }
            // Validate subcategory exists
            var subcategory = await _context.SubCategories.FindAsync(attributeDto.SubcategoryId);
            if (subcategory == null)
                throw new KeyNotFoundException($"Subcategory with ID {attributeDto.SubcategoryId} not found");

            var productAttribute = new ProductAttribute
            {
                SubcategoryId = attributeDto.SubcategoryId,
                Name = attributeDto.Name,
                Type = attributeDto.Type,
                PossibleValues = attributeDto.PossibleValues,
                IsRequired = attributeDto.IsRequired,
                IsFilterable = attributeDto.IsFilterable
            };

            _context.ProductAttributes.Add(productAttribute);
            await _context.SaveChangesAsync();

            return new ProductAttributeDto
            {
                AttributeId = productAttribute.AttributeId,
                SubcategoryId = productAttribute.SubcategoryId,
                SubcategoryName = subcategory.Name,
                Name = productAttribute.Name,
                Type = productAttribute.Type,
                PossibleValues = productAttribute.PossibleValues,
                IsRequired = productAttribute.IsRequired ?? false,
                IsFilterable = productAttribute.IsFilterable ?? true
            };
        }

        public async Task<ProductAttributeDto> UpdateProductAttributeAsync(int attributeId, UpdateProductAttributeDto attributeDto)
        {
            // Validate attribute type
            if (!ProductAttributeTypes.IsValidType(attributeDto.Type))
            {
                throw new ArgumentException($"Invalid attribute type: {attributeDto.Type}");
            }

            // Validate possible values based on type
            if (new[] { ProductAttributeTypes.Dropdown,
                ProductAttributeTypes.Multiselect,
                ProductAttributeTypes.Radio }.Contains(attributeDto.Type))
            {
                if (string.IsNullOrEmpty(attributeDto.PossibleValues))
                {
                    throw new ArgumentException($"{attributeDto.Type} type requires possible values");
                }
            }
            var attribute = await _context.ProductAttributes
                .Include(pa => pa.Subcategory)
                .FirstOrDefaultAsync(pa => pa.AttributeId == attributeId);

            if (attribute == null)
                throw new KeyNotFoundException($"Product Attribute with ID {attributeId} not found");

            attribute.Name = attributeDto.Name;
            attribute.Type = attributeDto.Type;
            attribute.PossibleValues = attributeDto.PossibleValues;
            attribute.IsRequired = attributeDto.IsRequired;
            attribute.IsFilterable = attributeDto.IsFilterable;

            await _context.SaveChangesAsync();

            return new ProductAttributeDto
            {
                AttributeId = attribute.AttributeId,
                SubcategoryId = attribute.SubcategoryId,
                SubcategoryName = attribute.Subcategory.Name,
                Name = attribute.Name,
                Type = attribute.Type,
                PossibleValues = attribute.PossibleValues,
                IsRequired = attribute.IsRequired ?? false,
                IsFilterable = attribute.IsFilterable ?? true
            };
        }

        public async Task DeleteProductAttributeAsync(int attributeId)
        {
            var attribute = await _context.ProductAttributes.FindAsync(attributeId);

            if (attribute == null)
                throw new KeyNotFoundException($"Product Attribute with ID {attributeId} not found");

            // Check if any products are using this attribute
            var attributeInUse = await _context.ProductAttributeValues
                .AnyAsync(pav => pav.AttributeId == attributeId);

            if (attributeInUse)
                throw new InvalidOperationException("Cannot delete attribute that is in use by products");

            _context.ProductAttributes.Remove(attribute);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductAttributeDto> GetProductAttributeByIdAsync(int attributeId)
        {
            var attribute = await _context.ProductAttributes
                .Include(pa => pa.Subcategory)
                .FirstOrDefaultAsync(pa => pa.AttributeId == attributeId);

            if (attribute == null)
                throw new KeyNotFoundException($"Product Attribute with ID {attributeId} not found");

            return new ProductAttributeDto
            {
                AttributeId = attribute.AttributeId,
                SubcategoryId = attribute.SubcategoryId,
                SubcategoryName = attribute.Subcategory.Name,
                Name = attribute.Name,
                Type = attribute.Type,
                PossibleValues = attribute.PossibleValues,
                IsRequired = attribute.IsRequired ?? false,
                IsFilterable = attribute.IsFilterable ?? true
            };
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.ProductAttributes.CountAsync();
        }
        public async Task<List<ProductAttributeDto>> GetProductAttributes(PaginationDto pagination, bool include_details)
        {
            var attributes = await _context.ProductAttributes
                .Include(pa => pa.Subcategory).Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();

            if (attributes == null)
                throw new KeyNotFoundException($"Product Attributes not found");
            List<ProductAttributeDto> result = new List<ProductAttributeDto>();

            foreach(var attribute in attributes)
            {
                var attr = new ProductAttributeDto
                {
                    AttributeId = attribute.AttributeId,
                    SubcategoryId = attribute.SubcategoryId,
                    SubcategoryName = attribute.Subcategory.Name,
                    Name = attribute.Name,
                    Type = attribute.Type,
                    PossibleValues = attribute.PossibleValues,
                    IsRequired = attribute.IsRequired ?? false,
                    IsFilterable = attribute.IsFilterable ?? true
                };

                result.Add(attr);
            }
            return result;
        }

        public async Task<IEnumerable<ProductAttributeDto>> GetProductAttributesBySubcategoryAsync(int subcategoryId)
        {
            var attributes = await _context.ProductAttributes
                .Include(pa => pa.Subcategory)
                .Where(pa => pa.SubcategoryId == subcategoryId)
                .ToListAsync();

            return attributes.Select(attribute => new ProductAttributeDto
            {
                AttributeId = attribute.AttributeId,
                SubcategoryId = attribute.SubcategoryId,
                SubcategoryName = attribute.Subcategory.Name,
                Name = attribute.Name,
                Type = attribute.Type,
                PossibleValues = attribute.PossibleValues,
                IsRequired = attribute.IsRequired ?? false,
                IsFilterable = attribute.IsFilterable ?? true
            });
        }

        public async Task<ProductAttributeValueDto> AddProductAttributeValueAsync(CreateProductAttributeValueDto attributeValueDto)
        {
            // Validate product exists
            var product = await _context.Products.FindAsync(attributeValueDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {attributeValueDto.ProductId} not found");

            // Validate attribute exists
            var attribute = await _context.ProductAttributes
                .FirstOrDefaultAsync(pa => pa.AttributeId == attributeValueDto.AttributeId);
            if (attribute == null)
                throw new KeyNotFoundException($"Product Attribute with ID {attributeValueDto.AttributeId} not found");

            // Validate attribute type and value
            ValidateAttributeValue(attribute, attributeValueDto.Value);

            // Check if attribute value already exists for this product and attribute
            var existingValue = await _context.ProductAttributeValues
                .FirstOrDefaultAsync(pav =>
                    pav.ProductId == attributeValueDto.ProductId &&
                    pav.AttributeId == attributeValueDto.AttributeId);
            if (existingValue != null)
                throw new InvalidOperationException("Attribute value already exists for this product");

            var productAttributeValue = new ProductAttributeValue
            {
                ProductId = attributeValueDto.ProductId,
                AttributeId = attributeValueDto.AttributeId,
                Value = attributeValueDto.Value
            };

            _context.ProductAttributeValues.Add(productAttributeValue);
            await _context.SaveChangesAsync();

            return new ProductAttributeValueDto
            {
                ValueId = productAttributeValue.ValueId,
                ProductId = productAttributeValue.ProductId,
                AttributeId = productAttributeValue.AttributeId,
                AttributeName = attribute.Name,
                AttributeType = attribute.Type,
                Value = productAttributeValue.Value
            };
        }

        private void ValidateAttributeValue(ProductAttribute attribute, string value)
        {
            // Check if value is provided
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value cannot be empty for attribute {attribute.Name}");

            // Validate based on attribute type
            switch (attribute.Type)
            {
                case ProductAttributeTypes.Text:
                    if (value.Length > 500)
                        throw new ArgumentException($"Text value for {attribute.Name} is too long");
                    break;

                case ProductAttributeTypes.Number:
                    if (!AttributeValidationHelper.IsValidNumeric(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid integer");
                    break;

                case ProductAttributeTypes.Decimal:
                    if (!AttributeValidationHelper.IsValidNumeric(value, "decimal"))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid decimal number");
                    break;

                case ProductAttributeTypes.Date:
                    if (!AttributeValidationHelper.IsValidDate(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid date");
                    break;

                case ProductAttributeTypes.Dropdown:
                case ProductAttributeTypes.Radio:
                    if (!AttributeValidationHelper.IsValidListValue(value, attribute.PossibleValues))
                        throw new ArgumentException($"Value for {attribute.Name} must be one of: {attribute.PossibleValues}");
                    break;

                case ProductAttributeTypes.Multiselect:
                    if (!AttributeValidationHelper.IsValidListValue(value, attribute.PossibleValues, true))
                        throw new ArgumentException($"All values for {attribute.Name} must be from: {attribute.PossibleValues}");
                    break;

                case ProductAttributeTypes.Checkbox:
                    if (!AttributeValidationHelper.IsValidBoolean(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be true or false");
                    break;

                case ProductAttributeTypes.Color:
                    if (!AttributeValidationHelper.IsValidColor(value))
                        throw new ArgumentException($"Invalid color value for {attribute.Name}");
                    break;

                case ProductAttributeTypes.Size:
                    if (!AttributeValidationHelper.IsValidSize(value, attribute.PossibleValues))
                        throw new ArgumentException($"Invalid size value for {attribute.Name}");
                    break;

                default:
                    throw new ArgumentException($"Unsupported attribute type: {attribute.Type}");
            }
        }
        public async Task<ProductAttributeValueDto> UpdateProductAttributeValueAsync(int valueId, UpdateProductAttributeValueDto attributeValueDto)
        {
            var attributeValue = await _context.ProductAttributeValues
                .Include(pav => pav.Attribute)
                .FirstOrDefaultAsync(pav => pav.ValueId == valueId);

            if (attributeValue == null)
                throw new KeyNotFoundException($"Product Attribute Value with ID {valueId} not found");

            attributeValue.Value = attributeValueDto.Value;
            await _context.SaveChangesAsync();

            return new ProductAttributeValueDto
            {
                ValueId = attributeValue.ValueId,
                ProductId = attributeValue.ProductId,
                AttributeId = attributeValue.AttributeId,
                AttributeName = attributeValue.Attribute.Name,
                AttributeType = attributeValue.Attribute.Type,
                Value = attributeValue.Value
            };
        }

        public async Task DeleteProductAttributeValueAsync(int valueId)
        {
            var attributeValue = await _context.ProductAttributeValues.FindAsync(valueId);

            if (attributeValue == null)
                throw new KeyNotFoundException($"Product Attribute Value with ID {valueId} not found");

            _context.ProductAttributeValues.Remove(attributeValue);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductAttributeValueDto>> GetProductAttributeValuesAsync(int productId)
        {
            var attributeValues = await _context.ProductAttributeValues
                .Include(pav => pav.Attribute)
                .Where(pav => pav.ProductId == productId)
                .ToListAsync();

            return attributeValues.Select(av => new ProductAttributeValueDto
            {
                ValueId = av.ValueId,
                ProductId = av.ProductId,
                AttributeId = av.AttributeId,
                AttributeName = av.Attribute.Name,
                AttributeType = av.Attribute.Type,
                Value = av.Value
            });
        }
    }
}
