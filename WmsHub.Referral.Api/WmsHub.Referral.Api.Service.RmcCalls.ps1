$apiKeyEnvVar = "WmsHub.Referral.Api.Service_ApiKey"
$emailPasswordEnvVar = "WmsHub.Referral.Api.Service_EmailPassword"

$apiKey = [System.Environment]::GetEnvironmentVariable($apiKeyEnvVar,"machine")
$emailPassword = [System.Environment]::GetEnvironmentVariable($emailPasswordEnvVar,"machine")

$subject = "WmsHub.Referral.Api.Service.RmcCalls"
$body = "The WmsHub.Referral.Api.Service.RmcCalls"
$mailPriority = 0

try
{

    if (!$apiKey) 
    { 
        throw [System.ArgumentException] "$($apiKeyEnvVar) not found in env vars." 
    }

    if (!$emailPassword) 
    { 
        Write-Error "$($emailPasswordEnvVar) not found in env vars."
        exit
    }

    $Params = @{
     "URI"     = "https://api-referrals.wmp.nhs.uk/admin/preparermccalls"
     "Method"  = 'GET'
     "Headers" = @{
     "Content-Type"  = 'application/json'
     "X-API-KEY" = $apiKey
     }
    }
 
    $result = Invoke-RestMethod @Params

    $subject = "$($subject) Success"
    $body = "$($body) completed successfully.`n$($result)"
    $mailPriority = 1

}
catch [Exception]
{
    $subject = "$($subject) Failure"
    $body = "$($body) failed.`n$($PSItem.ToString())"
    $mailPriority = 2
}

try 
{
    $smtpServer = "send.nhs.net"
    $smtpPort = "587"

    $username = "mlcsu.wms@nhs.net"

    $from = "mlcsu.wms@nhs.net"
    $to = "mlcsu.digitalinnovations@nhs.net"

    $message = new-object System.Net.Mail.MailMessage 
    $message.Priority = $mailPriority
    $message.From = $from 
    $message.To.Add($to) 
    $message.Subject = $subject 
    $message.Body = $body

    $smtp = New-Object System.Net.Mail.SmtpClient($smtpServer, $smtpPort)
    $smtp.EnableSSL = $true
    $smtp.Credentials = New-Object System.Net.NetworkCredential($username, $emailPassword)
   

    $smtp.Send($message)
}
catch 
{
    echo $_.Exception|format-list -force
}
