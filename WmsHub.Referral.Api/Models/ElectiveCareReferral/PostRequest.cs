using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using WmsHub.Common.Extensions;

namespace WmsHub.Referral.Api.Models.ElectiveCareReferral;

public class PostRequest
{
  private const StringComparison IGNORE_CASE =
    StringComparison.InvariantCultureIgnoreCase;

  public enum RowType
  {
    Header = 0,
    Data = 1,
    Empty = 2,
    Invalid = 3
  }

  [Required]
  public Guid TrustUserId { get; set; }

  [Required]
  public string TrustOdsCode { get; set; }

  [Required]
  public IFormFile File { get; set; }

  public List<TrustDataRow> AllRows { get; set; } = new();

  public List<TrustDataRow> DataRows => AllRows
    .Where(x => x.RowType == RowType.Data)
    .ToList();

  public int EmptyRowsCount => AllRows
    .Count(x => x.RowType == RowType.Empty);

  public bool HasEmptyRows => AllRows.Any(x => x.RowType == RowType.Empty);

  public bool HasHeaderRow => AllRows.Any(x => x.RowType == RowType.Header);

  private DataSet _dataSet = new();

  public class TrustDataRow
  {

    public TrustDataRow(int rowNumber, RowType rowType)
    {
      RowNumber = rowNumber;
      RowType = rowType;
    }

    public DateTimeOffset? DateOfBirth { get; set; }

    public DateTimeOffset? DateOfTrustReportedBmi { get; set; }

    public DateTimeOffset? DatePlacedOnWaitingList { get; set; }

    public string Ethnicity { get; set; }

    public string FamilyName { get; set; }

    public string GivenName { get; set; }

    public RowType RowType { get; private set; }

    public string Mobile { get; set; }

    public string NhsNumber { get; set; }

    public string OpcsCodes { get; set; }

    public string Postcode { get; set; }

    public int RowNumber { get; private set; }

    public string SexAtBirth { get; set; }

    public string SpellIdentifier { get; set; }

    public bool? SurgeryInLessThanEighteenWeeks { get; set; }

    public string TrustOdsCode { get; set; }

    public decimal? TrustReportedBmi { get; set; }

    public List<string> ValidationErrors { get; private set; } = new();

    public void SetAsDuplicateNhsNumber(TrustDataRow duplicateTrustDataRow)
    {
      ValidationErrors.Add($"The field 'NHS Number' is a duplicate " +
        $"of row {duplicateTrustDataRow.RowNumber}.");
    }

    public void SetAsDuplicateSpellIdentifier(
      TrustDataRow duplicateTrustDataRow)
    {
      ValidationErrors.Add($"The field 'Spell Identifier' is a duplicate " +
        $"of row {duplicateTrustDataRow.RowNumber}.");
    }


    internal void SetAsUnexpectedOdsCode(string expectedTrustOdsCode)
    {
      ValidationErrors.Add($"The field 'Trust ODS code' does not " +
        $"contain the expected ODS code of {expectedTrustOdsCode}.");
    }
  }

  public void GetRowsFromFile()
  {
    GetDataSetFromFile();
    GetRowsFromDataset();
  }

  private void GetDataSetFromFile()
  {
    IExcelDataReader reader = null;

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    Stream fileStream = File.OpenReadStream();

    if (fileStream != null)
    {
      if (File.FileName.EndsWith(".xls", IGNORE_CASE))
      {
        reader = ExcelReaderFactory.CreateBinaryReader(fileStream);
      }
      else
      {
        reader = File.FileName.EndsWith(".xlsx", IGNORE_CASE)
          ? ExcelReaderFactory.CreateOpenXmlReader(fileStream)
          : File.FileName.EndsWith(".csv", IGNORE_CASE)
            ? ExcelReaderFactory.CreateCsvReader(fileStream)
            : throw new InvalidOperationException("Filetype not supported.");
      }
    }

    if (reader != null)
    {
      _dataSet = reader.AsDataSet();
      reader.Close();
      reader.Dispose();
    }
  }

