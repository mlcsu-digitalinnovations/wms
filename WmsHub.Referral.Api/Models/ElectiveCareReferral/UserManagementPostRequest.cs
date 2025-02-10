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
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.ElectiveCareReferral;

public class UserManagementPostRequest
{
  internal enum ActionType 
  {
    Header = 0,
    Data = 1,
    Empty = 2,
    Invalid = 3,
    Create = 4,
    Delete = 5
  }
  internal List<UserManagement> AllRows { get; set; } = new();
  private DataSet _dataSet;
  [Required]
  public IFormFile File { get; set; }
  private const StringComparison IGNORE_CASE =
    StringComparison.InvariantCultureIgnoreCase;

  public UserManagementPostRequest(IFormFile file)
  {
    File = file;
    GetRowsFromFile();
  }

  private static string[] ColumnNames => new string[]
 {
    "GIVENNAME",
    "SURNAME",
    "EMAILADDRESS",
    "ODSCODE",
    "ACTION"
 };

  public int EmptyRowsCount => AllRows
   .Count(x => x.ActionType == ActionType.Empty);

  private void GetDataSetFromFile()
  {
    IExcelDataReader reader = null;

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    Stream fileStream = File.OpenReadStream();

    if (fileStream != null)
    {
      if (File.FileName.EndsWith(Constants.XLS, IGNORE_CASE))
      {
        reader = ExcelReaderFactory.CreateBinaryReader(fileStream);
      }
      else
      {
        reader = File.FileName.EndsWith(Constants.XLSX, IGNORE_CASE)
          ? ExcelReaderFactory.CreateOpenXmlReader(fileStream)
          : File.FileName.EndsWith(Constants.CSV, IGNORE_CASE)
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
          AllRows.Add(new UserManagement(rowNumber, ActionType.Header));
        }
      }
      else
      {
        if (row.ItemArray.All(x => x.ToString() == string.Empty))
        {
          AllRows.Add(new UserManagement(rowNumber, ActionType.Empty));
        }
        else if (row.ItemArray.Length != ColumnNames.Length)
        {
          AllRows.Add(new UserManagement(rowNumber, ActionType.Invalid));
        }
        else
        {
          UserManagement userManagementRow = new (rowNumber)
          {
            GivenName = row.GetColumnAsString(0),
            Surname = row.GetColumnAsString(1),
            EmailAddress = row.GetColumnAsString(2),
            ODSCode = row.GetColumnAsString(3),
            Action = row.GetColumnAsString(4),
            ActionType = row.GetColumnAsString(4) switch
            {
              Constants.Actions.CREATE => ActionType.Create,
              Constants.Actions.DELETE => ActionType.Delete,
              _ => ActionType.Invalid
            }
          };

          UserManagement duplicateRow = AllRows
            .FirstOrDefault(x => 
              x.EmailAddress == userManagementRow.EmailAddress);

          if (duplicateRow != null)
          {
            userManagementRow.SetAsDuplicateEmail(duplicateRow);
          }
          AllRows.Add(userManagementRow);
        }
      }
    }
  }

  public void GetRowsFromFile()
  {
    GetDataSetFromFile();
    GetRowsFromDataset();
  }

  public bool HasEmptyRows => AllRows.Any(x => 
    x.ActionType == ActionType.Empty);

  public bool HasHeaderRow => AllRows.Any(x => 
    x.ActionType == ActionType.Header);

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

  internal class UserManagement
  {
    public UserManagement()
    {
    }

    public UserManagement(int rowNumber) => RowNumber = rowNumber;

    public UserManagement(int rowNumber, ActionType actionType)
    {
      RowNumber = rowNumber;
      ActionType = actionType;
    }

    public string Action { get; set; }
    public ActionType ActionType { get; set; }
    public string EmailAddress { get; set; }
    public string GivenName { get; set; }
    public string ODSCode { get; set; }
    public int RowNumber { get; private set; }
    public string Surname { get; set; }
    public List<string> ValidationErrors { get; private set; } = new();

    public void SetAsDuplicateEmail(UserManagement duplicateRow)
    {
      ValidationErrors.Add($"The field 'Email Address' is a duplicate " +
        $"of row {duplicateRow.RowNumber}.");
    }
  }

}
