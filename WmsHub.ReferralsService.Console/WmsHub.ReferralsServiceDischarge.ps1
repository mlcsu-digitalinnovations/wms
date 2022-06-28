$emailPasswordEnvVar = "WmsHub.ReferralsService_EmailPassword"

$emailPassword = [System.Environment]::GetEnvironmentVariable($emailPasswordEnvVar,"machine")

$subject = "WmsHub.ReferralsService.Console discharge_referrals"
$body = "The WmsHub.ReferralsService.Console discharge_referrals"
$mailPriority = 0

# /process_referrals /report /test_failure /create_from_csv /update_from_csv /test_pdf /test_success /test_critical /discharge_referrals

try
{
  if (!$emailPassword) 
  { 
    Write-Error [System.ArgumentException] "$($emailPasswordEnvVar) not found in env vars."
    exit 
  }

  .\WmsHub.ReferralsService.Console.exe /discharge_referrals

  if ($LastExitCode -ne 0) {
    throw "WmsHub.ReferralsService exited with ($LastExitCode)"
  }

  $subject = "$($subject) Success"
  $body = "$($body) completed successfully."
  $mailPriority = 1
}
catch [Exception]
{
  if ($LastExitCode -eq 1) {
    $subject = "$($subject) Failure"
    $body = "$($body) completed with errors.`n$($PSItem.ToString())"
    $mailPriority = 0
  }
  else
  {
    $subject = "$($subject) Critical Failure"
    $body = "$($body) failed.  The process may not have completed. `n$($PSItem.ToString())"
    $mailPriority = 2
  }
}

$smtpServer = "send.nhs.net"
$smtpPort = "587"

$username = "mlcsu.wms@nhs.net"

$from = "mlcsu.wms@nhs.net"
$to = "mlcsu.digitalinnovations@nhs.net"

$message = new-object System.Net.Mail.MailMessage 

if ($mailPriority -eq 2) {
    $logFileDate = Get-Date -Format "yyyyMMdd"
    $logFile = ".\logs\log$logFileDate.txt"
    if (Test-Path $logFile)
    {
      $attachment = New-Object System.Net.Mail.Attachment($logFile)
      $message.Attachments.Add($attachment)
    }
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
