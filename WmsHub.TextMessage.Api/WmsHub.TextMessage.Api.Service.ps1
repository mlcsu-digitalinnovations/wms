$apiKeyEnvVar = "WmsHub.TextMessage.Api.Service_ApiKey"
$emailPasswordEnvVar = "WmsHub.TextMessage.Api.Service_EmailPassword"

$apiKey = [System.Environment]::GetEnvironmentVariable($apiKeyEnvVar,"machine")
$emailPassword = [System.Environment]::GetEnvironmentVariable($emailPasswordEnvVar,"machine")

$subject = "WmsHub.TextMessage.Api.Service"
$body = "The WmsHub.TextMessage.Api.Service"
$mailPriority = 0

try
{

    if (!$apiKey) 
    { 
        throw [System.ArgumentException] "$($apiKeyEnvVar) not found in env vars." 
    }

    if (!$emailPassword) 
    { 
        Write-Error [System.ArgumentException] "$($emailPasswordEnvVar) not found in env vars."
        exit 
    }

    $Params = @{
     "URI"     = "https://api-textmessage.wmp.nhs.uk/sms"
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
   
try 
{
    $smtp.Send($message)
}
catch 
{
    echo $_.Exception|format-list -force
}
