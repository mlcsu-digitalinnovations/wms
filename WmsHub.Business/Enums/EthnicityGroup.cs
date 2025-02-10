using System.ComponentModel;

namespace WmsHub.Business.Enums
{
	public enum EthnicityGroup
	{
		[Description("Asian or Asian British")]
		AsianOrAsianBritish,
		[Description("Black, African, Caribbean or Black British")]
		BlackAfricanCaribbeanOrBlackBritish,
		[Description("I do not wish to Disclose my Ethnicity")]
		DoNotWishToDisclose,
		[Description("Mixed or Multiple ethnic groups")]
		MixedOrMultiple,
		[Description("Other ethnic group")]
		Other,
		[Description("White")]
		White
	}
}