using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations;

/// <inheritdoc />
public partial class UdalExtract : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "UdalExtractsHistory",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
          ExtractDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          ModifiedFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
          ModifiedTo = table.Column<DateTime>(type: "datetime2", nullable: false),
          NumberOfCoachingInserts = table.Column<int>(type: "int", nullable: true),
          NumberOfCoachingUpdates = table.Column<int>(type: "int", nullable: true),
          NumberOfModifiedUpdates = table.Column<int>(type: "int", nullable: true),
          NumberOfProviderEngagementInserts = table.Column<int>(type: "int", nullable: true),
          NumberOfProviderEngagementUpdates = table.Column<int>(type: "int", nullable: true),
          NumberOfReferralInserts = table.Column<int>(type: "int", nullable: true),
          NumberOfReferralUpdates = table.Column<int>(type: "int", nullable: true),
          NumberOfWeightMeasurementInserts = table.Column<int>(type: "int", nullable: true),
          NumberOfWeightMeasurementUpdates = table.Column<int>(type: "int", nullable: true),
          StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UdalExtractsHistory", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "UdalExtracts",
        columns: table => new
        {
          ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          CalculatedBmiAtRegistration = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          Coaching0007 = table.Column<bool>(type: "bit", nullable: true),
          Coaching0814 = table.Column<bool>(type: "bit", nullable: true),
          Coaching1521 = table.Column<bool>(type: "bit", nullable: true),
          Coaching2228 = table.Column<bool>(type: "bit", nullable: true),
          Coaching2935 = table.Column<bool>(type: "bit", nullable: true),
          Coaching3642 = table.Column<bool>(type: "bit", nullable: true),
          Coaching4349 = table.Column<bool>(type: "bit", nullable: true),
          Coaching5056 = table.Column<bool>(type: "bit", nullable: true),
          Coaching5763 = table.Column<bool>(type: "bit", nullable: true),
          Coaching6470 = table.Column<bool>(type: "bit", nullable: true),
          Coaching7177 = table.Column<bool>(type: "bit", nullable: true),
          Coaching7884 = table.Column<bool>(type: "bit", nullable: true),
          ConsentForFutureContactForEvaluation = table.Column<bool>(type: "bit", nullable: true),
          DateCompletedProgramme = table.Column<DateTime>(type: "datetime2", nullable: true),
#if DEBUG_NOAE
          DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
#endif
          DateOfBmiAtRegistration = table.Column<DateTime>(type: "datetime2", nullable: true),
          DateOfProviderContactedServiceUser = table.Column<DateTime>(type: "datetime2", nullable: true),
          DateOfProviderSelection = table.Column<DateTime>(type: "datetime2", nullable: true),
          DateOfReferral = table.Column<DateTime>(type: "datetime2", nullable: true),
          DatePlacedOnWaitingListForElectiveCare = table.Column<DateTime>(type: "datetime2", nullable: true),
          DateStartedProgramme = table.Column<DateTime>(type: "datetime2", nullable: true),
          DateToDelayUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
          DeprivationQuintile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          DocumentVersion = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          Ethnicity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          EthnicityGroup = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          EthnicitySubGroup = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          GpRecordedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          GpSourceSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          HasALearningDisability = table.Column<bool>(type: "bit", nullable: true),
          HasAPhysicalDisability = table.Column<bool>(type: "bit", nullable: true),
          HasDiabetesType1 = table.Column<bool>(type: "bit", nullable: true),
          HasDiabetesType2 = table.Column<bool>(type: "bit", nullable: true),
          HasHypertension = table.Column<bool>(type: "bit", nullable: true),
          HasRegisteredSeriousMentalIllness = table.Column<bool>(type: "bit", nullable: true),
          HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          IsVulnerable = table.Column<bool>(type: "bit", nullable: true),
          ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
          MethodOfContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
          NhsNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
          NumberOfContacts = table.Column<int>(type: "int", nullable: true),
          OpcsCodesForElectiveCare = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          ProviderEngagement0007 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement0814 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement1521 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement2228 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement2935 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement3642 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement4349 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement5056 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement5763 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement6470 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement7177 = table.Column<bool>(type: "bit", nullable: true),
          ProviderEngagement7884 = table.Column<bool>(type: "bit", nullable: true),
          ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
          ProviderUbrn = table.Column<string>(type: "nvarchar(max)", maxLength: 200, nullable: true),
#endif
          ReferralSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          ReferringGpPracticeNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          ReferringOrganisationOdsCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          ServiceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          Sex = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          StaffRole = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          TriagedCompletionLevel = table.Column<int>(type: "int", nullable: true),
          UdalExtractHistoryId = table.Column<int>(type: "int", nullable: false),
#if DEBUG_NOAE
          VulnerableDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
#endif
          WeightMeasurement0007 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement0814 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement1521 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement2228 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement2935 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement3642 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement4349 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement5056 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement5763 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement6470 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement7177 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement7884 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
          WeightMeasurement8500 = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UdalExtracts", x => x.ReferralId);
          table.ForeignKey(
                    name: "FK_UdalExtracts_UdalExtractsHistory_UdalExtractHistoryId",
                    column: x => x.UdalExtractHistoryId,
                    principalTable: "UdalExtractsHistory",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

#if !DEBUG_NOAE
    AlwaysEncrypted.AddColumnsForMigration(
      AlwaysEncryptedMigrations.UdalExtract, migrationBuilder);
#endif

    migrationBuilder.CreateIndex(
        name: "IX_UdalExtracts_UdalExtractHistoryId",
        table: "UdalExtracts",
        column: "UdalExtractHistoryId");

    migrationBuilder.Sql(@"CREATE PROCEDURE [dbo].[usp_UdalExtractsMerge]
      @ModifiedFrom DATETIME=NULL, @ModifiedTo DATETIME=NULL
      AS
      BEGIN
          SET NOCOUNT ON;
          DECLARE @ExtractDate AS DATETIME;
          SET @ExtractDate = GETDATE();
          DECLARE @OutputUdalExtracts TABLE (
              [ActionPerformed] NVARCHAR (10));
          DECLARE @UdalExtractHistoryId AS INT;
          IF (@ModifiedFrom IS NULL)
              BEGIN
                  SELECT @ModifiedFrom = DATEADD(DAY, -1, CAST (MAX(ExtractDate) AS DATE))
                  FROM   dbo.UdalExtractHistory;
              END
          IF (@ModifiedFrom IS NULL)
              BEGIN
                  SET @ModifiedFrom = GETDATE();
              END
          IF (@ModifiedTo IS NULL)
              BEGIN
                  SET @ModifiedTo = GETDATE();
              END
          INSERT  dbo.UdalExtractsHistory (ExtractDate, ModifiedFrom, ModifiedTo, StartDateTime)
          VALUES                         (@ExtractDate, @ModifiedFrom, @ModifiedTo, GETDATE());
          SELECT @UdalExtractHistoryId = CAST (SCOPE_IDENTITY() AS INT);
          MERGE INTO dbo.UdalExtracts
           AS tgt
          USING (SELECT r.[Id],
                        r.[CalculatedBmiAtRegistration],
                        r.[ConsentForFutureContactForEvaluation],
                        CAST (r.[DateCompletedProgramme] AS DATE) AS [DateCompletedProgramme],
                        r.[DateOfBirth],
                        CAST (r.[DateOfBmiAtRegistration] AS DATE) AS [DateOfBmiAtRegistration],
                        CAST (r.[DateOfProviderContactedServiceUser] AS DATE) AS [DateOfProviderContactedServiceUser],
                        CAST (r.[DateOfProviderSelection] AS DATE) AS [DateOfProviderSelection],
                        CAST (r.[DateOfReferral] AS DATE) AS [DateOfReferral],
                        CAST (r.[DatePlacedOnWaitingList] AS DATE) AS [DatePlacedOnWaitingList],
                        CAST (r.[DateStartedProgramme] AS DATE) AS [DateStartedProgramme],
                        CAST (r.[DateToDelayUntil] AS DATE) AS [DateToDelayUntil],
                        r.[Deprivation],
                        r.[DocumentVersion],
                        r.[Ethnicity],
                        r.[WeightKg] AS [GpRecordedWeight],
                        r.[HasALearningDisability],
                        r.[HasAPhysicalDisability],
                        r.[HasDiabetesType1],
                        r.[HasDiabetesType2],
                        r.[HasHypertension],
                        r.[HasRegisteredSeriousMentalIllness],
                        r.[HeightCm],
                        r.[IsVulnerable],
                        r.[MethodOfContact],
                        CAST (r.[ModifiedAt] AS DATE) AS [ModifiedAt],
                        p.[Name],
                        r.[NhsNumber],
                        r.[NumberOfContacts],
                        r.[OpcsCodes],
                        r.[ProviderUbrn],
                        r.[ReferralSource],
                        r.[ReferringGpPracticeNumber],
                        r.[ReferringOrganisationOdsCode],
                        r.[ServiceUserEthnicityGroup],
                        r.[ServiceUserEthnicity],
                        r.[Sex],
                        r.[SourceSystem],
                        r.[StaffRole],
                        r.[Status],
                        r.[TriagedCompletionLevel],
                        r.[VulnerableDescription]
                 FROM   dbo.Referrals AS r
                        LEFT OUTER JOIN
                        dbo.Providers AS p
                        ON r.ProviderId = p.Id
                 WHERE  r.IsActive = 1
                        AND r.ModifiedAt BETWEEN @ModifiedFrom AND @ModifiedTo) AS src ON tgt.ReferralId = src.Id
          WHEN MATCHED THEN UPDATE 
          SET [ReferralId]                             = src.[Id],
              [UdalExtractHistoryId]                   = @UdalExtractHistoryId,
              [CalculatedBmiAtRegistration]            = src.[CalculatedBmiAtRegistration],
              [ConsentForFutureContactForEvaluation]   = src.[ConsentForFutureContactForEvaluation],
              [DateCompletedProgramme]                 = src.[DateCompletedProgramme],
              [DateOfBirth]                            = src.[DateOfBirth],
              [DateOfBmiAtRegistration]                = src.[DateOfBmiAtRegistration],
              [DateOfProviderContactedServiceUser]     = src.[DateOfProviderContactedServiceUser],
              [DateOfProviderSelection]                = src.[DateOfProviderSelection],
              [DateOfReferral]                         = src.[DateOfReferral],
              [DatePlacedOnWaitingListForElectiveCare] = src.[DatePlacedOnWaitingList],
              [DateStartedProgramme]                   = src.[DateStartedProgramme],
              [DateToDelayUntil]                       = src.[DateToDelayUntil],
              [DeprivationQuintile]                    = src.[Deprivation],
              [DocumentVersion]                        = src.[DocumentVersion],
              [Ethnicity]                              = src.[Ethnicity],
              [EthnicityGroup]                         = src.[ServiceUserEthnicityGroup],
              [EthnicitySubGroup]                      = src.[ServiceUserEthnicity],
              [GpRecordedWeight]                       = src.[GpRecordedWeight],
              [GpSourceSystem]                         = src.[SourceSystem],
              [HasALearningDisability]                 = src.[HasALearningDisability],
              [HasAPhysicalDisability]                 = src.[HasAPhysicalDisability],
              [HasDiabetesType1]                       = src.[HasDiabetesType1],
              [HasDiabetesType2]                       = src.[HasDiabetesType2],
              [HasHypertension]                        = src.[HasHypertension],
              [HasRegisteredSeriousMentalIllness]      = src.[HasRegisteredSeriousMentalIllness],
              [HeightCm]                               = src.[HeightCm],
              [IsVulnerable]                           = src.[IsVulnerable],
              [MethodOfContact]                        = src.[MethodOfContact],
              [ModifiedAt]                             = src.[ModifiedAt],
              [NhsNumber]                              = src.[NhsNumber],
              [NumberOfContacts]                       = src.[NumberOfContacts],
              [OpcsCodesForElectiveCare]               = src.[OpcsCodes],
              [ProviderName]                           = src.[Name],
              [ProviderUbrn]                           = src.[ProviderUbrn],
              [ReferralSource]                         = src.[ReferralSource],
              [ReferringGpPracticeNumber]              = src.[ReferringGpPracticeNumber],
              [ReferringOrganisationOdsCode]           = src.[ReferringOrganisationOdsCode],
              [Sex]                                    = src.[Sex],
              [StaffRole]                              = src.[StaffRole],
              [Status]                                 = src.[Status],
              [TriagedCompletionLevel]                 = src.[TriagedCompletionLevel],
              [VulnerableDescription]                  = src.[VulnerableDescription]
          WHEN NOT MATCHED THEN INSERT ([ReferralId], [UdalExtractHistoryId], [CalculatedBmiAtRegistration], [ConsentForFutureContactForEvaluation], [DateCompletedProgramme], [DateOfBirth], [DateOfBmiAtRegistration], [DateOfProviderContactedServiceUser], [DateOfProviderSelection], [DateOfReferral], [DatePlacedOnWaitingListForElectiveCare], [DateStartedProgramme], [DateToDelayUntil], [DeprivationQuintile], [DocumentVersion], [Ethnicity], [EthnicityGroup], [EthnicitySubGroup], [GpRecordedWeight], [GpSourceSystem], [HasALearningDisability], [HasAPhysicalDisability], [HasDiabetesType1], [HasDiabetesType2], [HasHypertension], [HasRegisteredSeriousMentalIllness], [HeightCm], [IsVulnerable], [MethodOfContact], [ModifiedAt], [NhsNumber], [NumberOfContacts], [OpcsCodesForElectiveCare], [ProviderName], [ProviderUbrn], [ReferralSource], [ReferringGpPracticeNumber], [ReferringOrganisationOdsCode], [Sex], [StaffRole], [Status], [TriagedCompletionLevel], [VulnerableDescription]) VALUES (src.[Id], @UdalExtractHistoryId, src.[CalculatedBmiAtRegistration], src.[ConsentForFutureContactForEvaluation], src.[DateCompletedProgramme], src.[DateOfBirth], src.[DateOfBmiAtRegistration], src.[DateOfProviderContactedServiceUser], src.[DateOfProviderSelection], src.[DateOfReferral], src.[DatePlacedOnWaitingList], src.[DateStartedProgramme], src.[DateToDelayUntil], src.[Deprivation], src.[DocumentVersion], src.[Ethnicity], src.[ServiceUserEthnicityGroup], src.[ServiceUserEthnicity], src.[GpRecordedWeight], src.[SourceSystem], src.[HasALearningDisability], src.[HasAPhysicalDisability], src.[HasDiabetesType1], src.[HasDiabetesType2], src.[HasHypertension], src.[HasRegisteredSeriousMentalIllness], src.[HeightCm], src.[IsVulnerable], src.[MethodOfContact], src.[ModifiedAt], src.[NhsNumber], src.[NumberOfContacts], src.[OpcsCodes], src.[Name], src.[ProviderUbrn], src.[ReferralSource], src.[ReferringGpPracticeNumber], src.[ReferringOrganisationOdsCode], src.[Sex], src.[StaffRole], src.[Status], src.[TriagedCompletionLevel], src.[VulnerableDescription])
          OUTPUT $ACTION INTO @OutputUdalExtracts;
          UPDATE dbo.UdalExtractsHistory
          SET    NumberOfReferralInserts = Counts.NumberOfInserts,
                 NumberOfReferralUpdates = Counts.NumberOfUpdates,
                 EndDateTime             = GETDATE()
          FROM   (SELECT ISNULL(SUM(CASE WHEN [ActionPerformed] = 'INSERT' THEN 1 ELSE 0 END), 0) AS NumberOfInserts,
                         ISNULL(SUM(CASE WHEN [ActionPerformed] = 'UPDATE' THEN 1 ELSE 0 END), 0) AS NumberOfUpdates
                  FROM   @OutputUdalExtracts) AS Counts
          WHERE  Id = @UdalExtractHistoryId;
          DELETE @OutputUdalExtracts;
          MERGE INTO dbo.UdalExtracts
           AS tgt
          USING (SELECT ReferralId,
                        [1] AS WeightMeasurement0007,
                        [2] AS WeightMeasurement0814,
                        [3] AS WeightMeasurement1521,
                        [4] AS WeightMeasurement2228,
                        [5] AS WeightMeasurement2935,
                        [6] AS WeightMeasurement3642,
                        [7] AS WeightMeasurement4349,
                        [8] AS WeightMeasurement5056,
                        [9] AS WeightMeasurement5763,
                        [10] AS WeightMeasurement6470,
                        [11] AS WeightMeasurement7177,
                        [12] AS WeightMeasurement7884,
                        [13] AS WeightMeasurement8500
                 FROM   (SELECT DISTINCT ReferralId,
                                         [Period],
                                         FIRST_VALUE([Weight]) OVER (PARTITION BY [ReferralId], [Period] ORDER BY [Date] DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS LastWeight
                         FROM   (SELECT p.ReferralId,
                                        CAST ([Date] AS DATE) AS [Date],
                                        [Weight],
                                        CASE WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 0 AND 7 THEN 1 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 8 AND 14 THEN 2 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 15 AND 21 THEN 3 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 22 AND 28 THEN 4 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 29 AND 35 THEN 5 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 36 AND 42 THEN 6 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 43 AND 49 THEN 7 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 50 AND 56 THEN 8 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 57 AND 63 THEN 9 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 64 AND 70 THEN 10 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 71 AND 77 THEN 11 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 78 AND 84 THEN 12 ELSE 13 END AS [Period]
                                 FROM   (SELECT   MAX(ModifiedAt) AS ModifiedAt,
                                                  ReferralId,
                                                  CAST ([Date] AS DATE) AS TruncatedDate
                                         FROM     [dbo].[ProviderSubmissions]
                                         WHERE    [Weight] > 0
                                                  AND IsActive = 1
                                         GROUP BY ReferralId, CAST ([Date] AS DATE)) AS pLatest
                                        INNER JOIN
                                        dbo.ProviderSubmissions AS p
                                        ON p.ReferralId = pLatest.ReferralId
                                           AND p.ModifiedAt = pLatest.ModifiedAt
                                           AND CAST (p.Date AS DATE) = pLatest.TruncatedDate
                                        INNER JOIN
                                        (SELECT ReferralId
                                         FROM   dbo.ProviderSubmissions
                                         WHERE  ModifiedAt BETWEEN @ModifiedFrom AND @ModifiedTo
                                                AND IsActive = 1) AS f
                                        ON pLatest.ReferralId = f.ReferralId
                                        INNER JOIN
                                        dbo.Referrals AS r
                                        ON r.Id = p.ReferralId) AS [Periods]) AS [PeriodGroups] PIVOT (MAX (LastWeight) FOR [Period] IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12], [13])) AS pvt) AS src ON tgt.ReferralId = src.ReferralId
          WHEN MATCHED THEN UPDATE 
          SET [WeightMeasurement0007] = src.[WeightMeasurement0007],
              [WeightMeasurement0814] = src.[WeightMeasurement0814],
              [WeightMeasurement1521] = src.[WeightMeasurement1521],
              [WeightMeasurement2228] = src.[WeightMeasurement2228],
              [WeightMeasurement2935] = src.[WeightMeasurement2935],
              [WeightMeasurement3642] = src.[WeightMeasurement3642],
              [WeightMeasurement4349] = src.[WeightMeasurement4349],
              [WeightMeasurement5056] = src.[WeightMeasurement5056],
              [WeightMeasurement5763] = src.[WeightMeasurement5763],
              [WeightMeasurement6470] = src.[WeightMeasurement6470],
              [WeightMeasurement7177] = src.[WeightMeasurement7177],
              [WeightMeasurement7884] = src.[WeightMeasurement7884],
              [WeightMeasurement8500] = src.[WeightMeasurement8500]
          WHEN NOT MATCHED THEN INSERT ([ReferralId], [WeightMeasurement0007], [WeightMeasurement0814], [WeightMeasurement1521], [WeightMeasurement2228], [WeightMeasurement2935], [WeightMeasurement3642], [WeightMeasurement4349], [WeightMeasurement5056], [WeightMeasurement5763], [WeightMeasurement6470], [WeightMeasurement7177], [WeightMeasurement7884], [WeightMeasurement8500], [UdalExtractHistoryId]) VALUES (src.[ReferralId], src.[WeightMeasurement0007], src.[WeightMeasurement0814], src.[WeightMeasurement1521], src.[WeightMeasurement2228], src.[WeightMeasurement2935], src.[WeightMeasurement3642], src.[WeightMeasurement4349], src.[WeightMeasurement5056], src.[WeightMeasurement5763], src.[WeightMeasurement6470], src.[WeightMeasurement7177], src.[WeightMeasurement7884], src.[WeightMeasurement8500], @UdalExtractHistoryId)
          OUTPUT $ACTION INTO @OutputUdalExtracts;
          UPDATE dbo.UdalExtractsHistory
          SET    NumberOfWeightMeasurementInserts = Counts.NumberOfInserts,
                 NumberOfWeightMeasurementUpdates = Counts.NumberOfUpdates,
                 EndDateTime                      = GETDATE()
          FROM   (SELECT ISNULL(SUM(CASE WHEN [ActionPerformed] = 'INSERT' THEN 1 ELSE 0 END), 0) AS NumberOfInserts,
                         ISNULL(SUM(CASE WHEN [ActionPerformed] = 'UPDATE' THEN 1 ELSE 0 END), 0) AS NumberOfUpdates
                  FROM   @OutputUdalExtracts) AS Counts
          WHERE  Id = @UdalExtractHistoryId;
          DELETE @OutputUdalExtracts;
          MERGE INTO dbo.UdalExtracts
           AS tgt
          USING (SELECT ReferralId,
                        [1] AS Coaching0007,
                        [2] AS Coaching0814,
                        [3] AS Coaching1521,
                        [4] AS Coaching2228,
                        [5] AS Coaching2935,
                        [6] AS Coaching3642,
                        [7] AS Coaching4349,
                        [8] AS Coaching5056,
                        [9] AS Coaching5763,
                        [10] AS Coaching6470,
                        [11] AS Coaching7177,
                        [12] AS Coaching7884
                 FROM   (SELECT   ReferralId,
                                  [Period],
                                  1 AS Coaching
                         FROM     (SELECT p.ReferralId,
                                          CASE WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 0 AND 7 THEN 1 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 8 AND 14 THEN 2 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 15 AND 21 THEN 3 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 22 AND 28 THEN 4 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 29 AND 35 THEN 5 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 36 AND 42 THEN 6 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 43 AND 49 THEN 7 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 50 AND 56 THEN 8 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 57 AND 63 THEN 9 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 64 AND 70 THEN 10 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 71 AND 77 THEN 11 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 78 AND 84 THEN 12 ELSE NULL END AS [Period]
                                   FROM   (SELECT   MAX(ModifiedAt) AS ModifiedAt,
                                                    ReferralId,
                                                    CAST ([Date] AS DATE) AS [TruncatedDate]
                                           FROM     [dbo].[ProviderSubmissions]
                                           WHERE    Coaching > 0
                                                    AND IsActive = 1
                                           GROUP BY [ReferralId], CAST ([Date] AS DATE)) AS pLatest
                                          INNER JOIN
                                          dbo.ProviderSubmissions AS p
                                          ON p.ReferralId = pLatest.ReferralId
                                             AND p.ModifiedAt = pLatest.ModifiedAt
                                             AND CAST (p.Date AS DATE) = pLatest.TruncatedDate
                                          INNER JOIN
                                          (SELECT ReferralId
                                           FROM   dbo.ProviderSubmissions
                                           WHERE  ModifiedAt BETWEEN @ModifiedFrom AND @ModifiedTo
                                                  AND IsActive = 1) AS f
                                          ON pLatest.ReferralId = f.ReferralId
                                          INNER JOIN
                                          dbo.Referrals AS r
                                          ON r.Id = p.ReferralId) AS [Periods]
                         GROUP BY ReferralId, Period) AS [PeriodGroups] PIVOT (COUNT (Coaching) FOR [Period] IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])) AS pvt) AS src ON tgt.ReferralId = src.ReferralId
          WHEN MATCHED THEN UPDATE 
          SET [Coaching0007] = src.[Coaching0007],
              [Coaching0814] = src.[Coaching0814],
              [Coaching1521] = src.[Coaching1521],
              [Coaching2228] = src.[Coaching2228],
              [Coaching2935] = src.[Coaching2935],
              [Coaching3642] = src.[Coaching3642],
              [Coaching4349] = src.[Coaching4349],
              [Coaching5056] = src.[Coaching5056],
              [Coaching5763] = src.[Coaching5763],
              [Coaching6470] = src.[Coaching6470],
              [Coaching7177] = src.[Coaching7177],
              [Coaching7884] = src.[Coaching7884]
          WHEN NOT MATCHED THEN INSERT ([ReferralId], [Coaching0007], [Coaching0814], [Coaching1521], [Coaching2228], [Coaching2935], [Coaching3642], [Coaching4349], [Coaching5056], [Coaching5763], [Coaching6470], [Coaching7177], [Coaching7884], [UdalExtractHistoryId]) VALUES (src.[ReferralId], src.[Coaching0007], src.[Coaching0814], src.[Coaching1521], src.[Coaching2228], src.[Coaching2935], src.[Coaching3642], src.[Coaching4349], src.[Coaching5056], src.[Coaching5763], src.[Coaching6470], src.[Coaching7177], src.[Coaching7884], @UdalExtractHistoryId)
          OUTPUT $ACTION INTO @OutputUdalExtracts;
          UPDATE dbo.UdalExtractsHistory
          SET    NumberOfCoachingInserts = Counts.NumberOfInserts,
                 NumberOfCoachingUpdates = Counts.NumberOfUpdates,
                 EndDateTime             = GETDATE()
          FROM   (SELECT ISNULL(SUM(CASE WHEN [ActionPerformed] = 'INSERT' THEN 1 ELSE 0 END), 0) AS NumberOfInserts,
                         ISNULL(SUM(CASE WHEN [ActionPerformed] = 'UPDATE' THEN 1 ELSE 0 END), 0) AS NumberOfUpdates
                  FROM   @OutputUdalExtracts) AS Counts
          WHERE  Id = @UdalExtractHistoryId;
          DELETE @OutputUdalExtracts;
          MERGE INTO dbo.UdalExtracts
           AS tgt
          USING (SELECT ReferralId,
                        [1] AS ProviderEngagement0007,
                        [2] AS ProviderEngagement0814,
                        [3] AS ProviderEngagement1521,
                        [4] AS ProviderEngagement2228,
                        [5] AS ProviderEngagement2935,
                        [6] AS ProviderEngagement3642,
                        [7] AS ProviderEngagement4349,
                        [8] AS ProviderEngagement5056,
                        [9] AS ProviderEngagement5763,
                        [10] AS ProviderEngagement6470,
                        [11] AS ProviderEngagement7177,
                        [12] AS ProviderEngagement7884
                 FROM   (SELECT   ReferralId,
                                  [Period],
                                  1 AS Measure
                         FROM     (SELECT p.ReferralId,
                                          CASE WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 0 AND 7 THEN 1 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 8 AND 14 THEN 2 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 15 AND 21 THEN 3 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 22 AND 28 THEN 4 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 29 AND 35 THEN 5 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 36 AND 42 THEN 6 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 43 AND 49 THEN 7 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 50 AND 56 THEN 8 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 57 AND 63 THEN 9 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 64 AND 70 THEN 10 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 71 AND 77 THEN 11 WHEN DATEDIFF(DAY, CAST (r.DateStartedProgramme AS DATE), CAST (p.[Date] AS DATE)) BETWEEN 78 AND 84 THEN 12 ELSE NULL END AS [Period]
                                   FROM   (SELECT   MAX(ModifiedAt) AS ModifiedAt,
                                                    ReferralId,
                                                    CAST ([Date] AS DATE) AS [TruncatedDate]
                                           FROM     [dbo].[ProviderSubmissions]
                                           WHERE    Measure > 0
                                                    AND IsActive = 1
                                           GROUP BY [ReferralId], CAST ([Date] AS DATE)) AS pLatest
                                          INNER JOIN
                                          dbo.ProviderSubmissions AS p
                                          ON p.ReferralId = pLatest.ReferralId
                                             AND p.ModifiedAt = pLatest.ModifiedAt
                                             AND CAST (p.Date AS DATE) = pLatest.TruncatedDate
                                          INNER JOIN
                                          (SELECT ReferralId
                                           FROM   dbo.ProviderSubmissions
                                           WHERE  ModifiedAt BETWEEN @ModifiedFrom AND @ModifiedTo
                                                  AND IsActive = 1) AS f
                                          ON pLatest.ReferralId = f.ReferralId
                                          INNER JOIN
                                          dbo.Referrals AS r
                                          ON r.Id = p.ReferralId) AS [Periods]
                         GROUP BY ReferralId, Period) AS [PeriodGroups] PIVOT (COUNT (Measure) FOR [Period] IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])) AS pvt) AS src ON tgt.ReferralId = src.ReferralId
          WHEN MATCHED THEN UPDATE 
          SET [ProviderEngagement0007] = src.[ProviderEngagement0007],
              [ProviderEngagement0814] = src.[ProviderEngagement0814],
              [ProviderEngagement1521] = src.[ProviderEngagement1521],
              [ProviderEngagement2228] = src.[ProviderEngagement2228],
              [ProviderEngagement2935] = src.[ProviderEngagement2935],
              [ProviderEngagement3642] = src.[ProviderEngagement3642],
              [ProviderEngagement4349] = src.[ProviderEngagement4349],
              [ProviderEngagement5056] = src.[ProviderEngagement5056],
              [ProviderEngagement5763] = src.[ProviderEngagement5763],
              [ProviderEngagement6470] = src.[ProviderEngagement6470],
              [ProviderEngagement7177] = src.[ProviderEngagement7177],
              [ProviderEngagement7884] = src.[ProviderEngagement7884]
          WHEN NOT MATCHED THEN INSERT ([ReferralId], [ProviderEngagement0007], [ProviderEngagement0814], [ProviderEngagement1521], [ProviderEngagement2228], [ProviderEngagement2935], [ProviderEngagement3642], [ProviderEngagement4349], [ProviderEngagement5056], [ProviderEngagement5763], [ProviderEngagement6470], [ProviderEngagement7177], [ProviderEngagement7884], [UdalExtractHistoryId]) VALUES (src.[ReferralId], src.[ProviderEngagement0007], src.[ProviderEngagement0814], src.[ProviderEngagement1521], src.[ProviderEngagement2228], src.[ProviderEngagement2935], src.[ProviderEngagement3642], src.[ProviderEngagement4349], src.[ProviderEngagement5056], src.[ProviderEngagement5763], src.[ProviderEngagement6470], src.[ProviderEngagement7177], src.[ProviderEngagement7884], @UdalExtractHistoryId)
          OUTPUT $ACTION INTO @OutputUdalExtracts;
          UPDATE dbo.UdalExtractsHistory
          SET    NumberOfProviderEngagementInserts = Counts.NumberOfInserts,
                 NumberOfProviderEngagementUpdates = Counts.NumberOfUpdates,
                 EndDateTime                       = GETDATE()
          FROM   (SELECT ISNULL(SUM(CASE WHEN [ActionPerformed] = 'INSERT' THEN 1 ELSE 0 END), 0) AS NumberOfInserts,
                         ISNULL(SUM(CASE WHEN [ActionPerformed] = 'UPDATE' THEN 1 ELSE 0 END), 0) AS NumberOfUpdates
                  FROM   @OutputUdalExtracts) AS Counts
          WHERE  Id = @UdalExtractHistoryId;
          DELETE @OutputUdalExtracts;
          UPDATE dbo.UdalExtracts
          SET    ModifiedAt = ps.ModifiedAt
          FROM   (SELECT   ReferralId,
                           MAX(ModifiedAt) AS ModifiedAt
                  FROM     dbo.ProviderSubmissions
                  GROUP BY ReferralId) AS ps
                 INNER JOIN
                 dbo.UdalExtracts AS ue
                 ON ps.ReferralId = ue.ReferralId
          WHERE  ps.ModifiedAt > ISNULL(ue.ModifiedAt, '1900-01-01');
          UPDATE dbo.UdalExtractsHistory
          SET    NumberOfModifiedUpdates = @@ROWCOUNT
	      WHERE  Id = @UdalExtractHistoryId;
      END");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "UdalExtracts");

    migrationBuilder.DropTable(
        name: "UdalExtractsHistory");

    migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_UdalExtractsMerge]");
  }
}
