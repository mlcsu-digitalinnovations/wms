using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public enum EncryptionType
  {
    DETERMINISTIC,
    RANDOMIZED
  }

  public static class AlwaysEncryptedHelper
  {
    public static void AddEncryptedColumn(
      MigrationBuilder migrationBuilder,
      string schemaName,
      string tableName,
      string columnName,
      string columnType,
      EncryptionType encryptionType,
      string cek,
      bool isColumnNullable)
    {
      string binCollation = string.Empty;
      if (encryptionType == EncryptionType.DETERMINISTIC)
      {
        binCollation = " COLLATE Latin1_General_BIN2";
      }

      migrationBuilder.Sql(
        $"ALTER TABLE [{schemaName}].[{tableName}] ADD [{columnName}] " +
        $"{columnType} {binCollation} " + 
        $"ENCRYPTED WITH (ENCRYPTION_TYPE = {encryptionType}, " +
        $"ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256', " +
        $"COLUMN_ENCRYPTION_KEY = [{cek}]) " +
        $"{(isColumnNullable ? "" : "NOT")} NULL" 
      );
    }

    public static void AddEncryptedColumnWithAudit(
      MigrationBuilder migrationBuilder,
      string schemaName,
      string tableName,
      string columnName,
      string columnType,
      EncryptionType encryptionType,
      string cek,
      bool isColumnNullable)
    {
      AddEncryptedColumn(
        migrationBuilder,
        schemaName,
        tableName,
        columnName,
        columnType,
        encryptionType,
        cek,
        isColumnNullable);

      AddEncryptedColumn(
        migrationBuilder,
        schemaName,
        $"{tableName}Audit",
        columnName,
        columnType,
        encryptionType,
        cek,
        isColumnNullable);
    }
  }
}