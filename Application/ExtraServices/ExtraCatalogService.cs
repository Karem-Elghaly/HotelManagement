
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.ExtraServices;

public class ExtraCatalogService : IExtraCatalogService
{
    private readonly IRepository<ExtraService> _extras;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SaveExtraServiceRequest> _validator;

    public ExtraCatalogService(
        IRepository<ExtraService> extras, IUnitOfWork uow, IValidator<SaveExtraServiceRequest> validator)
    {
        _extras = extras;
        _uow = uow;
        _validator = validator;
    }


    public async Task<IReadOnlyList<ExtraServiceResponse>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        var list = await _extras.ListAsync(e => includeInactive || e.IsActive, ct);
        return list.OrderBy(e => e.Name).Select(e => e.ToResponse()).ToList();
    }

    public async Task<ExtraServiceResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();

    public async Task<ExtraServiceResponse> CreateAsync(SaveExtraServiceRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var name = request.Name.Trim();
        if (await _extras.ExistsAsync(e => e.Name == name, ct))
            throw new DomainException($"Extra service '{name}' already exists.");

        var entity = request.ToEntity();
        await _extras.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return entity.ToResponse();
    }

    public async Task<ExtraServiceResponse> UpdateAsync(int id, SaveExtraServiceRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var entity = await GetOrThrowAsync(id, ct);

        var name = request.Name.Trim();
        if (await _extras.ExistsAsync(e => e.Name == name && e.Id != id, ct))
            throw new DomainException($"Extra service '{name}' already exists.");

       
        request.ApplyTo(entity);
        await _uow.SaveChangesAsync(ct);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);

      
        _extras.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<ExtraService> GetOrThrowAsync(int id, CancellationToken ct)
        => await _extras.GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(ExtraService), id);
}