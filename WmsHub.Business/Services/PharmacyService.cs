using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services
{
  public class PharmacyService
    : ServiceBase<Entities.Pharmacy>, IPharmacyService
  {
    private readonly IMapper _mapper;

    public PharmacyService(DatabaseContext context, IMapper mapper)
      : base(context)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Creates a practice.
    /// </summary>
    /// <param name="createModel">
    /// An object with an IPharmacy interface.</param>
    /// <returns>A pharmacy model containing the saved properties.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PharmacyExistsException"></exception>
    /// <exception cref="PharmacyInvalidException"></exception>
    /// <exception cref="DbUpdateException"></exception>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    public async Task<IPharmacy> CreateAsync(IPharmacy createModel)
    {
      if (createModel == null)
        throw new ArgumentNullException(nameof(createModel));

      ValidatePharmacy(createModel);

      Entities.Pharmacy entity = await _context.Pharmacies
        .Where(p => p.OdsCode == createModel.OdsCode)
        .FirstOrDefaultAsync();

      if (entity == null)
      {
        entity = new Entities.Pharmacy();
        _context.Pharmacies.Add(entity);
      }
      else if (entity.IsActive)
      {
        throw new PharmacyExistsException(createModel.OdsCode);
      }

      entity.Email = createModel.Email;
      entity.OdsCode = createModel.OdsCode;
      entity.TemplateVersion = createModel.TemplateVersion;
      entity.IsActive = true;

      UpdateModified(entity);

      await _context.SaveChangesAsync();

      Pharmacy model = _mapper.Map<Pharmacy>(entity);

      return model;
    }

    /// <summary>
    /// Gets all pharmacies.
    /// </summary>
    /// <returns>All active pharmacies.</returns>
    public virtual async Task<IEnumerable<IPharmacy>> GetAsync()
    {
      var models = await _context.Pharmacies
        .Where(p => p.IsActive)
        .ProjectTo<Pharmacy>(_mapper.ConfigurationProvider)
        .ToListAsync();

      return models;
    }

    /// <summary>
    /// Gets an existing pharmacy model by ODS code.
    /// </summary>
    /// <param name="odsCode">The ODS code of the pharmacy to return.</param>
    /// <returns>The existing pharmacy model or null.</returns>
    /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
    public async Task<Pharmacy> GetByObsCodeAsync(string odsCode)
    {
      if (string.IsNullOrWhiteSpace(odsCode))
        throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));

      var model = await _context.Pharmacies
        .Where(p => p.IsActive)
        .Where(p => p.OdsCode == odsCode)
        .ProjectTo<Pharmacy>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();

      return model;
    }

    /// <summary>
    /// Updates an existing practice.
    /// </summary>
    /// <param name="updateModel">
    /// An object with an IPharmacy interface.</param>
    /// <returns>A pharmacy model containing the updated properties.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PharmacyInvalidException"></exception>
    /// <exception cref="PharmacyNotFoundException"></exception>
    /// <exception cref="DbUpdateException"></exception>
    /// <exception cref="DbUpdateConcurrencyException"></exception>    
    public async Task<IPharmacy> UpdateAsync(IPharmacy updateModel)
    {
      if (updateModel == null)
        throw new ArgumentNullException(nameof(updateModel));

      ValidatePharmacy(updateModel);

      Entities.Pharmacy entity = await _context.Pharmacies
        .Where(p => p.OdsCode == updateModel.OdsCode)
        .FirstOrDefaultAsync();

      if (entity == null)
        throw new PharmacyNotFoundException(updateModel.OdsCode);

      entity.Email = updateModel.Email;
      entity.IsActive = true;
      entity.TemplateVersion = updateModel.TemplateVersion;

      UpdateModified(entity);

      await _context.SaveChangesAsync();

      Pharmacy model = _mapper.Map<Pharmacy>(entity);

      return model;
    }

    /// <summary>
    /// Validates the pharmacy using its validation attributes
    /// </summary>
    /// <param name="model">The pharmacy to validate</param>
    /// <exception cref="PharmacyInvalidException"></exception>
    private void ValidatePharmacy(IPharmacy model)
    {
      ValidateModelResult validationResult = ValidateModel(model);
      if (!validationResult.IsValid)
        throw new PharmacyInvalidException(validationResult.GetErrorMessage());
    }
  }
}