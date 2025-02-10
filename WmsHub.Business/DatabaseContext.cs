using Audit.Core;
using Audit.EntityFramework;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Threading;
using WmsHub.Business.Entities;
using WmsHub.Business.Entities.ErsMock;

namespace WmsHub.Business;

// use this to create a idempotent script with always encrypted
// dotnet ef migrations script -i -o WmsHub_Database_Creation.sql

// use this to create a idempotent script without always encrypted
// dotnet ef migrations script -i --configuration DebugNoAE
// -o WmsHub_Database_Creation_NoAE.sql

public class DatabaseContext : AuditDbContext
{
  internal const string CEK = "CEK_WmsHub";

  protected readonly string _connectionString = Environment
    .GetEnvironmentVariable("SQLCONNSTR_WmsHub");

  private static int _timesInitializeAzureKeyVaultProviderCalled = 0;

  private readonly static string _alwaysEncryptedTenantId = Environment
    .GetEnvironmentVariable("WmsHub_AlwaysEncrypted:TenantId");
  private readonly static string _alwaysEncryptedClientId = Environment
    .GetEnvironmentVariable("WmsHub_AlwaysEncrypted:ClientId");
  private readonly static string _alwaysEncryptedClientSecret =
    Environment.GetEnvironmentVariable("WmsHub_AlwaysEncrypted:ClientSecret");
  private static readonly string _alwaysEncryptedIsEnabled = Environment
    .GetEnvironmentVariable("WmsHub_AlwaysEncrypted:IsEnabled");

  public DatabaseContext() : base()
  {
    ConfigureAudit();
  }

  public DatabaseContext(string connectionString) : base()
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new ArgumentNullException(nameof(connectionString));

    _connectionString = connectionString;

