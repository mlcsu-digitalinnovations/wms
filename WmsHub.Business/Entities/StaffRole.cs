using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities
{
  // Additional config in OnModelCreating
  [Table("StaffRoles")]
  public class StaffRole : StaffRoleBase, IStaffRole
  {
  }
}
