using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Catalog.Requests;
using Omranco.BLL.DTOS.Catalog.Responses;
using Omranco.BLL.Services.Abstractions.CatalogServices;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.CatalogServices
{
    public sealed class CatalogService
        : ICatalogService
    {
        private readonly IUnitOfWork _unitOfWork;


        public CatalogService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork =
                unitOfWork;
        }


        // =====================================================
        // Public - Categories
        // =====================================================

        public async Task<Result<List<CategoryTreeResponse>>>
            GetCategoriesAsync()
        {
            var categories =
                await _unitOfWork
                    .Categories
                    .GetAllAsync(
                        filter:
                            x => x.IsActive,

                        orderBy:
                            query =>
                                query
                                    .OrderBy(
                                        x => x.SortOrder
                                    )
                                    .ThenBy(
                                        x => x.Name
                                    )
                    );


            var lookup =
                categories
                    .GroupBy(
                        x => x.ParentId
                    )
                    .ToDictionary(
                        x => x.Key,
                        x => x.ToList()
                    );


            var response =
                BuildCategoryChildren(
                    null,
                    lookup
                );


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Public - Units
        // =====================================================

        public async Task<Result<List<UnitResponse>>>
            GetUnitsAsync()
        {
            var units =
                await _unitOfWork
                    .Units
                    .GetAllAsync(
                        filter:
                            x => x.IsActive,

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.Name
                                )
                    );


            return Result.Success(
                units
                    .Select(
                        x =>
                            new UnitResponse(
                                x.Id,
                                x.Name,
                                x.Symbol
                            )
                    )
                    .ToList()
            );
        }


        // =====================================================
        // Public - Products
        // =====================================================

        public async Task<Result<List<ProductSummaryResponse>>>
            GetProductsAsync(
                int? categoryId)
        {
            List<int>? categoryIds =
                null;


            if (categoryId.HasValue)
            {
                var category =
                    await _unitOfWork
                        .Categories
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                    categoryId.Value
                                &&
                                x.IsActive
                        );


                if (category is null)
                {
                    return Result
                        .Failure<List<ProductSummaryResponse>>(
                            CatalogErrors.CategoryNotFound
                        );
                }


                categoryIds =
                    await GetCategoryAndDescendantIdsAsync(
                        category.Id
                    );
            }


            var products =
                await _unitOfWork
                    .Products
                    .GetAllAsync(
                        filter:
                            categoryIds is null
                                ? x => x.IsActive
                                : x =>
                                    x.IsActive
                                    &&
                                    categoryIds.Contains(
                                        x.CategoryId
                                    ),

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.Name
                                ),

                        includes:
                        [
                            x => x.Category
                        ]
                    );


            return Result.Success(
                products
                    .Select(
                        x =>
                            new ProductSummaryResponse(
                                x.Id,
                                x.CategoryId,
                                x.Category.Name,
                                x.Name,
                                x.Description,
                                x.ImagePath
                            )
                    )
                    .ToList()
            );
        }


        // =====================================================
        // Public - Product Details
        // =====================================================

        public async Task<Result<ProductDetailsResponse>>
            GetProductAsync(
                int productId)
        {
            var product =
                await _unitOfWork
                    .Products
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                productId
                            &&
                            x.IsActive,

                        x => x.Category
                    );


            if (product is null)
            {
                return Result
                    .Failure<ProductDetailsResponse>(
                        CatalogErrors.ProductNotFound
                    );
            }


            return await BuildProductDetailsAsync(
                product
            );
        }


        // =====================================================
        // Admin - Create Category
        // =====================================================

        public async Task<Result<int>>
            CreateCategoryAsync(
                CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure<int>(
                    CatalogErrors.InvalidName
                );
            }


            if (request.ParentId.HasValue)
            {
                var parentExists =
                    await _unitOfWork
                        .Categories
                        .AnyAsync(
                            x =>
                                x.Id ==
                                    request.ParentId.Value
                                &&
                                x.IsActive
                        );


                if (!parentExists)
                {
                    return Result.Failure<int>(
                        CatalogErrors.InvalidCategoryParent
                    );
                }
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Categories
                    .AnyAsync(
                        x =>
                            x.ParentId ==
                                request.ParentId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    CatalogErrors.CategoryAlreadyExists
                );
            }


            var category =
                new Category
                {
                    ParentId =
                        request.ParentId,

                    Name =
                        name,

                    Description =
                        Normalize(
                            request.Description
                        ),

                    ImagePath =
                        Normalize(
                            request.ImagePath
                        ),

                    SortOrder =
                        request.SortOrder,

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .Categories
                .InsertAsync(
                    category
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                category.Id
            );
        }


        // =====================================================
        // Admin - Update Category
        // =====================================================

        public async Task<Result>
            UpdateCategoryAsync(
                int categoryId,
                UpdateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure(
                    CatalogErrors.InvalidName
                );
            }


            var category =
                await _unitOfWork
                    .Categories
                    .GetByIdAsync(
                        categoryId
                    );


            if (category is null)
            {
                return Result.Failure(
                    CatalogErrors.CategoryNotFound
                );
            }


            if (
                request.ParentId.HasValue
                &&
                request.ParentId.Value ==
                    categoryId
            )
            {
                return Result.Failure(
                    CatalogErrors.CategoryCycle
                );
            }


            if (request.ParentId.HasValue)
            {
                var parent =
                    await _unitOfWork
                        .Categories
                        .GetByIdAsync(
                            request.ParentId.Value
                        );


                if (
                    parent is null
                    ||
                    !parent.IsActive
                )
                {
                    return Result.Failure(
                        CatalogErrors.InvalidCategoryParent
                    );
                }


                var createsCycle =
                    await WouldCreateCategoryCycleAsync(
                        categoryId,
                        request.ParentId.Value
                    );


                if (createsCycle)
                {
                    return Result.Failure(
                        CatalogErrors.CategoryCycle
                    );
                }
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Categories
                    .AnyAsync(
                        x =>
                            x.Id !=
                                categoryId
                            &&
                            x.ParentId ==
                                request.ParentId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure(
                    CatalogErrors.CategoryAlreadyExists
                );
            }


            var trackedCategory =
                await _unitOfWork
                    .Categories
                    .FindAsync(
                        categoryId
                    );


            if (trackedCategory is null)
            {
                return Result.Failure(
                    CatalogErrors.CategoryNotFound
                );
            }


            trackedCategory.ParentId =
                request.ParentId;

            trackedCategory.Name =
                name;

            trackedCategory.Description =
                Normalize(
                    request.Description
                );

            trackedCategory.ImagePath =
                Normalize(
                    request.ImagePath
                );

            trackedCategory.SortOrder =
                request.SortOrder;

            trackedCategory.IsActive =
                request.IsActive;

            trackedCategory.UpdatedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Unit
        // =====================================================

        public async Task<Result<int>>
            CreateUnitAsync(
                CreateUnitRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure<int>(
                    CatalogErrors.InvalidName
                );
            }
            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Units
                    .AnyAsync(
                        x =>
                            x.Name ==
                            name
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    CatalogErrors.UnitAlreadyExists
                );
            }


            var unit =
                new Unit
                {
                    Name =
                        name,

                    Symbol =
                        Normalize(
                            request.Symbol
                        ),

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .Units
                .InsertAsync(
                    unit
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                unit.Id
            );
        }


        // =====================================================
        // Admin - Update Unit
        // =====================================================

        public async Task<Result>
            UpdateUnitAsync(
                int unitId,
                UpdateUnitRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure(
                    CatalogErrors.InvalidName
                );
            }


            var unit =
                await _unitOfWork
                    .Units
                    .GetByIdAsync(
                        unitId
                    );


            if (unit is null)
            {
                return Result.Failure(
                    CatalogErrors.UnitNotFound
                );
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Units
                    .AnyAsync(
                        x =>
                            x.Id !=
                                unitId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure(
                    CatalogErrors.UnitAlreadyExists
                );
            }


            var trackedUnit =
                await _unitOfWork
                    .Units
                    .FindAsync(
                        unitId
                    );


            if (trackedUnit is null)
            {
                return Result.Failure(
                    CatalogErrors.UnitNotFound
                );
            }


            trackedUnit.Name =
                name;

            trackedUnit.Symbol =
                Normalize(
                    request.Symbol
                );

            trackedUnit.IsActive =
                request.IsActive;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Product
        // =====================================================

        public async Task<Result<int>>
            CreateProductAsync(
                CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure<int>(
                    CatalogErrors.InvalidName
                );
            }


            var categoryExists =
                await _unitOfWork
                    .Categories
                    .AnyAsync(
                        x =>
                            x.Id ==
                                request.CategoryId
                            &&
                            x.IsActive
                    );


            if (!categoryExists)
            {
                return Result.Failure<int>(
                    CatalogErrors.CategoryNotFound
                );
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Products
                    .AnyAsync(
                        x =>
                            x.CategoryId ==
                                request.CategoryId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    CatalogErrors.ProductAlreadyExists
                );
            }


            var unitValidation =
                await ValidateProductUnitsAsync(
                    request.Units
                );


            if (unitValidation.IsFailure)
            {
                return Result.Failure<int>(
                    unitValidation.Error
                );
            }


            var product =
                new Product
                {
                    CategoryId =
                        request.CategoryId,

                    Name =
                        name,

                    Description =
                        Normalize(
                            request.Description
                        ),

                    ImagePath =
                        Normalize(
                            request.ImagePath
                        ),

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow,

                    Units =
                        request.Units
                            .Select(
                                x =>
                                    new ProductUnit
                                    {
                                        UnitId =
                                            x.UnitId,

                                        IsDefault =
                                            x.IsDefault
                                    }
                            )
                            .ToList()
                };


            await _unitOfWork
                .Products
                .InsertAsync(
                    product
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                product.Id
            );
        }


        // =====================================================
        // Admin - Update Product
        // =====================================================

        public async Task<Result>
            UpdateProductAsync(
                int productId,
                UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure(
                    CatalogErrors.InvalidName
                );
            }


            var product =
                await _unitOfWork
                    .Products
                    .GetByIdAsync(
                        productId
                    );


            if (product is null)
            {
                return Result.Failure(
                    CatalogErrors.ProductNotFound
                );
            }


            var categoryExists =
                await _unitOfWork
                    .Categories
                    .AnyAsync(
                        x =>
                            x.Id ==
                                request.CategoryId
                            &&
                            x.IsActive
                    );


            if (!categoryExists)
            {
                return Result.Failure(
                    CatalogErrors.CategoryNotFound
                );
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .Products
                    .AnyAsync(
                        x =>
                            x.Id !=
                                productId
                            &&
                            x.CategoryId ==
                                request.CategoryId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure(
                    CatalogErrors.ProductAlreadyExists
                );
            }


            var trackedProduct =
                await _unitOfWork
                    .Products
                    .FindAsync(
                        productId
                    );


            if (trackedProduct is null)
            {
                return Result.Failure(
                    CatalogErrors.ProductNotFound
                );
            }


            trackedProduct.CategoryId =
                request.CategoryId;

            trackedProduct.Name =
                name;

            trackedProduct.Description =
                Normalize(
                    request.Description
                );

            trackedProduct.ImagePath =
                Normalize(
                    request.ImagePath
                );

            trackedProduct.IsActive =
                request.IsActive;

            trackedProduct.UpdatedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Product Units
        // =====================================================

        public async Task<Result>
            SetProductUnitsAsync(
                int productId,
                SetProductUnitsRequest request)
        {
            var productExists =
                await _unitOfWork
                    .Products
                    .AnyAsync(
                        x =>
                            x.Id ==
                            productId
                    );


            if (!productExists)
            {
                return Result.Failure(
                    CatalogErrors.ProductNotFound
                );
            }


            var validation =
                await ValidateProductUnitsAsync(
                    request.Units
                );


            if (validation.IsFailure)
            {
                return validation;
            }


            var existing =
                await _unitOfWork
                    .ProductUnits
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductId ==
                                productId
                    );


            foreach (var productUnit in existing)
            {
                var tracked =
                    await _unitOfWork
                        .ProductUnits
                        .FindAsync(
                            productUnit.Id
                        );


                if (tracked is not null)
                {
                    _unitOfWork
                        .ProductUnits
                        .Delete(
                            tracked
                        );
                }
            }


            var newLinks =
                request.Units
                    .Select(
                        x =>
                            new ProductUnit
                            {
                                ProductId =
                                    productId,

                                UnitId =
                                    x.UnitId,

                                IsDefault =
                                    x.IsDefault
                            }
                    )
                    .ToList(); await _unitOfWork
                .ProductUnits
                .InsertListAsync(
                    newLinks
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Attribute
        // =====================================================

        public async Task<Result<int>>
            CreateAttributeAsync(
                int productId,
                CreateProductAttributeRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure<int>(
                    CatalogErrors.InvalidName
                );
            }


            var productExists =
                await _unitOfWork
                    .Products
                    .AnyAsync(
                        x =>
                            x.Id ==
                            productId
                    );


            if (!productExists)
            {
                return Result.Failure<int>(
                    CatalogErrors.ProductNotFound
                );
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .ProductAttributes
                    .AnyAsync(
                        x =>
                            x.ProductId ==
                                productId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    CatalogErrors.AttributeAlreadyExists
                );
            }


            var attribute =
                new ProductAttribute
                {
                    ProductId =
                        productId,

                    Name =
                        name,

                    IsRequired =
                        request.IsRequired,

                    SortOrder =
                        request.SortOrder,

                    IsActive =
                        true
                };


            await _unitOfWork
                .ProductAttributes
                .InsertAsync(
                    attribute
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                attribute.Id
            );
        }


        // =====================================================
        // Admin - Update Attribute
        // =====================================================

        public async Task<Result>
            UpdateAttributeAsync(
                int attributeId,
                UpdateProductAttributeRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Name))
            {
                return Result.Failure(
                    CatalogErrors.InvalidName
                );
            }


            var attribute =
                await _unitOfWork
                    .ProductAttributes
                    .GetByIdAsync(
                        attributeId
                    );


            if (attribute is null)
            {
                return Result.Failure(
                    CatalogErrors.AttributeNotFound
                );
            }


            var name =
                request.Name.Trim();


            var duplicate =
                await _unitOfWork
                    .ProductAttributes
                    .AnyAsync(
                        x =>
                            x.Id !=
                                attributeId
                            &&
                            x.ProductId ==
                                attribute.ProductId
                            &&
                            x.Name ==
                                name
                    );


            if (duplicate)
            {
                return Result.Failure(
                    CatalogErrors.AttributeAlreadyExists
                );
            }


            var trackedAttribute =
                await _unitOfWork
                    .ProductAttributes
                    .FindAsync(
                        attributeId
                    );


            if (trackedAttribute is null)
            {
                return Result.Failure(
                    CatalogErrors.AttributeNotFound
                );
            }


            trackedAttribute.Name =
                name;

            trackedAttribute.IsRequired =
                request.IsRequired;

            trackedAttribute.SortOrder =
                request.SortOrder;

            trackedAttribute.IsActive =
                request.IsActive;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Option
        // =====================================================

        public async Task<Result<int>>
            CreateAttributeOptionAsync(
                int attributeId,
                CreateProductAttributeOptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Value))
            {
                return Result.Failure<int>(
                    CatalogErrors.InvalidName
                );
            }


            var attributeExists =
                await _unitOfWork
                    .ProductAttributes
                    .AnyAsync(
                        x =>
                            x.Id ==
                            attributeId
                    );


            if (!attributeExists)
            {
                return Result.Failure<int>(
                    CatalogErrors.AttributeNotFound
                );
            }


            var value =
                request.Value.Trim();


            var duplicate =
                await _unitOfWork
                    .ProductAttributeOptions
                    .AnyAsync(
                        x =>
                            x.ProductAttributeId ==
                                attributeId
                            &&
                            x.Value ==
                                value
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    CatalogErrors.OptionAlreadyExists
                );
            }


            var option =
                new ProductAttributeOption
                {
                    ProductAttributeId =
                        attributeId,

                    Value =
                        value,

                    SortOrder =
                        request.SortOrder,

                    IsActive =
                        true
                };


            await _unitOfWork
                .ProductAttributeOptions
                .InsertAsync(
                    option
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                option.Id
            );
        }


        // =====================================================
        // Admin - Update Option
        // =====================================================

        public async Task<Result>
            UpdateAttributeOptionAsync(
                int optionId,
                UpdateProductAttributeOptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Value))
            {
                return Result.Failure(
                    CatalogErrors.InvalidName
                );
            }


            var option =
                await _unitOfWork
                    .ProductAttributeOptions
                    .GetByIdAsync(
                        optionId
                    );


            if (option is null)
            {
                return Result.Failure(
                    CatalogErrors.OptionNotFound
                );
            }


            var value =
                request.Value.Trim();


            var duplicate =
                await _unitOfWork
                    .ProductAttributeOptions
                    .AnyAsync(
                        x =>
                            x.Id !=
                                optionId
                            &&
                            x.ProductAttributeId ==
                                option.ProductAttributeId
                            &&
                            x.Value ==
                                value
                    );


            if (duplicate)
            {
                return Result.Failure(
                    CatalogErrors.OptionAlreadyExists
                );
            }


            var trackedOption =
                await _unitOfWork
                    .ProductAttributeOptions
                    .FindAsync(
                        optionId
                    );


            if (trackedOption is null)
            {
                return Result.Failure(
                    CatalogErrors.OptionNotFound
                );
            }


            trackedOption.Value =
                value;

            trackedOption.SortOrder =
                request.SortOrder;

            trackedOption.IsActive =
                request.IsActive; await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Variant
        // =====================================================

        public async Task<Result<int>>
            CreateVariantAsync(
                int productId,
                CreateProductVariantRequest request)
        {
            var productExists =
                await _unitOfWork
                    .Products
                    .AnyAsync(
                        x =>
                            x.Id ==
                            productId
                    );


            if (!productExists)
            {
                return Result.Failure<int>(
                    CatalogErrors.ProductNotFound
                );
            }


            var sku =
                Normalize(
                    request.Sku
                );


            if (sku is not null)
            {
                var skuExists =
                    await _unitOfWork
                        .ProductVariants
                        .AnyAsync(
                            x =>
                                x.Sku ==
                                sku
                        );


                if (skuExists)
                {
                    return Result.Failure<int>(
                        CatalogErrors.SkuAlreadyExists
                    );
                }
            }


            var valuesResult =
                await ValidateVariantValuesAsync(
                    productId,
                    request.Values
                );


            if (valuesResult.IsFailure)
            {
                return Result.Failure<int>(
                    valuesResult.Error
                );
            }


            var variant =
                new ProductVariant
                {
                    ProductId =
                        productId,

                    Name =
                        Normalize(
                            request.Name
                        ),

                    Sku =
                        sku,

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow,

                    Values =
                        valuesResult.Value!
                };


            await _unitOfWork
                .ProductVariants
                .InsertAsync(
                    variant
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                variant.Id
            );
        }


        // =====================================================
        // Admin - Update Variant
        // =====================================================

        public async Task<Result>
            UpdateVariantAsync(
                int variantId,
                UpdateProductVariantRequest request)
        {
            var variant =
                await _unitOfWork
                    .ProductVariants
                    .GetByIdAsync(
                        variantId
                    );


            if (variant is null)
            {
                return Result.Failure(
                    CatalogErrors.VariantNotFound
                );
            }


            var sku =
                Normalize(
                    request.Sku
                );


            if (sku is not null)
            {
                var skuExists =
                    await _unitOfWork
                        .ProductVariants
                        .AnyAsync(
                            x =>
                                x.Id !=
                                    variantId
                                &&
                                x.Sku ==
                                    sku
                        );


                if (skuExists)
                {
                    return Result.Failure(
                        CatalogErrors.SkuAlreadyExists
                    );
                }
            }


            var valuesResult =
                await ValidateVariantValuesAsync(
                    variant.ProductId,
                    request.Values
                );


            if (valuesResult.IsFailure)
            {
                return valuesResult;
            }


            var oldValues =
                await _unitOfWork
                    .ProductVariantValues
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductVariantId ==
                                variantId
                    );


            foreach (var oldValue in oldValues)
            {
                var trackedOldValue =
                    await _unitOfWork
                        .ProductVariantValues
                        .FindAsync(
                            oldValue.Id
                        );


                if (trackedOldValue is not null)
                {
                    _unitOfWork
                        .ProductVariantValues
                        .Delete(
                            trackedOldValue
                        );
                }
            }


            var trackedVariant =
                await _unitOfWork
                    .ProductVariants
                    .FindAsync(
                        variantId
                    );


            if (trackedVariant is null)
            {
                return Result.Failure(
                    CatalogErrors.VariantNotFound
                );
            }


            trackedVariant.Name =
                Normalize(
                    request.Name
                );

            trackedVariant.Sku =
                sku;

            trackedVariant.IsActive =
                request.IsActive;

            trackedVariant.UpdatedAt =
                DateTime.UtcNow;


            var newValues =
                valuesResult.Value!;


            foreach (var value in newValues)
            {
                value.ProductVariantId =
                    variantId;
            }


            if (newValues.Count > 0)
            {
                await _unitOfWork
                    .ProductVariantValues
                    .InsertListAsync(
                        newValues
                    );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Helpers - Product Units
        // =====================================================

        private async Task<Result>
            ValidateProductUnitsAsync(
                List<ProductUnitRequest>? units)
        {
            if (
                units is null
                ||
                units.Count == 0
            )
            {
                return Result.Failure(
                    CatalogErrors.ProductUnitsRequired
                );
            }


            var unitIds =
                units
                    .Select(
                        x => x.UnitId
                    )
                    .ToList();


            if (
                unitIds.Distinct().Count() !=
                unitIds.Count
            )
            {
                return Result.Failure(
                    CatalogErrors.ProductUnitsRequired
                );
            }


            if (
                units.Count(
                    x => x.IsDefault
                )
                != 1
            )
            {
                return Result.Failure(
                    CatalogErrors
                        .ExactlyOneDefaultUnitRequired
                );
            }


            var existingUnits =
                await _unitOfWork
                    .Units
                    .GetAllAsync(
                        filter:
                            x =>
                                unitIds.Contains(
                                    x.Id
                                )
                                &&
                                x.IsActive
                    );


            if (
                existingUnits.Count !=
                unitIds.Count
            )
            {
                return Result.Failure(
                    CatalogErrors.UnitNotFound
                );
            }


            return Result.Success();
        }


        // =====================================================
        // Helpers - Variant Values
        // =====================================================

        private async Task<Result<List<ProductVariantValue>>>
            ValidateVariantValuesAsync(
                int productId,
                List<ProductVariantValueRequest>? requestedValues)
        {
            requestedValues ??=
                [];


            var attributes =
                await _unitOfWork
                    .ProductAttributes
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductId ==
                                    productId
                                &&
                                x.IsActive
                    );


            var activeAttributeIds =
                attributes
                    .Select(
                        x => x.Id
                    )
                    .ToHashSet();


            var requestedAttributeIds =
                requestedValues
                    .Select(
                        x =>
                            x.ProductAttributeId
                    )
                    .ToList();


            // Only one option per attribute.
            if (
                requestedAttributeIds
                    .Distinct()
                    .Count()
                !=
                requestedAttributeIds.Count
            )
            {
                return Result
                    .Failure<List<ProductVariantValue>>(
                        CatalogErrors.InvalidVariantValues
                    );
            }


            // Every referenced attribute must belong
            // to the product and must be active.
            if (
                requestedAttributeIds.Any(
                    x =>
                        !activeAttributeIds
                            .Contains(x)
                )
            )
            {
                return Result
                    .Failure<List<ProductVariantValue>>(
                        CatalogErrors.InvalidVariantValues
                    );
            }


            // Every required active attribute
            // must have one selected option.
            var requiredAttributeIds =
                attributes
                    .Where(
                        x => x.IsRequired
                    )
                    .Select(
                        x => x.Id
                    )
                    .ToHashSet();


            if (
                requiredAttributeIds.Any(
                    x =>
                        !requestedAttributeIds
                            .Contains(x)
                )
            )
            {
                return Result
                    .Failure<List<ProductVariantValue>>(
                        CatalogErrors.InvalidVariantValues
                    );
            }


            var optionIds =
                requestedValues
                    .Select(
                        x =>
                            x.ProductAttributeOptionId
                    )
                    .Distinct()
                    .ToList();


            var options =
                optionIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .ProductAttributeOptions
                        .GetAllAsync(
                            filter:
                                x =>
                                    optionIds.Contains(
                                        x.Id
                                    )
                                    &&
                                    x.IsActive
                        );


            if (
                options.Count !=
                optionIds.Count
            )
            {
                return Result
                    .Failure<List<ProductVariantValue>>(
                        CatalogErrors.InvalidVariantValues
                    );
            }


            var optionLookup =
                options.ToDictionary(
                    x => x.Id
                );


            foreach (var value in requestedValues)
            {
                if (
                    !optionLookup.TryGetValue(
                        value.ProductAttributeOptionId,
                        out var option
                    )
                    ||
                    option.ProductAttributeId !=
                        value.ProductAttributeId
                )
                {
                    return Result
                        .Failure<List<ProductVariantValue>>(
                            CatalogErrors.InvalidVariantValues
                        );
                }
            }


            var values =
                requestedValues
                    .Select(
                        x =>
                            new ProductVariantValue
                            {
                                ProductAttributeId =
                                    x.ProductAttributeId,

                                ProductAttributeOptionId =
                                    x.ProductAttributeOptionId
                            }
                    )
                    .ToList();


            return Result.Success(
                values
            );
        }


        // =====================================================
        // Helpers - Product Details
        // =====================================================

        private async Task<Result<ProductDetailsResponse>>
            BuildProductDetailsAsync(
                Product product)
        {
            var productUnits =
                await _unitOfWork
                    .ProductUnits
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductId ==
                                product.Id
                    ); var unitIds =
                productUnits
                    .Select(
                        x => x.UnitId
                    )
                    .ToList();


            var units =
                unitIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .Units
                        .GetAllAsync(
                            filter:
                                x =>
                                    unitIds.Contains(
                                        x.Id
                                    )
                                    &&
                                    x.IsActive
                        );


            var unitLookup =
                units.ToDictionary(
                    x => x.Id
                );


            var attributes =
                await _unitOfWork
                    .ProductAttributes
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductId ==
                                    product.Id
                                &&
                                x.IsActive,

                        orderBy:
                            query =>
                                query
                                    .OrderBy(
                                        x => x.SortOrder
                                    )
                                    .ThenBy(
                                        x => x.Name
                                    )
                    );


            var attributeIds =
                attributes
                    .Select(
                        x => x.Id
                    )
                    .ToList();


            var options =
                attributeIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .ProductAttributeOptions
                        .GetAllAsync(
                            filter:
                                x =>
                                    attributeIds.Contains(
                                        x.ProductAttributeId
                                    )
                                    &&
                                    x.IsActive,

                            orderBy:
                                query =>
                                    query.OrderBy(
                                        x => x.SortOrder
                                    )
                        );


            var variants =
                await _unitOfWork
                    .ProductVariants
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProductId ==
                                    product.Id
                                &&
                                x.IsActive,

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.Id
                                )
                    );


            var variantIds =
                variants
                    .Select(
                        x => x.Id
                    )
                    .ToList();


            var variantValues =
                variantIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .ProductVariantValues
                        .GetAllAsync(
                            filter:
                                x =>
                                    variantIds.Contains(
                                        x.ProductVariantId
                                    )
                        );


            var attributeLookup =
                attributes.ToDictionary(
                    x => x.Id
                );


            var optionLookup =
                options.ToDictionary(
                    x => x.Id
                );


            return Result.Success(
                new ProductDetailsResponse(
                    product.Id,
                    product.CategoryId,
                    product.Category.Name,
                    product.Name,
                    product.Description,
                    product.ImagePath,

                    productUnits
                        .Where(
                            x =>
                                unitLookup.ContainsKey(
                                    x.UnitId
                                )
                        )
                        .OrderByDescending(
                            x => x.IsDefault
                        )
                        .Select(
                            x =>
                            {
                                var unit =
                                    unitLookup[
                                        x.UnitId
                                    ];


                                return new ProductUnitResponse(
                                    unit.Id,
                                    unit.Name,
                                    unit.Symbol,
                                    x.IsDefault
                                );
                            }
                        )
                        .ToList(),

                    attributes
                        .Select(
                            attribute =>
                                new ProductAttributeResponse(
                                    attribute.Id,
                                    attribute.Name,
                                    attribute.IsRequired,
                                    attribute.SortOrder,

                                    options
                                        .Where(
                                            option =>
                                                option
                                                    .ProductAttributeId ==
                                                attribute.Id
                                        )
                                        .OrderBy(
                                            option =>
                                                option.SortOrder
                                        )
                                        .Select(
                                            option =>
                                                new ProductAttributeOptionResponse(
                                                    option.Id,
                                                    option.Value,
                                                    option.SortOrder
                                                )
                                        )
                                        .ToList()
                                )
                        )
                        .ToList(),

                    variants
                        .Select(
                            variant =>
                                new ProductVariantResponse(
                                    variant.Id,
                                    variant.Name,
                                    variant.Sku,

                                    variantValues
                                        .Where(
                                            value =>
                                                value
                                                    .ProductVariantId ==
                                                variant.Id
                                        )
                                        .Where(
                                            value =>
                                                attributeLookup
                                                    .ContainsKey(
                                                        value
                                                            .ProductAttributeId
                                                    )
                                                &&
                                                optionLookup
                                                    .ContainsKey(
                                                        value
                                                            .ProductAttributeOptionId
                                                    )
                                        )
                                        .Select(
                                            value =>
                                            {
                                                var attribute =
                                                    attributeLookup[
                                                        value.ProductAttributeId
                                                    ];

                                                var option =
                                                    optionLookup[
                                                        value.ProductAttributeOptionId
                                                    ];


                                                return new ProductVariantValueResponse(
                                                    attribute.Id,
                                                    attribute.Name,
                                                    option.Id,
                                                    option.Value
                                                );
                                            }
                                        )
                                        .ToList()
                                )
                        )
                        .ToList()
                )
            );
        }


        // =====================================================
        // Helpers - Category Hierarchy
        // =====================================================

        private async Task<List<int>>
            GetCategoryAndDescendantIdsAsync(
                int categoryId)
        {
            var categories =
                await _unitOfWork
                    .Categories
                    .GetAllAsync(
                        filter:
                            x => x.IsActive
                    );


            var result =
                new List<int>();


            AddCategoryAndChildren(
                categoryId,
                categories,
                result
            );


            return result;
        }


        private static void AddCategoryAndChildren(
            int categoryId,
            List<Category> categories,
            List<int> result)
        {
            if (result.Contains(
                    categoryId))
            {
                return;
            }


            result.Add(
                categoryId
            );


            var children =
                categories
                    .Where(
                        x =>
                            x.ParentId ==
                            categoryId
                    )
                    .ToList();


            foreach (var child in children)
            {
                AddCategoryAndChildren(
                    child.Id,
                    categories,
                    result
                );
            }
        }


        private async Task<bool>
            WouldCreateCategoryCycleAsync(
                int categoryId,
                int newParentId)
        {
            var visited =
                new HashSet<int>();


            int? currentId =
                newParentId;


            while (currentId.HasValue)
            {
                if (
                    currentId.Value ==
                    categoryId
                )
                {
                    return true;
                }


                if (!visited.Add(
                        currentId.Value))
                {
                    return true;
                }


                var category =
                    await _unitOfWork
                        .Categories
                        .GetByIdAsync(
                            currentId.Value
                        );


                if (category is null)
                {
                    return false;
                }


                currentId =
                    category.ParentId;
            }


            return false;
        }


        private static List<CategoryTreeResponse>
            BuildCategoryChildren(
                int? parentId,
                Dictionary<int?, List<Category>> lookup)
        {
            if (!lookup.TryGetValue(
                    parentId,
                    out var children))
            {
                return [];
            }


            return children
                .OrderBy(
                    x => x.SortOrder
                )
                .ThenBy(
                    x => x.Name
                )
                .Select(
                    category =>
                        new CategoryTreeResponse(
                            category.Id,
                            category.ParentId,
                            category.Name,
                            category.Description,
                            category.ImagePath,
                            category.SortOrder,

                            BuildCategoryChildren(
                                category.Id,
                                lookup
                            )
                        )
                )
                .ToList();
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }
    }
}