  private void GetRowsFromDataset()
  {
    if (_dataSet == null || _dataSet.Tables.Count == 0)
    {
      return;
    }

    DataTable requests = _dataSet.Tables[0];

    int rowNumber = 0;
    foreach (DataRow row in requests.Rows)
    {
      rowNumber++;

      // first row must be header
      if (rowNumber == 1)
      {
        if (IsHeaderRow(row))
        {
          AllRows.Add(new TrustDataRow(rowNumber, RowType.Header));
        }
      }
      else
      {
        if (row.ItemArray.All(x => x.ToString() == string.Empty))
        {
          AllRows.Add(new TrustDataRow(rowNumber, RowType.Empty));
        }
        else if (row.ItemArray.Length != ColumnNames.Length)
        {
          AllRows.Add(new TrustDataRow(rowNumber, RowType.Invalid));
        }
        else
        {
          TrustDataRow trustDataRow = new(rowNumber, RowType.Data)
          {
            DateOfBirth = row.GetColumnAsDateTimeOffset(0),
            DateOfTrustReportedBmi = row.GetColumnAsDateTimeOffset(1),
            DatePlacedOnWaitingList = row.GetColumnAsDateTimeOffset(2),
            Ethnicity = row.GetColumnAsString(3),
            FamilyName = row.GetColumnAsString(4),
            GivenName = row.GetColumnAsString(5),
            Mobile = row.GetColumnAsString(6),
            NhsNumber = row.GetColumnAsString(7),
            OpcsCodes = row.GetColumnAsString(8),
            Postcode = row.GetColumnAsString(9),
            SexAtBirth = row.GetColumnAsString(10),
            SurgeryInLessThanEighteenWeeks = row.GetColumnAsBool(11),
            TrustOdsCode = row.GetColumnAsString(12),
            TrustReportedBmi = row.GetColumnAsDecimal(13),
            SpellIdentifier = row.GetColumnAsString(14)
          };

          TrustDataRow duplicateNhsNumberRow = AllRows
            .FirstOrDefault(x => x.NhsNumber == trustDataRow.NhsNumber);

          if (duplicateNhsNumberRow != null)
          {
            trustDataRow.SetAsDuplicateNhsNumber(duplicateNhsNumberRow);
          }

          // check for duplicate spell identifier if one is present
          if (trustDataRow.SpellIdentifier != string.Empty)
          {
            TrustDataRow duplicateSpellIdentifierRow = AllRows
              .FirstOrDefault(x =>
                x.SpellIdentifier == trustDataRow.SpellIdentifier);

            if (duplicateSpellIdentifierRow != null)
            {
              trustDataRow
                .SetAsDuplicateSpellIdentifier(duplicateSpellIdentifierRow);
            }
          }

          if (trustDataRow.TrustOdsCode != TrustOdsCode)
          {
            trustDataRow.SetAsUnexpectedOdsCode(TrustOdsCode);
          }

          AllRows.Add(trustDataRow);
        }
      }
    }
  }

  private static bool IsHeaderRow(DataRow row)
  {
    if (row.ItemArray.Length == ColumnNames.Length)
    {
      for (int i = 0; i < ColumnNames.Length; i++)
      {
        if (!row.IsMatch(i, ColumnNames[i]))
        {
          return false;
        }
      }
      return true;
    }
    else
    {
      return false;
    }
  }

  private static string[] ColumnNames => new string[]
  {
    "DATEOFBIRTH",
    "DATEOFTRUSTREPORTEDBMI",
    "DATEPLACEDONWAITINGLIST",
    "ETHNICITY",
    "FAMILYNAME",
    "GIVENNAME",
    "MOBILE",
    "NHSNUMBER",
    "OPCSSURGERYCODE(S)",
    "POSTCODE",
    "SEXATBIRTH",
    "SURGERYINLESSTHAN18WEEKS?",
    "TRUSTODSCODE",
    "TRUSTREPORTEDBMI",
    "SPELLIDENTIFIER(OPTIONAL)"
  };
}
