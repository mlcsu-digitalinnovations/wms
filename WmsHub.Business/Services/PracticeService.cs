using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services
{
  public class PracticeService
    : ServiceBase<Entities.Practice>, IPracticeService
  {
    private readonly IMapper _mapper;

    public PracticeService(DatabaseContext context, IMapper mapper)
      : base(context)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Creates a practice.
    /// </summary>
    /// <param name="practiceCreate">
    /// An object with an IPractice interface.</param>
    /// <returns>A practice model containing the saved properties.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PracticeExistsException"></exception>
    /// <exception cref="PracticeInvalidException"></exception>
    /// <exception cref="DbUpdateException"></exception>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    public async Task<IPractice> CreateAsync(IPractice practiceCreate)
    {
      if (practiceCreate == null)
        throw new ArgumentNullException(nameof(practiceCreate));

      ValidatePractice(practiceCreate);

      Entities.Practice practiceEntity = await _context.Practices
        .Where(p => p.OdsCode == practiceCreate.OdsCode)
        .FirstOrDefaultAsync();

      if (practiceEntity == null)
      {
        practiceEntity = new Entities.Practice();
        _context.Practices.Add(practiceEntity);
      }
      else if (practiceEntity.IsActive)
      {
        throw new PracticeExistsException(practiceCreate.OdsCode);
      }

      practiceEntity.Email = practiceCreate.Email;
      practiceEntity.Name = practiceCreate.Name;
      practiceEntity.OdsCode = practiceCreate.OdsCode;
      practiceEntity.SystemName = practiceCreate.SystemName;
      practiceEntity.IsActive = true;

      UpdateModified(practiceEntity);      

      await _context.SaveChangesAsync();

      Practice createdPractice = _mapper.Map<Practice>(practiceEntity);

      return createdPractice;
    }

    /// <summary>
    /// Gets all practices.
    /// </summary>
    /// <returns>All active practices.</returns>
    public virtual async Task<IEnumerable<IPractice>> GetAsync()
    {
      IEnumerable<Practice> practices = await _context.Practices
        .Where(p => p.IsActive)
        .ProjectTo<Practice>(_mapper.ConfigurationProvider)
        .ToListAsync();

      return practices;
    }

    /// <summary>
    /// Gets an existing practice model by ODS code.
    /// </summary>
    /// <param name="odsCode">The ODS code of the practice to return.</param>
    /// <returns>The existing practice model or null.</returns>
    /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
    public async Task<Practice> GetByObsCodeAsync(string odsCode)
    {
      if (string.IsNullOrWhiteSpace(odsCode))
        throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));

      Practice practice = await _context.Practices
        .Where(p => p.IsActive)
        .Where(p => p.OdsCode == odsCode)
        .ProjectTo<Practice>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();

      return practice;
    }

    /// <summary>
    /// Updates an existing practice.
    /// </summary>
    /// <param name="practiceUpdate">
    /// An object with an IPractice interface.</param>
    /// <returns>A practice model containing the updated properties.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PracticeNotFoundException"></exception>
    /// <exception cref="PracticeInvalidException"></exception>
    /// <exception cref="DbUpdateException"></exception>
    /// <exception cref="DbUpdateConcurrencyException"></exception>    
    public async Task<IPractice> UpdateAsync(IPractice practiceUpdate)
    {
      if (practiceUpdate == null)
        throw new ArgumentNullException(nameof(practiceUpdate));

      ValidatePractice(practiceUpdate);

      Entities.Practice existingPractice = await _context.Practices
        .Where(p => p.OdsCode == practiceUpdate.OdsCode)
        .FirstOrDefaultAsync();

      if (existingPractice == null)
        throw new PracticeNotFoundException(practiceUpdate.OdsCode);

      existingPractice.Email = practiceUpdate.Email;
      existingPractice.IsActive = true;
      existingPractice.Name = practiceUpdate.Name;
      existingPractice.SystemName = practiceUpdate.SystemName;

      UpdateModified(existingPractice);

      await _context.SaveChangesAsync();

      Practice updatedPractice = _mapper.Map<Practice>(existingPractice);

      return updatedPractice;
    }

    /// <summary>
    /// Validates the practice using its validation attributes
    /// </summary>
    /// <param name="practice">The practice to validate</param>
    /// <exception cref="PracticeInvalidException"></exception>
    private void ValidatePractice(IPractice practice)
    {
      ValidateModelResult validationResult = ValidateModel(practice);
      if (!validationResult.IsValid)
        throw new PracticeInvalidException(validationResult.GetErrorMessage());
    }
  }
}
