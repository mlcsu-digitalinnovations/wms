$emailPasswordEnvVar = "WmsHub.ReferralsService_EmailPassword"

$emailPassword = [System.Environment]::GetEnvironmentVariable($emailPasswordEnvVar,"user")

$subject = "IdentityAgentApi.Console Test"
$body = "The IdentityAgentApi.Console Test"
$mailPriority = 0

# /log_ticket - adds the ticket to the log file (use with care)

try
{
  if (!$emailPassword) 
  { 
    Write-Error [System.ArgumentException] "$($emailPasswordEnvVar) not found in env vars."
    exit 
  }

  .\WmsHub.IdentityAgentApi.Console.exe /log_ticket

  if ($LastExitCode -ne 0) {
    throw "IdentityAgentApi Test exited with ($LastExitCode)"
  }

  $subject = "$($subject) Success"
  $body = "$($body) completed successfully."
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

$logFileDate = Get-Date -Format "yyyyMMdd"
$logFile = ".\logs\log$logFileDate.txt"
if (Test-Path $logFile)
{
  $attachment = New-Object System.Net.Mail.Attachment($logFile)
  $message.Attachments.Add($attachment)
}

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
