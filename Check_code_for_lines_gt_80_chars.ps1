$fileDirectory = ".\"
$exceptionNo = 0

Get-Childitem -Recurse -include *.cs $fileDirectory | 
  where { $_ -notlike '*obj*' -and $_ -notlike '*migrations*'  } |
  foreach {
    $file = $_
    $lines = Get-Content $file
    $lineNo = 0;    

    foreach ($line in $lines)
    {        
        $lineNo++;
        if ($line.Length -gt 80)
        {
            $exceptionNo++;
            
            Write-Host "##vso[task.logissue type=warning;]"$exceptionNo $file : "Line" $lineNo : "Length" $line.Length

            if ($line.Length -eq 81)
            {                
                Write-Host $exceptionNo $file : "Line" $lineNo : "Length" $line.Length -ForegroundColor Green
            }
            elseif ($line.Length -eq 82)
            {
                Write-Host $exceptionNo $file : "Line" $lineNo : "Length" $line.Length -ForegroundColor Yellow
            }
            else
            {
                Write-Host $exceptionNo $file : "Line" $lineNo : "Length" $line.Length -ForegroundColor Red
            } 
            
        }
    }
  }
