## !!! THIS FILE IS SIGNED - ANY CHANGE TO THIS FILE WILL REQUIRE IT TO BE RESIGNED !!!

$emailPasswordEnvVar = "WmsHub.ReferralsService_EmailPassword"

$emailPassword = [System.Environment]::GetEnvironmentVariable($emailPasswordEnvVar,"user")

$subject = "WmsHub.ReferralsService.Console"
$body = "The WmsHub.ReferralsService.Console"
$mailPriority = 0

$app = "WmsHub.ReferralsService.Every3Hours"
ProcessStatus.Started.ps1 $app

try
{
  if (!$emailPassword)
  { 
    Write-Error [System.ArgumentException] "$($emailPasswordEnvVar) not found in env vars."
        throw "$($emailPasswordEnvVar) not found in env vars."
  }

# /process_referrals /report /test_failure /create_from_csv /update_from_csv /test_pdf /test_success /test_critical  

#  WmsHub.ReferralsService.SmartCard.ps1
  .\WmsHub.ReferralsService.Console.exe /process_referrals
  $wmsExitCode = $LastExitCode
  
  #If there is a critical failure, try again.  This may resolve issues with failures due to WMS going to sleep
  if ($wmsExitCode -eq 2) {
#    WmsHub.ReferralsService.SmartCard.ps1
    .\WmsHub.ReferralsService.Console.exe /process_referrals
	$wmsExitCode = $LastExitCode
  }
  
  if ($wmsExitCode -ne 0) {
    throw "WmsHub.ReferralsService exited with ($wmsExitCode)"
  }
  
  ProcessStatus.Success.ps1 $app
  Exit 0

}
catch [Exception]
{
  if ($wmsExitCode -eq 1) {
        ProcessStatus.Failure.ps1 $app "($app) failed with ($wmsExitCode)"
    $subject = "$($subject) Failure"
    $body = "$($body) completed with non-critical errors.  All referrals were processed, although individual referrals may have reported an error.`n$($PSItem.ToString())"
    $mailPriority = 0
  }
  else
  {
        ProcessStatus.Failure.ps1 $app "($app) critcally failed with ($wmsExitCode)"
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

if ($wmsExitCode -ne 0) {
  $parentDirectory = Split-Path -Parent $PSScriptRoot
  $logFileDate = Get-Date -Format "yyyyMMdd"
  $logFile = "$parentDirectory\logs\ReferralsService\log$logFileDate.txt"
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

$smtp.Send($message)

# SIG # Begin signature block
# MIIF0wYJKoZIhvcNAQcCoIIFxDCCBcACAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU1ppdWkKtAOGPkE/jfJdx7eMH
# aYygggMiMIIDHjCCAgagAwIBAgIUMoK+MFFeWZ0khSY9veeDMKDpkgQwDQYJKoZI
# hvcNAQELBQAwYjELMAkGA1UEBhMCR0IxFjAUBgNVBAgMDVN0YWZmb3Jkc2hpcmUx
# EjAQBgNVBAoMCU5IUyBNTENTVTEMMAoGA1UECwwDRElVMRkwFwYDVQQDDBBESVUg
# TUxDU1UgTkhTIENBMB4XDTI0MDYxMTEzNTUyNVoXDTI5MDYxMDEzNTUyNVowFzEV
# MBMGA1UEAwwMZmlsZS1zaWduaW5nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIB
# CgKCAQEAteHNVq0Cc6BbX+/OvKz6Bqqbj+Xd+Q4HJX8Ml05nIu07x6zkdpCfhzJl
# yPfgwrt8Z1WarmMo1kBneLnWa3F+6PAFS8Vx6d6QksiYqRkVzbsiCyo4GBLqdGjI
# 0W+Lwb62SUaGtFSfrEHZaPuPgXiI1bI/fmJJ3BIvhO1/CMn9KF4g/E4z3ZJkIh93
# r2kKLvtzkdDze8ueHRzTLyi3uOdP7QZZFbVUbFGjnPoQT4HxB5RkGeZ9QkV7PKv1
# AE4R+GL3XoKkiA4r0nUG4PquHgu0b0rZGdXzU1zolID8Ju4VR3gw4yGDszx4VuAI
# 5a0iORoGw6T/Ip7/lJ+95FM+bIA/9QIDAQABoxcwFTATBgNVHSUEDDAKBggrBgEF
# BQcDAzANBgkqhkiG9w0BAQsFAAOCAQEAjZcnfd0Or0f31jqIk6N+VEAKYGLFHu2N
# iL/UyW8bn63jUQwyV9jogv6vUIBV8CB2HPZsZy+8cKIiAYk6yHJzh9h0c7WT7LlE
# ksbvk/xd/9G9jANsYTHclDyiUZ6vdgkyZRUoEZXCSduuEiYUbaYOqGo0uNDN7Akq
# Dp8wVj26yFRe1GYoKwsmrG62Am3UwYuLg4Z3lnvKIbtNWLRrMG/JyEAK7++N0KlI
# 0cH7wm71Hb8vT9fC4pnXE3Tu6Z8Z82Mo+bbIPPmyg5Xp1CH8FLVFQ3Ga0PmU5NIS
# 3AurmFF/0K1upHnb6U3jVl2zN1qkJwllv0qEyfkZiPXwE3J5iwWGOjGCAhswggIX
# AgEBMHowYjELMAkGA1UEBhMCR0IxFjAUBgNVBAgMDVN0YWZmb3Jkc2hpcmUxEjAQ
# BgNVBAoMCU5IUyBNTENTVTEMMAoGA1UECwwDRElVMRkwFwYDVQQDDBBESVUgTUxD
# U1UgTkhTIENBAhQygr4wUV5ZnSSFJj2954MwoOmSBDAJBgUrDgMCGgUAoHgwGAYK
# KwYBBAGCNwIBDDEKMAigAoAAoQKAADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIB
# BDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAjBgkqhkiG9w0BCQQxFgQU
# dgFXmiLWOQGZMF5spFwf2tMR4U0wDQYJKoZIhvcNAQEBBQAEggEArRc8xvp5PSfO
# G3olaZEdYSCON8HDv6o2q0iXHwYVV5WTzAbBzDujvHV0WUt83Ns15ymMQjXIlfLn
# 2a2NSCH0AlE0vLp1DhdZ7ca/4jDRWBKoBQxszs3y1UNcBwTaVNt912n1+AUdX4NJ
# JpsCrSomVn0Zh5zF+eHzXBB5qSPI8IyLUuHcMDDuqgknsEwrBOTg+4Vas20d7Nf0
# rLmgpvnrIvMkYvdD/cNPweBfGYlOx8jc2PQ7RKU1zAqYpe0QPx+bxXX1VzH925DT
# cO1yTXBYA0QTT/s6IW4P14VOYj5ZQTosmf6FHn6eH3ovwLOW5h0XHzgqWqVP4/7O
# mgwnk6FS5A==
# SIG # End signature block
