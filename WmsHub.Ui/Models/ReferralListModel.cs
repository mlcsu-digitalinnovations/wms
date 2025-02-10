using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
namespace WmsHub.Ui.Models
{
	public class ReferralListModel
	{
		public List<ReferralListItemModel> ListItems { get; set; }
		public ReferralSearchModel Search { get; set; }
		public string ActiveUser { get; set; }
		public int Count { get; set; }
	}
}