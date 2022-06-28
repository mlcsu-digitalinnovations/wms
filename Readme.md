# Weight Management System

## Environmental Variables

### If using Always Encrypted with an Azure KeyStore
<pre>
WmsHub_AlwaysEncrypted:ClientId 
WmsHub_AlwaysEncrypted:ClientSecret
WmsHub_AlwaysEncrypted:IsEnabled
WmsHub_AlwaysEncrypted:TenantId
</pre>

### Business intelligence API
<pre>
WmsHub_BusinessIntelligence_Api_ApiKey
</pre>

### Chat Bot API
<pre>
WmsHub_ChatBot_Api_ApiKey
WmsHub_ChatBot_Api_ArcusSettings:ApiKey
WmsHub_ChatBot_Api_ArcusSettings:Endpoint
WmsHub_ChatBot_Api_ArcusSettings:IsNumberWhiteListEnabled
WmsHub_ChatBot_Api_SignalR:AllowedOrigins:0
</pre>

### Chat Bot Service
WmsHub_ChatBot_Service_ApiKey - Must match WmsHub_ChatBot_Api_ApiKey
WmsHub_ChatBot_Service_EmailPassword
</pre>

### Provider API
<pre>
WmsHub_Provider_Api_ApiKey
WmsHub_Provider_Api_AuthOptions:Notifylink
WmsHub_Provider_Api_AuthOptions:SmsApiKey
WmsHub_Provider_Api_AuthOptions:SmsSenderId
WmsHub_Provider_Api_AuthOptions:SmsTemplateId
</pre>

### Referral API
<pre>
WmsHub_Referral_Api_ApiKey
WmsHub_Referral_Api_PharmacyApiKey
WmsHub_Referral_Api_PharmacyReferralApiKey
WmsHub_Referral_Api_PracticeApiKey
WmsHub_Referral_Api_RmcUiViewReferralUrl
WmsHub_Referral_Api_SelfReferralApiKey
</pre>

### Text Message API
<pre>
WmsHub_TextMessage_Api_ApiKey
WmsHub_TextMessage_Api_TextSettings:GeneralReferralNotifyLink
WmsHub_TextMessage_Api_TextSettings:IsNumberWhiteListEnabled
WmsHub_TextMessage_Api_TextSettings:Issuer
WmsHub_TextMessage_Api_TextSettings:Notifylink
WmsHub_TextMessage_Api_TextSettings:NumberWhiteList:0
WmsHub_TextMessage_Api_TextSettings:SmsApiKey
WmsHub_TextMessage_Api_TextSettings:SmsSenderId
WmsHub_TextMessage_Api_TextSettings:TokenEnabled
WmsHub_TextMessage_Api_TextSettings:TokenPassword
WmsHub_TextMessage_Api_TextSettings:TokenSecret
WmsHub_TextMessage_Api_TextSettings:ValidUsers:0
</pre>

### Text Message Service
<pre>
WmsHub_TextMessage_Service_ApiKey - MUST match WmsHub_TextMessage_Api_ApiKey
WmsHub_TextMessage_Service_EmailPassword
</pre>

### UI
<pre>
WmsHub_Ui_AzureAd:CallbackPath
WmsHub_Ui_AzureAd:ClientId
WmsHub_Ui_AzureAd:Domain
WmsHub_Ui_AzureAd:Instance
WmsHub_Ui_AzureAd:TenantId
WmsHub_Ui_AzureFrontDoor_Uri
WmsHub_Ui_SignalR_Endpoint
</pre>

### Referral Service Console
<pre>
WmsHub.ReferralService_Data:HubRegistrationAPIKey - Must match WmsHub_Referral_Api_ApiKey
</pre>

## IdentityAgentApi
<pre>
To be able to run the tests:
Identity Agent needs to be installed
Connected to HSCN
Have a valid smart card inserted in a connected reader
The tests must be run with the 32bit because the TicketAPIDll which provides 
the interface to the card reader is a 32 bit dll. So you'll need to include 
--runtime win-x86 when you run the tests e.g.

dotnet test --runtime win-x86

to enable logging of the terminal output

dotnet test --runtime win-x86 -l "console;verbosity=detailed"
</pre>

