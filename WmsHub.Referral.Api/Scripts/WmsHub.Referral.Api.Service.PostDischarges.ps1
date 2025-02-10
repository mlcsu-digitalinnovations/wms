$apiKeyEnvVar = "WmsHub.Referral.Api.Service_ApiKey"
$apiKey = [System.Environment]::GetEnvironmentVariable($apiKeyEnvVar,"user")
$app = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
C:\ProcessStatus\ProcessStatus.Started.ps1 $app

try
{
    if (!$apiKey) 
    { 
        throw [System.ArgumentException] "$($apiKeyEnvVar) not found in env vars." 
    }

    $Params = @{
     "URI"     = "https://api-referrals.wmp.nhs.uk/referral/GpDocumentProxy/Discharge"
     "Method"  = 'GET'
     "Headers" = @{
     "Content-Type"  = 'application/json'
     "X-API-KEY" = $apiKey
     }
    }
 
    $result = Invoke-RestMethod @Params
	
	C:\ProcessStatus\ProcessStatus.Success.ps1 $app
	Exit 0

}
catch [Exception]
{
	C:\ProcessStatus\ProcessStatus.Failure.ps1 $app "($app) failed with ($LastExitCode)"   
}