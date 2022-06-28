using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities
{
	public abstract class TextMessageBase : BaseEntity
	{
		public Guid ReferralId { get; set; }
		public string Number { get; set; }
		public DateTimeOffset Sent { get; set; }
		public DateTimeOffset? Received { get; set; }
		public string Outcome { get; set; }
		public string Base36DateSent { get; set; }
	}
}