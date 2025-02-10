using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ProviderRejectionReasonData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.InsertData(
            table: "ProviderRejectionReasons",
            columns: new[]
            {
              "Id",
              "Title",
              "Description",
              "ModifiedByUserId",
              "IsActive",
              "ModifiedAt",
              "Group"
            },
            values: new object[,]
            {
              {
                Guid.Parse("BFF0C030-5CEC-4682-AAEB-D17D0B89BC6D"),
                "BMI Below Threshold",
                "Service user's BMI is below the threshold for their ethnicity", 
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                1
              },
              {
                Guid.Parse("C070E614-F707-485D-992F-7F879B2FC3E5"),
                "Exclusion",
                "Service user's registration information indicates they meet one of the exclusion criteria",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                1
              },
              {
                Guid.Parse("457E1CD9-C0BB-4FCD-BC4C-D7CEAD5A4B17"),
                "Wrong Provider",
                "Service user has selected the wrong provider",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                2
              },
              {
                Guid.Parse("67BAB31A-3928-4D25-9E20-0E37E84C3482"),
                "No Response",
                "Service user has not responded within 28 days of provider initiating first contact",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                2
              },
              {
                Guid.Parse(("21ED78CF-3A51-4242-BECE-2BBB30A24FBB").ToLower()),
                "BMI Below 20",
                "Service user's BMI has fallen below 20",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                3
              },
              {
                Guid.Parse(("0B4E4038-DA6D-400D-934B-154D59217261").ToLower()),
                "Withdraw Requested",
                "Service user has requested to withdraw from the programme",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                3
              },
              {
                Guid.Parse(("0FB61EC5-E3D5-4451-A927-D65B1D75735F").ToLower()),
                "Contact Failed",
                "Service user failed to respond after several contact attempts by RMC",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse(("C34C37F1-FA77-4652-BF76-5383D2BB7813").ToLower()),
                "Digital Divide",
                "Service user does not have any digital capability or capacity",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("38EF3F83-B229-46E6-9E59-7C8FF0DB730D"),
                "Criteria not met",
                "Service user does not have a diagnosis of diabetes type 1, type 2, or hypertension",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("18470477-97C4-4471-96B2-1282CE60D0D0"),
                "Service User Not Informed",
                "Service user was unaware of the referral and does not wish to engage with the programme",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("D7ACE043-9FA5-4506-8B43-CDCAF30152E6"),
                "Service User Health",
                "Service user's physical health capacity",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("9D073603-F2A9-48B0-A3B3-6800FBD8CB76"),
                "Language barrier",
                "Language barrier",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("604BA70D-EA85-41A7-9FB0-BEEE6E778FB4"),
                "Learning disability",
                "Service user has a learning disability",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("E9470731-50B7-4629-8095-6489F12350ED"),
                "Face 2 face requested",
                "Service user would like to join a face-to-face programme instead",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              },
              {
                Guid.Parse("A2B71183-FEC1-460B-ABDC-463B4C1F01DB"),
                "Service user details invalid",
                "Service user contact details unavailable/invalid",
                Guid.Empty,
                true,
                DateTimeOffset.Now,
                0
              }
            }
          );
    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.Sql(@"DELETE FROM ProviderRejectionReasons
WHERE ID in ('BFF0C030-5CEC-4682-AAEB-D17D0B89BC6D',
'C070E614-F707-485D-992F-7F879B2FC3E5',
'457E1CD9-C0BB-4FCD-BC4C-D7CEAD5A4B17',
'67BAB31A-3928-4D25-9E20-0E37E84C3482',
'21ED78CF-3A51-4242-BECE-2BBB30A24FBB',
'0B4E4038-DA6D-400D-934B-154D59217261',
'0FB61EC5-E3D5-4451-A927-D65B1D75735F',
'C34C37F1-FA77-4652-BF76-5383D2BB7813',
'38EF3F83-B229-46E6-9E59-7C8FF0DB730D',
'18470477-97C4-4471-96B2-1282CE60D0D0',
'D7ACE043-9FA5-4506-8B43-CDCAF30152E6',
'9D073603-F2A9-48B0-A3B3-6800FBD8CB76',
'604BA70D-EA85-41A7-9FB0-BEEE6E778FB4',
'E9470731-50B7-4629-8095-6489F12350ED',
'A2B71183-FEC1-460B-ABDC-463B4C1F01DB')");
        }
    }
}