    InitializeAzureKeyVaultProvider();
    ConfigureAudit();
  }

  public DatabaseContext(DbContextOptions<DatabaseContext> options)
    : base(options)
  {
    InitializeAzureKeyVaultProvider();
    ConfigureAudit();
  }

  public virtual DbSet<Call> Calls { get; set; }
  public virtual DbSet<CallAudit> CallsAudit { get; set; }
  public virtual DbSet<Deprivation> Deprivations { get; set; }
  public virtual DbSet<DeprivationAudit> DeprivationsAudit { get; set; }
  public virtual DbSet<ElectiveCareReferral> ElectiveCareReferrals { get; set; }
  public virtual DbSet<Ethnicity> Ethnicities { get; set; }
  public virtual DbSet<EthnicityAudit> EthnicitiesAudit { get; set; }
  public virtual DbSet<EthnicityOverride> EthnicityOverrides { get; set; }
  public virtual DbSet<EthnicityOverrideAudit> EthnicityOverridesAudit
   { get; set; }
  public virtual DbSet<StaffRole> StaffRoles { get; set; }
  public virtual DbSet<StaffRoleAudit> StaffRolesAudit { get; set; }
  public virtual DbSet<Log> Logs { get; set; }
  public virtual DbSet<MskOrganisation> MskOrganisations { get; set; }
  public virtual DbSet<MskOrganisationAudit> MskOrganisationsAudit
    { get; set; }
  public virtual DbSet<Practice> Practices { get; set; }
  public virtual DbSet<PracticeAudit> PracticesAudit { get; set; }
  public virtual DbSet<Provider> Providers { get; set; }
  public virtual DbSet<ProviderAudit> ProvidersAudit { get; set; }
  public virtual DbSet<ProviderAuth> ProviderAuth { get; set; }
  public virtual DbSet<ProviderAuthAudit> ProviderAuthAudit { get; set; }
  public virtual DbSet<ProviderDetail> ProviderDetails { get; set; }
  public virtual DbSet<ProviderDetailAudit> ProviderDetailsAudit { get; set; }
  public virtual DbSet<ProviderSubmission> ProviderSubmissions { get; set; }
  public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
  public virtual DbSet<RefreshTokenAudit> RefreshTokensAudit { get; set; }
  public virtual DbSet<ProviderSubmissionAudit> ProviderSubmissionsAudit
   { get; set; }
  public virtual DbSet<Referral> Referrals { get; set; }
  public virtual DbSet<ReferralAudit> ReferralsAudit { get; set; }
  public virtual DbSet<RequestResponseLog> RequestResponseLog { get; set; }
  public virtual DbSet<TextMessage> TextMessages { get; set; }
  public virtual DbSet<TextMessageAudit> TextMessagesAudit { get; set; }
  public virtual DbSet<UserStore> UsersStore { get; set; }
  public virtual DbSet<UserStoreAudit> UsersStoreAudit { get; set; }
  public virtual DbSet<ReferralCri> ReferralCri { get; set; }
  public virtual DbSet<ReferralCriAudit> ReferralCriAudit { get; set; }
  public virtual DbSet<Analytics> Analytics { get; set; }
  public virtual DbSet<AnalyticsAudit> AnalyticsAudit { get; set; }
  public virtual DbSet<SelfReferral> SelfReferrals { get; set; }
  public virtual DbSet<ReferralStatusReason> ReferralStatusReasons
    { get; set; }
  public virtual DbSet<ReferralStatusReasonAudit> ReferralStatusReasonsAudit  
    { get; set; }
  public virtual DbSet<PatientTriage> PatientTriages { get; set; }
  public virtual DbSet<PatientTriageAudit> PatientTriagesAudit { get; set; }
  public virtual DbSet<PharmacyReferral> PharmacyReferrals { get; set; }
  public virtual DbSet<Pharmacist> Pharmacists { get; set; }
  public virtual DbSet<PharmacistAudit> PharmacistsAudit { get; set; }
  public virtual DbSet<Pharmacy> Pharmacies { get; set; }
  public virtual DbSet<PharmacyAudit> PharmaciesAudit { get; set; }
  public virtual DbSet<ApiKeyStore> ApiKeyStore { get; set; }
  public virtual DbSet<ApiKeyStoreAudit> ApiKeyStoreAudit { get; set; }
  public virtual DbSet<GeneralReferral> GeneralReferrals { get; set; }
  public virtual DbSet<UserActionLog> UserActionLogs { get; set; }
  public virtual DbSet<ConfigurationValue> ConfigurationValues { get; set; }
  public virtual DbSet<MskReferral> MskReferrals { get; set; }
  public virtual DbSet<GpReferral> GpReferrals { get; set; }
  public virtual DbSet<Questionnaire> Questionnaires { get; set; }
  public virtual DbSet<QuestionnaireAudit> QuestionnairesAudit { get; set; }
  public virtual DbSet<ReferralQuestionnaire> ReferralQuestionnaires 
    { get; set; }
  public virtual DbSet<ReferralQuestionnaireAudit> ReferralQuestionnairesAudit 
    { get; set; }
  public virtual DbSet<MessageQueue> MessagesQueue { get; set; }
  public virtual DbSet<AccessKey> AccessKeys { get; set; }

  public virtual DbSet<Organisation> Organisations { get; set; }
  public virtual DbSet<OrganisationAudit> OrganisationsAudit { get; set; }

  public virtual DbSet<ErsMockReferral> ErsMockReferrals { get; set; }

  public virtual DbSet<ElectiveCarePostError> ElectiveCarePostErrors
  { get; set; }

  public virtual DbSet<LinkId> LinkIds { get; set; }
 
  public virtual DbSet<UdalExtractsHistory> UdalExtractsHistory { get; set; }

  public virtual DbSet<UdalExtract> UdalExtracts { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder options)
  {
    if (!options?.IsConfigured ?? false)
      options.UseSqlServer(_connectionString);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<ConfigurationValue>()
      .HasKey(x => x.Id);
    // encrypted
    modelBuilder.Entity<ConfigurationValue>()
      .Property(x => x.Value)
      .HasMaxLength(4000);

    SetGuidKeyOnId<Call>(modelBuilder);
    // encrypted
    modelBuilder.Entity<Call>()
      .Property(c => c.Number).HasMaxLength(200);
    modelBuilder.Entity<CallAudit>().HasKey(c => c.AuditId);
    // encrypted
    modelBuilder.Entity<CallAudit>()
      .Property(c => c.Number).HasMaxLength(200);

    SetGuidKeyOnId<Deprivation>(modelBuilder);
    modelBuilder.Entity<Deprivation>().HasIndex(d => d.Lsoa);
    modelBuilder.Entity<Deprivation>()
      .Property(d => d.Lsoa).HasMaxLength(200);
    modelBuilder.Entity<DeprivationAudit>().HasKey(d => d.AuditId);

    SetGuidKeyOnId<Ethnicity>(modelBuilder);

    modelBuilder.Entity<Ethnicity>()
      .Property(p => p.MinimumBmi).HasPrecision(18, 2);
    modelBuilder.Entity<Ethnicity>()
      .HasMany(e => e.Overrides)
      .WithOne(o => o.Ethnicity)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<EthnicityAudit>().HasKey(c => c.AuditId);
    modelBuilder.Entity<EthnicityAudit>()
      .Property(p => p.MinimumBmi).HasPrecision(18, 2);

    SetGuidKeyOnId<EthnicityOverride>(modelBuilder);
    modelBuilder.Entity<EthnicityOverride>()
      .Property(e => e.ReferralSource)
      .HasConversion<string>()
      .HasMaxLength(200);

    modelBuilder.Entity<EthnicityOverrideAudit>()
      .HasKey(c => c.AuditId);
    modelBuilder.Entity<EthnicityOverrideAudit>()
      .Property(e => e.ReferralSource)
      .HasConversion<string>()
      .HasMaxLength(200);

    SetGuidKeyOnId<StaffRole>(modelBuilder);
    modelBuilder.Entity<StaffRoleAudit>().HasKey(c => c.AuditId);

    SetGuidKeyOnId<Provider>(modelBuilder);
    modelBuilder.Entity<ProviderAudit>().HasKey(c => c.AuditId);
    modelBuilder.Entity<Provider>()
      .Property(p => p.Name).HasMaxLength(100);
    modelBuilder.Entity<Provider>();

    SetGuidKeyOnId<ProviderSubmission>(modelBuilder);
    modelBuilder.Entity<ProviderSubmissionAudit>().HasKey(c => c.AuditId);

    SetGuidKeyOnId<RefreshToken>(modelBuilder);
    modelBuilder.Entity<RefreshTokenAudit>().HasKey(c => c.AuditId);

    SetGuidKeyOnId<Practice>(modelBuilder);
    modelBuilder.Entity<Practice>()
      .Property(p => p.Email).HasMaxLength(200);
    modelBuilder.Entity<Practice>()
      .Property(p => p.Name).HasMaxLength(200);
    modelBuilder.Entity<Practice>().HasIndex(p => p.OdsCode).IsUnique();
    modelBuilder.Entity<PracticeAudit>().HasKey(p => p.AuditId);
    modelBuilder.Entity<PracticeAudit>()
      .Property(p => p.Email).HasMaxLength(200);
    modelBuilder.Entity<PracticeAudit>()
      .Property(p => p.Name).HasMaxLength(200);

    modelBuilder.Entity<Provider>()
     .HasOne(a => a.ProviderAuth)
     .WithOne(a => a.Provider)
     .HasForeignKey<ProviderAuth>(c => c.Id);

    modelBuilder.Entity<Provider>()
     .HasMany(x => x.Details)
     .WithOne(x => x.Provider)
     .OnDelete(DeleteBehavior.NoAction);

    SetGuidKeyOnId<ProviderAuth>(modelBuilder);
    modelBuilder.Entity<ProviderAuthAudit>().HasKey(c => c.AuditId);
    modelBuilder.Entity<ProviderAuthAudit>()
     .Property(p => p.MobileNumber).HasMaxLength(200);
    modelBuilder.Entity<ProviderAuthAudit>()
     .Property(p => p.EmailContact).HasMaxLength(200);
    modelBuilder.Entity<ProviderAuthAudit>()
     .Property(r => r.IpWhitelist).HasMaxLength(200);
    modelBuilder.Entity<ProviderAuth>()
     .Property(r => r.MobileNumber).HasMaxLength(200);
    modelBuilder.Entity<ProviderAuth>()
     .Property(r => r.EmailContact).HasMaxLength(200);
    modelBuilder.Entity<ProviderAuth>()
     .Property(r => r.IpWhitelist).HasMaxLength(200);

    SetGuidKeyOnId<ProviderDetail>(modelBuilder);
    modelBuilder.Entity<ProviderDetail>()
     .Property(x => x.Section).HasMaxLength(200);
    modelBuilder.Entity<ProviderDetail>()
     .Property(x => x.Value).HasMaxLength(200);
    modelBuilder.Entity<ProviderDetailAudit>().HasKey(c => c.AuditId);

    SetGuidKeyOnId<Referral>(modelBuilder);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Status).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ReferringGpPracticeNumber).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Sex).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Ethnicity).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Deprivation).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.TriagedCompletionLevel).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.TriagedWeightedLevel).HasMaxLength(200);
    // encrypted
    modelBuilder.Entity<Referral>().HasIndex(r => r.Ubrn);
    modelBuilder.Entity<Referral>()
      .Property(r => r.NhsNumber).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.NhsNumber);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Ubrn).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.FamilyName);
    modelBuilder.Entity<Referral>()
      .Property(r => r.FamilyName).HasMaxLength(200).IsUnicode(true);
    modelBuilder.Entity<Referral>()
      .Property(r => r.GivenName).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.Postcode);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Postcode).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Address1).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Address2).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Address3).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.Telephone);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Telephone).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.Mobile);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Mobile).HasMaxLength(200);
    modelBuilder.Entity<Referral>().HasIndex(r => r.Email);
    modelBuilder.Entity<Referral>()
      .Property(r => r.Email).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ServiceUserEthnicity).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ServiceUserEthnicityGroup).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.DelayReason).HasMaxLength(2000);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ReferringOrganisationEmail).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ReferringOrganisationOdsCode).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.NhsLoginClaimEmail).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.NhsLoginClaimFamilyName).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.NhsLoginClaimGivenName).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.NhsLoginClaimMobile).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ServiceId).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.SourceSystem).HasMaxLength(200).HasConversion<string>()
      .IsRequired(false);
    modelBuilder.Entity<Referral>()
      .Property(r => r.DocumentVersion).IsRequired(false);
    modelBuilder.Entity<Referral>()
      .HasMany(r => r.Audits)
      .WithOne(a => a.Referral)
      .HasForeignKey(a => a.Id);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ReferringClinicianEmail).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.CreatedByUserId).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ProviderUbrn).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.OpcsCodes).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.SourceEthnicity).HasMaxLength(200);
    modelBuilder.Entity<Referral>()
      .Property(r => r.SpellIdentifier).HasMaxLength(20);
    modelBuilder.Entity<Referral>()
      .Property(r => r.ReferralAttachmentId).HasMaxLength(50);
    modelBuilder.Entity<Referral>()
      .Property(r => r.HeightUnits)
      .HasMaxLength(200)
      .HasConversion<string>()
      .IsRequired(false);
    modelBuilder.Entity<Referral>()
      .Property(r => r.WeightUnits)
      .HasMaxLength(200)
      .HasConversion<string>()
      .IsRequired(false);

    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Status).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ReferringGpPracticeNumber).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Sex).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Ethnicity).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Deprivation).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.TriagedCompletionLevel).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.TriagedWeightedLevel).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.DelayReason).HasMaxLength(2000);
    // encrypted
    modelBuilder.Entity<ReferralAudit>().HasKey(ra => ra.AuditId);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.NhsNumber).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Ubrn).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.FamilyName).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.GivenName).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Postcode).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Address1).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Address2).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Address3).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Telephone).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Mobile).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.Email).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ServiceUserEthnicity).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ServiceUserEthnicityGroup).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ReferringOrganisationEmail).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ReferringOrganisationOdsCode).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.NhsLoginClaimEmail).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.NhsLoginClaimFamilyName).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.NhsLoginClaimGivenName).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.NhsLoginClaimMobile).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ServiceId).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.SourceSystem).HasMaxLength(200).HasConversion<string>()
      .IsRequired(false);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.DocumentVersion).IsRequired(false);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ReferringClinicianEmail).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.CreatedByUserId).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ProviderUbrn).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.OpcsCodes).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.SourceEthnicity).HasMaxLength(200);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.SpellIdentifier).HasMaxLength(20);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.ReferralAttachmentId).HasMaxLength(50);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.HeightUnits)
      .HasMaxLength(200)
      .HasConversion<string>()
      .IsRequired(false);
    modelBuilder.Entity<ReferralAudit>()
      .Property(r => r.WeightUnits)
      .HasMaxLength(200)
      .HasConversion<string>()
      .IsRequired(false);

    SetGuidKeyOnId<TextMessage>(modelBuilder);
    // encrypted
    modelBuilder.Entity<TextMessage>()
      .Property(c => c.Number).HasMaxLength(200);
    modelBuilder.Entity<TextMessage>()
      .Property(r => r.ServiceUserLinkId).HasMaxLength(20);
    modelBuilder.Entity<TextMessage>()
      .HasIndex(r => r.ServiceUserLinkId);
    modelBuilder.Entity<TextMessage>()
      .Property(t => t.ReferralStatus).HasMaxLength(200);
    modelBuilder.Entity<TextMessageAudit>().HasKey(t => t.AuditId);
    // encrypted
    modelBuilder.Entity<TextMessageAudit>()
      .Property(c => c.Number).HasMaxLength(200);
    modelBuilder.Entity<TextMessageAudit>()
      .Property(r => r.ServiceUserLinkId).HasMaxLength(20);
    modelBuilder.Entity<TextMessageAudit>()
      .Property(r => r.ReferralStatus).HasMaxLength(200);

    SetGuidKeyOnId<UserStore>(modelBuilder);
    modelBuilder.Entity<UserStore>()
     .Property(t => t.OwnerName).HasMaxLength(200);
    modelBuilder.Entity<UserStore>()
     .Property(t => t.ApiKey).HasMaxLength(200);
    modelBuilder.Entity<UserStore>()
     .Property(t => t.Domain).HasMaxLength(200);
    modelBuilder.Entity<UserStore>()
     .HasMany(u => u.ReferralAudits)
     .WithOne(ra => ra.User)
     .HasForeignKey(ra => ra.ModifiedByUserId)
     .IsRequired(false)
     .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<UserStoreAudit>().HasKey(t => t.AuditId);
    modelBuilder.Entity<UserStoreAudit>()
     .Property(t => t.OwnerName).HasMaxLength(200);
    modelBuilder.Entity<UserStoreAudit>()
     .Property(t => t.ApiKey).HasMaxLength(200);
    modelBuilder.Entity<UserStoreAudit>()
     .Property(t => t.Domain).HasMaxLength(200);

    SetGuidKeyOnId<ReferralCri>(modelBuilder);
    modelBuilder.Entity<ReferralCriAudit>().HasKey(t => t.AuditId);

    SetGuidKeyOnId<Analytics>(modelBuilder);
    modelBuilder.Entity<Analytics>()
      .Property(t => t.LinkDescription).HasMaxLength(200);
    modelBuilder.Entity<Analytics>()
      .Property(t => t.LinkDescription).IsRequired();
    modelBuilder.Entity<Analytics>()
      .Property(t => t.LinkId).IsRequired();
    modelBuilder.Entity<AnalyticsAudit>().HasKey(t => t.AuditId);

    SetGuidKeyOnId<ReferralStatusReason>(modelBuilder);
    modelBuilder.Entity<ReferralStatusReason>()
      .Property(t => t.Description).HasMaxLength(500);

    modelBuilder.Entity<ReferralStatusReasonAudit>()
      .HasKey(t => t.AuditId);
    modelBuilder.Entity<ReferralStatusReasonAudit>()
      .Property(t => t.Description).HasMaxLength(500);

    modelBuilder.Entity<PatientTriage>()
      .HasKey(t => t.Id);
    modelBuilder.Entity<PatientTriage>()
        .Property(t => t.TriageSection).HasMaxLength(200);
    modelBuilder.Entity<PatientTriage>()
      .Property(t => t.TriageSection).IsRequired();
    modelBuilder.Entity<PatientTriage>()
      .Property(t => t.Key).IsRequired();
    modelBuilder.Entity<PatientTriage>()
      .Property(t => t.TriageSection).HasMaxLength(50);
    modelBuilder.Entity<PatientTriage>()
      .Property(t => t.Descriptions).HasMaxLength(200);
    modelBuilder.Entity<PatientTriage>()
      .Property(t => t.Descriptions).IsRequired();
    modelBuilder.Entity<PatientTriageAudit>()
      .HasKey(t => t.AuditId);
    modelBuilder.Entity<PatientTriageAudit>()
      .Property(t => t.TriageSection).HasMaxLength(200);
    modelBuilder.Entity<PatientTriageAudit>()
      .Property(t => t.TriageSection).IsRequired();
    modelBuilder.Entity<PatientTriageAudit>()
      .Property(t => t.Key).IsRequired();
    modelBuilder.Entity<PatientTriageAudit>()
      .Property(t => t.Descriptions).HasMaxLength(200);
    modelBuilder.Entity<PatientTriageAudit>()
      .Property(t => t.Descriptions).IsRequired();

    modelBuilder.Entity<Pharmacist>()
      .HasKey(t => t.Id);
    modelBuilder.Entity<Pharmacist>()
      .Property(t => t.ReferringPharmacyEmail).HasMaxLength(200);
    modelBuilder.Entity<Pharmacist>()
      .Property(t => t.KeyCode).HasMaxLength(20);
    modelBuilder.Entity<PharmacistAudit>()
      .HasKey(t => t.AuditId);
    modelBuilder.Entity<PharmacistAudit>()
      .Property(t => t.ReferringPharmacyEmail).HasMaxLength(200);
    modelBuilder.Entity<PharmacistAudit>()
      .Property(t => t.KeyCode).HasMaxLength(20);

    SetGuidKeyOnId<Pharmacy>(modelBuilder);
    modelBuilder.Entity<Pharmacy>()
      .Property(p => p.Email).HasMaxLength(200);
    modelBuilder.Entity<Pharmacy>()
      .Property(p => p.TemplateVersion).HasMaxLength(5);
    modelBuilder.Entity<Pharmacy>().HasIndex(p => p.OdsCode).IsUnique();
    modelBuilder.Entity<PharmacyAudit>().HasKey(p => p.AuditId);
    modelBuilder.Entity<PharmacyAudit>()
      .Property(p => p.Email).HasMaxLength(200);
    modelBuilder.Entity<PharmacyAudit>()
      .Property(p => p.TemplateVersion).HasMaxLength(5);

    SetGuidKeyOnId<ApiKeyStore>(modelBuilder);
    modelBuilder.Entity<ApiKeyStore>()
      .Property(r => r.Key).HasMaxLength(500);
    modelBuilder.Entity<ApiKeyStore>()
      .Property(r => r.Sid).HasMaxLength(200);
    modelBuilder.Entity<ApiKeyStore>()
      .Property(r => r.KeyUser).HasMaxLength(500);
    modelBuilder.Entity<ApiKeyStore>()
      .Property(r => r.Domains).HasMaxLength(500);
    modelBuilder.Entity<ApiKeyStoreAudit>().HasKey(c => c.AuditId);
    modelBuilder.Entity<ApiKeyStoreAudit>()
      .Property(r => r.Key).HasMaxLength(500);
    modelBuilder.Entity<ApiKeyStoreAudit>()
      .Property(r => r.Sid).HasMaxLength(200);
    modelBuilder.Entity<ApiKeyStoreAudit>()
      .Property(r => r.KeyUser).HasMaxLength(500);
    modelBuilder.Entity<ApiKeyStoreAudit>()
      .Property(r => r.Domains).HasMaxLength(500);

    modelBuilder.Entity<UserActionLog>()
      .Property(u => u.Action).HasMaxLength(200);
    modelBuilder.Entity<UserActionLog>()
      .Property(u => u.Controller).HasMaxLength(200);
    modelBuilder.Entity<UserActionLog>()
      .Property(u => u.IpAddress).HasMaxLength(200);
    modelBuilder.Entity<UserActionLog>()
      .Property(u => u.Method).HasMaxLength(200);
    modelBuilder.Entity<UserActionLog>()
      .Property(u => u.Request).HasMaxLength(4000);

    SetGuidKeyOnId<Questionnaire>(modelBuilder);
    modelBuilder.Entity<Questionnaire>()
      .Property(p => p.Type)
      .HasConversion(new EnumToStringConverter<Enums.QuestionnaireType>());
    modelBuilder.Entity<Questionnaire>()
      .Property(p => p.Type).HasMaxLength(200);
    modelBuilder.Entity<Questionnaire>()
      .HasIndex(p => p.Type).IsUnique();
    modelBuilder.Entity<Questionnaire>()
      .HasMany(r => r.ReferralQuestionnaires)
      .WithOne(a => a.Questionnaire)
      .HasForeignKey(a => a.QuestionnaireId);

    modelBuilder.Entity<QuestionnaireAudit>().HasKey(ra => ra.AuditId);
    modelBuilder.Entity<QuestionnaireAudit>()
      .Property(p => p.Type)
      .HasConversion(new EnumToStringConverter<Enums.QuestionnaireType>());
    modelBuilder.Entity<QuestionnaireAudit>()
      .Property(p => p.Type).HasMaxLength(200);

    SetGuidKeyOnId<ReferralQuestionnaire>(modelBuilder);
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(p => p.NotificationKey).HasMaxLength(200);
    modelBuilder.Entity<ReferralQuestionnaire>()
      .HasIndex(p => p.NotificationKey);
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(p => p.Answers).HasColumnType("nvarchar(max)");
    modelBuilder.Entity<Referral>()
      .HasOne(a => a.ReferralQuestionnaire)
      .WithOne(a => a.Referral)
      .HasForeignKey<ReferralQuestionnaire>(a => a.ReferralId);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(r => r.FamilyName)
      .HasMaxLength(200)
      .IsUnicode(true);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(r => r.GivenName)
      .HasMaxLength(200);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(r => r.Mobile)
      .HasMaxLength(200);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(r => r.Email)
      .HasMaxLength(200);
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(p => p.Status)
      .HasConversion(
        new EnumToStringConverter<Enums.ReferralQuestionnaireStatus>());
    modelBuilder.Entity<ReferralQuestionnaire>()
      .Property(p => p.Status).HasMaxLength(200);

    modelBuilder.Entity<ReferralQuestionnaireAudit>().HasKey(ra => ra.AuditId);
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(p => p.NotificationKey).HasMaxLength(200);
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(p => p.Answers).HasColumnType("nvarchar(max)");
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(r => r.FamilyName)
      .HasMaxLength(200)
      .IsUnicode(true);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(r => r.GivenName)
      .HasMaxLength(200);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(r => r.Mobile)
      .HasMaxLength(200);
    // Encrypted.
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(r => r.Email)
      .HasMaxLength(200);
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(p => p.Status)
      .HasConversion(
        new EnumToStringConverter<Enums.ReferralQuestionnaireStatus>());
    modelBuilder.Entity<ReferralQuestionnaireAudit>()
      .Property(p => p.Status).HasMaxLength(200);

    modelBuilder.Entity<AccessKey>()
      .Property(p => p.Type)
      .HasConversion(
        new EnumToStringConverter<Enums.AccessKeyType>());
    modelBuilder.Entity<AccessKey>()
      .Property(x => x.Email)
      .HasMaxLength(200);
    modelBuilder.Entity<AccessKey>()
      .Property(x => x.Key)
      .HasMaxLength(200);
    modelBuilder.Entity<AccessKey>()
      .Property(x => x.Type)
      .HasMaxLength(200);

    SetGuidKeyOnId<MessageQueue>(modelBuilder);
    modelBuilder.Entity<MessageQueue>()
      .HasKey(t => t.Id);
    modelBuilder.Entity<MessageQueue>()
      .Property(x => x.ApiKeyType)
      .HasConversion(new EnumToStringConverter<Enums.ApiKeyType>());
    modelBuilder.Entity<MessageQueue>()
     .Property(p => p.Type)
     .HasConversion(new EnumToStringConverter<Enums.MessageType>());
    modelBuilder.Entity<MessageQueue>()
     .Property(p => p.SendTo)
     .HasMaxLength(200);
    modelBuilder.Entity<MessageQueue>()
      .Property(x => x.PersonalisationJson)
      .HasMaxLength(4000);
    modelBuilder.Entity<MessageQueue>()
      .HasIndex(x => x.ServiceUserLinkId);

    SetGuidKeyOnId<Organisation>(modelBuilder);
    modelBuilder.Entity<Organisation>()
      .Property(x => x.OdsCode)
      .IsRequired()
      .HasMaxLength(200);
    modelBuilder.Entity<Organisation>()
      .HasIndex(x => x.OdsCode)
      .IsUnique();

    modelBuilder.Entity<OrganisationAudit>()
      .HasKey(ra => ra.AuditId);
    modelBuilder.Entity<OrganisationAudit>()
      .Property(x => x.OdsCode)
      .IsRequired()
      .HasMaxLength(200);

    modelBuilder.Entity<ElectiveCarePostError>()
      .HasKey(x => x.Id);

    modelBuilder.Entity<ElectiveCarePostError>()
    .Property(x => x.Id)
    .ValueGeneratedOnAdd();

    SetGuidKeyOnId<MskOrganisation>(modelBuilder);
    modelBuilder.Entity<MskOrganisation>()
      .Property(x => x.OdsCode)
      .IsRequired()
      .HasMaxLength(200);
    modelBuilder.Entity<MskOrganisation>()
      .HasIndex(x => x.OdsCode)
      .IsUnique();
    modelBuilder.Entity<MskOrganisation>()
      .Property(x => x.SiteName)
      .IsRequired()
      .HasMaxLength(200);

    modelBuilder.Entity<MskOrganisationAudit>()
      .HasKey(ra => ra.AuditId);
    modelBuilder.Entity<MskOrganisationAudit>()
      .Property(x => x.OdsCode)
      .IsRequired()
      .HasMaxLength(200);
    modelBuilder.Entity<MskOrganisationAudit>()
      .Property(x => x.SiteName)
      .IsRequired()
      .HasMaxLength(200);

    modelBuilder.Entity<LinkId>()
      .HasKey(x => x.Id);
    modelBuilder.Entity<LinkId>()
      .Property(x => x.Id)
      .HasMaxLength(200);

    modelBuilder.Entity<UdalExtract>()
      .Property(r => r.NhsNumber).HasMaxLength(200);

    modelBuilder.Entity<UdalExtract>()
      .HasOne(ur => ur.UdalExtractHistory)
      .WithMany(ue => ue.UdalExtracts)
      .HasForeignKey(ur => ur.UdalExtractHistoryId);
  }

  private static void SetGuidKeyOnId<T>(ModelBuilder modelBuilder)
    where T : BaseEntity
  {
    modelBuilder.Entity<T>().HasKey(p => p.Id);

    modelBuilder.Entity<T>(r =>
    {
      r.Property(p => p.Id)
        .HasDefaultValueSql("newsequentialid()")
        .ValueGeneratedOnAdd();
    });
  }

  private static void ConfigureAudit()
  {
    Audit.EntityFramework.Configuration.Setup()
      .ForContext<DatabaseContext>(config => config
          .IncludeEntityObjects(true)
          .AuditEventType("{context}:{database}"))
      .UseOptOut();

    Audit.Core.Configuration.Setup()
      .UseEntityFramework(x => x
        .AuditTypeNameMapper(typeName => typeName + "Audit")
        .AuditEntityAction<IAudit>(
          (auditEvent, eventEntry, auditEntity) =>
          {
            EntityFrameworkEvent efEvent =
                  auditEvent.GetEntityFrameworkEvent();

            auditEntity.AuditAction = eventEntry.Action;
            auditEntity.AuditDuration = auditEvent.Duration;
            auditEntity.AuditErrorMessage = efEvent.ErrorMessage;
            auditEntity.AuditResult = efEvent.Result;
            auditEntity.AuditSuccess = efEvent.Success;

            return true;
          }));
  }

  private static void InitializeAzureKeyVaultProvider()
  {
    // thread-safe
    Interlocked.Increment(ref _timesInitializeAzureKeyVaultProviderCalled);

    if (_timesInitializeAzureKeyVaultProviderCalled == 1 
      && _alwaysEncryptedIsEnabled != "false")
    {
      ClientSecretCredential tokenCredential = new(
        tenantId: _alwaysEncryptedTenantId,
        clientId: _alwaysEncryptedClientId,
        clientSecret: _alwaysEncryptedClientSecret);

      SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider =
        new(tokenCredential);

      Dictionary<string, SqlColumnEncryptionKeyStoreProvider> providers = new()
      {
        {
          SqlColumnEncryptionAzureKeyVaultProvider.ProviderName,
          azureKeyVaultProvider
        }
      };

      SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
    }
  }
}
