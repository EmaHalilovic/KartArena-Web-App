//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace KartArena.Application.Modules.Catalog.Cities.Commands.Create;

//    public class CreateCityCommandHandler(IAppDbContext context)
//    : IRequestHandler<CreateCityCommand, int>
//{
//    public async Task<int> Handle(CreateCityCommand request, CancellationToken cancellationToken)
//    {
//        var normalizedName = request.Name?.Trim();
//        var normalizedCountry = request.Country?.Trim();
//        var normalizedPostalCode = request.PostalCode?.Trim();


//        if (string.IsNullOrWhiteSpace(normalizedName))
//            throw new ValidationException("Name is required.");
//        if (string.IsNullOrWhiteSpace(normalizedPostalCode))
//            throw new ValidationException("PostalCode is required.");
//        if (string.IsNullOrWhiteSpace(normalizedCountry))
//            throw new ValidationException("Country is required.");


//        // Check if a category with the same name already exists.
//        bool exists = await context.ProductCategories
//            .AnyAsync(x => x.Name == normalized, cancellationToken);

//        if (exists)
//        {
//            throw new MarketConflictException("Name already exists.");
//        }

//        var category = new ProductCategoryEntity
//        {
//            Name = request.Name!.Trim(),
//            IsEnabled = true // deault IsEnabled
//        };

//        context.ProductCategories.Add(category);
//        await context.SaveChangesAsync(cancellationToken);

//        return category.Id;
//    }
//}
        
    

