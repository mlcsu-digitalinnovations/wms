using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace WmsHub.Business.Migrations;

/// <inheritdoc />
public partial class UpdateReferralStatusReasons : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='9d073603-f2a9-48b0-a3b3-6800fbd8cb76'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='93f2d86a-b41f-ed11-ae83-501ac5963972'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='a2b71183-fec1-460b-abdc-463b4c1f01db'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='38ef3f83-b229-46e6-9e59-7c8ff0db730d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='c34c37f1-fa77-4652-bf76-5383d2bb7813'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='652df478-8275-4e03-bfe0-fef7c4a4a32d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0fb61ec5-e3d5-4451-a927-d65b1d75735f'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='f3db7d3b-fa59-4468-beef-c939f5570d2a'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='604ba70d-ea85-41a7-9fb0-beee6e778fb4'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='67bab31a-3928-4d25-9e20-0e37e84c3482'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='745b8181-cb80-4752-aedb-5134e28cbd66'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='28c62be0-8a83-4313-ab96-9b581351ec08'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0b4e4038-da6d-400d-934b-154d59217261'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='d5d9c0f4-e77d-41f5-a0d1-1d5052f254ab'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='7c9c8e6d-930a-4a23-a642-d6c5affbb01f'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='b738d646-6ce4-4b8c-a893-e1c0b4c7c0da'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='cba77064-4044-4476-b93e-f3dd5651971b'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='457e1cd9-c0bb-4fcd-bc4c-d7cead5a4b17'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0da3d7b4-5d5f-4bcf-b25c-2bad70f724b9'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='7611fd48-117b-4a34-9159-b5f67855151d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='62719502-8d2a-4e09-aa94-038cae644657'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='18470477-97c4-4471-96b2-1282ce60d0d0'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='e9470731-50b7-4629-8095-6489f12350ed'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='21ed78cf-3a51-4242-bece-2bbb30a24fbb'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='bff0c030-5cec-4682-aaeb-d17d0b89bc6d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='d7ace043-9fa5-4506-8b43-cdcaf30152e6'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='c070e614-f707-485d-992f-7f879b2fc3e5'");

    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('9d073603-f2a9-48b0-a3b3-6800fbd8cb76',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Language barrier',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('93f2d86a-b41f-ed11-ae83-501ac5963972',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','NHS number matches existing referral - patient is not eligible to be re-referred as yet',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('a2b71183-fec1-460b-abdc-463b4c1f01db',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Contact details unavailable or invalid',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('38ef3f83-b229-46e6-9e59-7c8ff0db730d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','No diagnosis of diabetes type 1, type 2 or hypertension',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('c34c37f1-fa77-4652-bf76-5383d2bb7813',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','No digital capability or capacity',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('652df478-8275-4e03-bfe0-fef7c4a4a32d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Does not meet eligibility criteria ',2)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0fb61ec5-e3d5-4451-a927-d65b1d75735f',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','No response despite several contact attempts by RMC',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('f3db7d3b-fa59-4468-beef-c939f5570d2a',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','No response despite several contact attempts by provider',2)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('604ba70d-ea85-41a7-9fb0-beee6e778fb4',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Learning disability',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('67bab31a-3928-4d25-9e20-0e37e84c3482',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','No response within 42 days of provider initiating first contact',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('745b8181-cb80-4752-aedb-5134e28cbd66',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to change provider - service provision not met',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('28c62be0-8a83-4313-ab96-9b581351ec08',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to change provider - technical reasons',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0b4e4038-da6d-400d-934b-154d59217261',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to withdraw from the programme',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('d5d9c0f4-e77d-41f5-a0d1-1d5052f254ab',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to withdraw from the programme - bereavement',9)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7c9c8e6d-930a-4a23-a642-d6c5affbb01f',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to withdraw from the programme - medical',9)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('b738d646-6ce4-4b8c-a893-e1c0b4c7c0da',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to withdraw from the programme - no motivation',13)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('cba77064-4044-4476-b93e-f3dd5651971b',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Request to withdraw from the programme - no reason',13)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('457e1cd9-c0bb-4fcd-bc4c-d7cead5a4b17',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Selected the wrong provider',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0da3d7b4-5d5f-4bcf-b25c-2bad70f724b9',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Already on a tier 2 weight management programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7611fd48-117b-4a34-9159-b5f67855151d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Deceased',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('62719502-8d2a-4e09-aa94-038cae644657',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Expecting to be referred to a different programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('18470477-97c4-4471-96b2-1282ce60d0d0',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Unaware the referral and does not wish to start',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('e9470731-50b7-4629-8095-6489f12350ed',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Wants a face-to-face programme',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('21ed78cf-3a51-4242-bece-2bbb30a24fbb',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','BMI has fallen below 30',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('bff0c030-5cec-4682-aaeb-d17d0b89bc6d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','BMI is below the threshold for ethnicity',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('d7ace043-9fa5-4506-8b43-cdcaf30152e6',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Physical health capacity',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('c070e614-f707-485d-992f-7f879b2fc3e5',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Meets one of the exclusion criteria',1)");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='9d073603-f2a9-48b0-a3b3-6800fbd8cb76'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='93f2d86a-b41f-ed11-ae83-501ac5963972'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='a2b71183-fec1-460b-abdc-463b4c1f01db'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='38ef3f83-b229-46e6-9e59-7c8ff0db730d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='c34c37f1-fa77-4652-bf76-5383d2bb7813'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='652df478-8275-4e03-bfe0-fef7c4a4a32d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0fb61ec5-e3d5-4451-a927-d65b1d75735f'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='f3db7d3b-fa59-4468-beef-c939f5570d2a'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='604ba70d-ea85-41a7-9fb0-beee6e778fb4'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='67bab31a-3928-4d25-9e20-0e37e84c3482'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='745b8181-cb80-4752-aedb-5134e28cbd66'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='28c62be0-8a83-4313-ab96-9b581351ec08'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0b4e4038-da6d-400d-934b-154d59217261'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='d5d9c0f4-e77d-41f5-a0d1-1d5052f254ab'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='7c9c8e6d-930a-4a23-a642-d6c5affbb01f'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='b738d646-6ce4-4b8c-a893-e1c0b4c7c0da'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='cba77064-4044-4476-b93e-f3dd5651971b'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='457e1cd9-c0bb-4fcd-bc4c-d7cead5a4b17'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='0da3d7b4-5d5f-4bcf-b25c-2bad70f724b9'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='7611fd48-117b-4a34-9159-b5f67855151d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='62719502-8d2a-4e09-aa94-038cae644657'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='18470477-97c4-4471-96b2-1282ce60d0d0'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='e9470731-50b7-4629-8095-6489f12350ed'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='21ed78cf-3a51-4242-bece-2bbb30a24fbb'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='bff0c030-5cec-4682-aaeb-d17d0b89bc6d'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='d7ace043-9fa5-4506-8b43-cdcaf30152e6'");
    migrationBuilder.Sql($"DELETE FROM dbo.ReferralStatusReasons WHERE Id ='c070e614-f707-485d-992f-7f879b2fc3e5'");

    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('62719502-8D2A-4E09-AA94-038CAE644657',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user was expecting to be referred to a different programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('67BAB31A-3928-4D25-9E20-0E37E84C3482',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has not responded within 42 days of provider initiating first contact',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('18470477-97C4-4471-96B2-1282CE60D0D0',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user was unaware of the referral and does not wish to engage with the programme',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0B4E4038-DA6D-400D-934B-154D59217261',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('D5D9C0F4-E77D-41F5-A0D1-1D5052F254AB',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - bereavement',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0DA3D7B4-5D5F-4BCF-B25C-2BAD70F724B9',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user is already on a tier 2 weight management programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('21ED78CF-3A51-4242-BECE-2BBB30A24FBB',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s BMI has fallen below 30',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('A2B71183-FEC1-460B-ABDC-463B4C1F01DB',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user contact details unavailable/invalid',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('93F2D86A-B41F-ED11-AE83-501AC5963972',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','NHS number matches existing referral - patient is not eligible to be re-referred as yet',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('745B8181-CB80-4752-AEDB-5134E28CBD66',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to change provider - service provision not met',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('C34C37F1-FA77-4652-BF76-5383D2BB7813',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not have any digital capability or capacity',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('E9470731-50B7-4629-8095-6489F12350ED',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user would like to join a face-to-face programme instead',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('9D073603-F2A9-48B0-A3B3-6800FBD8CB76',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Language barrier',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('38EF3F83-B229-46E6-9E59-7C8FF0DB730D',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not have a diagnosis of diabetes type 1, type 2, or hypertension',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('C070E614-F707-485D-992F-7F879B2FC3E5',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s registration information indicates they meet one of the exclusion criteria',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('28C62BE0-8A83-4313-AB96-9B581351EC08',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to change provider - technical reasons',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7611FD48-117B-4A34-9159-B5F67855151D',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user is deceased',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('604BA70D-EA85-41A7-9FB0-BEEE6E778FB4',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has a learning disability',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('F3DB7D3B-FA59-4468-BEEF-C939F5570D2A',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user failed to respond despite several contact attempts by provider',2)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('D7ACE043-9FA5-4506-8B43-CDCAF30152E6',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s physical health capacity',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('BFF0C030-5CEC-4682-AAEB-D17D0B89BC6D',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s BMI is below the threshold for their ethnicity',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0FB61EC5-E3D5-4451-A927-D65B1D75735F',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user failed to respond after several contact attempts by RMC',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7C9C8E6D-930A-4A23-A642-D6C5AFFBB01F',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - medical',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('457E1CD9-C0BB-4FCD-BC4C-D7CEAD5A4B17',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has selected the wrong provider',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('B738D646-6CE4-4B8C-A893-E1C0B4C7C0DA',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - no motivation',12)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('CBA77064-4044-4476-B93E-F3DD5651971B',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - no reason',12)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('652DF478-8275-4E03-BFE0-FEF7C4A4A32D',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not meet eligibility criteria ',2)");
  }
}
