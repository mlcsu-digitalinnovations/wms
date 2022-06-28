using System;
using System.Linq;
using WmsHub.Common.Api.Interfaces;
using static WmsHub.Common.Enums;

namespace WmsHub.ReferralsService.Pdf
{
  public partial class ReferralAttachmentPdfProcessor
  {
    /// <summary>
    /// After the document has been loaded and converted into a list
    /// of string values, corrective measures are performed here.
    /// Transformations are performed against the DocumentContent field.
    /// New transformations for V1 templates should be added to the end of this 
    /// list.
    /// </summary>
    private void PerformTransformationsV1()
    {
      FixAlternativeSourceSystemEntry();
      if (Source == SourceSystem.Unidentified)
      {
        _log.Debug($"Skipped interventions on attachment from " +
          $"source '{Source}'");
      }
      else
      {
        //Pre-mapping transformations go here
        if (Source == SourceSystem.DXS)
        {
          FixDXSTemplateV1();
        }
        else
        {
          FixVisionTemplate();
          FixEmisBMI();
          FixSystemOneWeightDateSplit();
          FixCompoundDateWeight();
          FixCompoundHeight();
          FixCompoundBmiValue();
          FixDiabetesHypertensionWithDates();
          FixMultipleDiabetesHypertensionAnswers();
          FixUnmappableItems();
          FixDuplicateWeightValues();
        }
      }
    }

    /// <summary>
    /// After the document has been loaded and converted into a list
    /// of string values, corrective measures are performed here.
    /// Transformations are performed against the DocumentContent field.
    /// New transformations for V1 templates should be added to the end of this 
    /// list.
    /// </summary>
    private void PerformTransformationsV2()
    {
      if (Source == SourceSystem.Unidentified)
      {
        _log.Debug($"Skipped interventions on attachment from " +
          $"source '{Source}'");
      }
      else
      {
        //Pre-mapping transformations go here
        FixAlternativeSourceSystemEntry();
        FixDXSTemplateV2();
      }
    }



    /// <summary>
    /// Alternations made to the ReferralPost object before the referral record 
    /// is created.  This applies to both V1 and V2 templates.
    /// </summary>
    /// <param name="referral"></param>
    private void PerformReferralTransformations(IReferralTransformable referral)
    {
      RemovePDLDSectionAnswersOnFailureToProcess(referral);
      RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(referral);
      FixAddressFieldsIfAddress1Missing(referral);
    }

    /// <summary>
    /// This is a post-process intervention.  If certain questions in the 
    /// PD/LD section failed to process, then 
    /// all answers in that section will be removed.
    /// </summary>
    /// <param name="referralPost"></param>
    private void RemovePDLDSectionAnswersOnFailureToProcess(
      IReferralTransformable referral)
    {
      if (Source == SourceSystem.SystemOne)
      {
        if (referral.IsVulnerable == null ||
        referral.HasRegisteredSeriousMentalIllness == null ||
        referral.HasAPhysicalDisability == null ||
        referral.HasALearningDisability == null)
        {
          string logEntry = "PDLD Answers removed due to failure when " +
          $"processing one of the answers for {Source}";
          _log.Debug(logEntry);
          AppendToParsingReport(logEntry);
          referral.IsVulnerable = null;
          referral.HasRegisteredSeriousMentalIllness = null;
          referral.HasAPhysicalDisability = null;
          referral.HasALearningDisability = null;
        }
      }
    }

    /// <summary>
    /// This is a post-process intervention. If Address1 is not provided,
    /// Address2 and Address3 are moved up to populate where possible.
    /// </summary>
    /// <param name="referral"></param>
    public void FixAddressFieldsIfAddress1Missing(
      IReferralTransformable referral)
    {
      if (string.IsNullOrWhiteSpace(referral.Address1))
      {
        if (!string.IsNullOrWhiteSpace(referral.Address2))
        {
          referral.Address1 = referral.Address2;
          referral.Address2 = referral.Address3;
          referral.Address3 = string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(referral.Address3))
        {
          referral.Address1 = referral.Address3;
          referral.Address2 = string.Empty;
          referral.Address3 = string.Empty;
        }
      }
    }

    /// <summary>
    /// This is a post-process intervention.  If any one of the Hypertension
    /// or Diabetes answers are TRUE, then set the others to FALSE if they
    /// are currently null.
    /// </summary>
    /// <param name="referral"></param>
    public void RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(
      IReferralTransformable referral)
    {
      if (referral.HasDiabetesType1 == true ||
        referral.HasDiabetesType2 == true ||
        referral.HasHypertension == true)
      {
        if (referral.HasDiabetesType1 == null)
        {
          referral.HasDiabetesType1 = false;
        }
        if (referral.HasDiabetesType2 == null)
        {
          referral.HasDiabetesType2 = false;
        }
        if (referral.HasHypertension == null)
        {
          referral.HasHypertension = false;
        }
      }
    }


    /// <summary>
    /// Take the source system entry and place any superfluous text into 
    /// the next lines
    /// </summary>
    private void FixAlternativeSourceSystemEntry()
    {
      bool found = false;
      int lineNumber = 0;

      for (int i = 0; i < DocumentContent.Count; i++)
      {
        if (DocumentContent[i].ToUpper().Contains("SYSTEM:"))
        {
          found = true;
          lineNumber = i;
          break;
        }
      }
      if (found == true)
      {
        string[] entry = DocumentContent[lineNumber].Trim().Split(' ');
        if (entry.Length > 1)
        {
          _log.Debug($"Applying Source System fix for " +
            $"'{DocumentContent[lineNumber]}'.");
          string sourceSystemText = entry[0];
          string templateVersionText;
          //Get Template Version
          if (entry[1].ToUpper().StartsWith("V")){
            templateVersionText = entry[1];
          }
          else
          {
            templateVersionText = entry[0];
          }
          
          string extraText = "";
          for (int i = 1; i < entry.Length; i++)
          {
            extraText = extraText + entry[i] + " ";
          }
          DocumentContent.RemoveAt(lineNumber);
          DocumentContent.Insert(lineNumber, extraText);
          DocumentContent.Insert(lineNumber, "EXTRASYSTEMTEXT:");
          DocumentContent.Insert(lineNumber, templateVersionText);
          DocumentContent.Insert(lineNumber, sourceSystemText);
        }
      }
    }

    /// <summary>
    /// Where the Date of last BMI and BMI value are both filled in, the
    /// second question needs to be moved down by 1 line in pdf version 1.7 +
    /// </summary>
    private void FixEmisBMI()
    {
      if (Source != SourceSystem.Emis) return;

      int lineNumber = LineContaining("Date of last BMI (within 24 months):");
      if (lineNumber != -1)
      {
        int nextSectionLine = LineContaining("ADDITIONAL INFORMATION:");
        bool fixToBeApplied = false;

        if (nextSectionLine == lineNumber + 4)
        {
          if (DocumentContent[lineNumber + 1].Trim().EndsWith(":"))
          {
            fixToBeApplied = true;
          }
        }
        else if (nextSectionLine == lineNumber + 3)
        {
          //This means one of the two questions has not been answered.
          //If the answer does not parse as a date, then there is no need
          //to apply this fix.
          DateTime testValue;
          if (DateTime.TryParse(DocumentContent[lineNumber + 2], out testValue)
            == false)
          {
            _log.Debug("Did not need to apply EMIS BMI questions fix as " +
              "the Date of Last BMI question was not answered.");
          }
          else
          {
            fixToBeApplied = true;
          }
        }
        if (fixToBeApplied)
        {
          _log.Debug("Applying EMIS BMI questions fix.");
          string questionToMove = DocumentContent[lineNumber + 1];
          DocumentContent.RemoveAt(lineNumber + 1);
          DocumentContent.Insert(lineNumber + 2, questionToMove);
        }
      }
    }

    /// <summary>
    /// System One does not allow the date of a BMI result to be reported
    /// upon seperately.  It does allow date and weight to be reported as
    /// a single entry though, separated by a comma.  This method looks
    /// for this and creates the necessary question for 'Date of BMI'.
    /// </summary>
    private void FixSystemOneWeightDateSplit()
    {
      if (Source != SourceSystem.SystemOne) return;

      bool found = false;
      int lineNumber = 0;

      for (int i = 0; i < DocumentContent.Count; i++)
      {
        if (DocumentContent[i] == "Weight:")
        {
          found = true;
          lineNumber = i;
          break;
        }
      }
      if (found == true)
      {
        _log.Debug("Applying SYSTEM 1 Date Split.");
        string[] splitValues = DocumentContent[lineNumber + 1].Split(",");
        if (splitValues.Length == 2)
        {
          DocumentContent[lineNumber + 1] = splitValues[1].Trim();
          DocumentContent.Add("Date of last BMI:");
          DocumentContent.Add(splitValues[0].Trim());
        }
      }
      else
      {
        _log.Debug("Did not apply SYSTEM 1 Date Split as 'Weight:' " +
          "question not found.");
      }
    }

    /// <summary>
    /// Detect where weight field has been included preceeded by a date and a 
    /// heading
    /// </summary>
    private void FixCompoundDateWeight()
    {
      //Look for the weight field
      int lineToCheck = LineContaining("WEIGHT:");
      if (lineToCheck == -1)
      {
        _log.Debug("Could not apply Compound Date and Weight fix as" +
          " the 'Weight' question could not be found");
        return;
      }
      string answerToCheck = DocumentContent[lineToCheck + 1];
      if (ContainsCompoundWeightDateAnswers(answerToCheck))
      {
        string correctWeightAnswer =
          WeightFromCompoundWeightDateAnswer(answerToCheck);
        string correctDateAnswer =
          DateFromCompoundWeightDateAnswer(answerToCheck);
        //Replace the existing answer with the correct one.
        DocumentContent.RemoveAt(lineToCheck + 1);
        DocumentContent.RemoveAt(lineToCheck);
        DocumentContent.Add("WEIGHT:");
        DocumentContent.Add(correctWeightAnswer);
        _log.Debug($"Fixed compound weight and date " +
          $"from '{answerToCheck}' to '{correctWeightAnswer}'");
        if (!string.IsNullOrWhiteSpace(correctDateAnswer)) {
          _log.Debug($"Checking BMI date answer as the date " +
            $"from '{answerToCheck}' was found to be '{correctDateAnswer}'");

          //Check to see if the Date of Last BMI question is missing.  If it is,
          //add that question to the end.
          int bmiLineToCheck = LinePartiallyContaining("Date of last BMI");
          if (bmiLineToCheck < 0) //Question was not found
          {
            DocumentContent.Add("DATE OF LAST BMI:");
            DocumentContent.Add(correctDateAnswer);
            _log.Debug($"Added 'DATE OF LAST BMI' question with " +
              $"the answer '{correctDateAnswer}' from the Weight answer.");
          }
          else
          {
            //If the Date of Last BMI question has not been answered, remove that
            //question and replace it at the end.
            //If the next line is a qustion, then provide the date answer
            if (DocumentContent[bmiLineToCheck + 1].Trim().EndsWith(':'))
            {
              //Move question and answer to the end.
              DocumentContent.RemoveAt(bmiLineToCheck);
              DocumentContent.Add("DATE OF LAST BMI:");
              DocumentContent.Add(correctDateAnswer);
              _log.Debug($"Changed 'DATE OF LAST BMI' answer with " +
                $"'{correctDateAnswer}' as the Date question was not " +
                $"answered.");
            }
          }        
        }
      }
      else
      {
        _log.Debug("Did not need to apply Compound Date and Weight fix.");
      }
    }

    public static string SplitAndExtractCorrectedAnswer(
      string answerToCheck, 
      decimal minValue, 
      decimal maxValue, 
      bool includeConversionCheck = false,
      int conversionFactor = 1)
    {
      // split the answer by spaces
      string[] splits = answerToCheck
        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

      string correctedAnswer = string.Empty;

      // check for value in reverse order as its likely to be at the end of the
      // answerToCheck
      for (int i = splits.Length - 1; i >= 0; i--)
      {
        // if the first char is a digit then after removing any non digits
        // if the value is a valid decimal and in the expected range then 
        // we can assume its the correct value
        if (char.IsDigit(splits[i][0]))
        {
          //string possibleValue = new string(splits[i]
          //  .Where(c => char.IsDigit(c) || c == '.')
          //  .ToArray());

          string possibleValue = string.Empty;
          foreach (char c in splits[i])
          {
            if (char.IsDigit(c) || c == '.')
            {
              possibleValue += c;
            }
            else
            {
              // finding anything other than a digit or . assume end of number
              break;
            }
          }
          
          if (decimal.TryParse(possibleValue, out decimal value))
          {
            if (includeConversionCheck && value < minValue)
            {
              value = value * conversionFactor;
            }
            
            if (value >= minValue && value <= maxValue)
            {
              correctedAnswer = value.ToString();
              break;
            }            
          }
        }
      }
      return correctedAnswer;
    }

    /// <summary>
    /// Detect where height field has anything else in the field and find the
    /// actual height and return it
    /// </summary>
    private void FixCompoundHeight()
    {
      //Look for the weight field
      int lineToCheck = LineContaining("HEIGHT:");
      if (lineToCheck == -1)
      {
        _log.Debug("Could not apply Compound Height fix as " +
          "the 'Height' question could not be found");
        return;
      }
      string answerToCheck = DocumentContent[lineToCheck + 1];
      //Ensure the answer to check is not the next question
      if (answerToCheck.EndsWith(":"))
      {
        _log.Debug("Skipped FixCompoundHeight transformation as " +
          "the 'Height' answer could not be identified.");
        return;
      }

      string correctedAnswer = SplitAndExtractCorrectedAnswer(
        answerToCheck, 100, 250, true, 100);

      if (correctedAnswer != string.Empty)
      {
        //Replace the existing answer with the correct one.
        DocumentContent.RemoveAt(lineToCheck + 1);
        DocumentContent.RemoveAt(lineToCheck);
        DocumentContent.Add("HEIGHT:");
        DocumentContent.Add(correctedAnswer);
        _log.Debug($"Fixed compound height " +
          $"from '{answerToCheck}' to '{correctedAnswer}'");
      }
      else
      {
        _log.Debug("Did not need to apply Compound Height fix.");
      }
    }

    /// <summary>
    /// Detect where Bmi field has anything else in the field and find the
    /// actual Bmi and return it
    /// </summary>
    private void FixCompoundBmiValue()
    {
      //Look for the weight field
      int lineToCheck = LineContaining("Value of last BMI (within 24 months):");
      if (lineToCheck == -1)
      {
        _log.Debug("Could not apply Compound Bmi Value fix as " +
          "the 'Value of last BMI (within 24 months):' question could not " +
          "be found");
        return;
      }
      string answerToCheck = DocumentContent[lineToCheck + 1];

      //Under certain circumstances, the Date of Last BMI answer is in the
      //wrong place.
      DateTime nextLineDateTime;
      if (DateTime.TryParse(answerToCheck, out nextLineDateTime) == true)
      {
        _log.Debug("Misplaced Date identified");
        //Find the Date of Last BMI question and ensure there is no answer given
        int lineToCheckbmiDate =
          LineContaining("Date of last BMI (within 24 months):");
        if (lineToCheckbmiDate != -1)
        {
          //If the next line is a qustion, then provide the date answer
          if (DocumentContent[lineToCheckbmiDate + 1].Trim().EndsWith(':'))
          {
            //Move question and answer to the end.
            DocumentContent.RemoveAt(lineToCheckbmiDate);
            if (lineToCheckbmiDate < lineToCheck) lineToCheck--;
            DocumentContent.Add("Date of last BMI:");
            DocumentContent.Add(answerToCheck);
            DocumentContent.RemoveAt(lineToCheck+1);
            answerToCheck = DocumentContent[lineToCheck + 1];
          }
        }
      }

      
      string correctedAnswer = SplitAndExtractCorrectedAnswer(
        answerToCheck, 27.5m, 90, false);

      if (correctedAnswer != string.Empty)
      {
        //Replace the existing answer with the correct one.
        DocumentContent.RemoveAt(lineToCheck + 1);
        DocumentContent.RemoveAt(lineToCheck);
        DocumentContent.Add("Value of last BMI (within 24 months):");
        DocumentContent.Add(correctedAnswer);
        _log.Debug($"Fixed compound Bmi " +
          $"from '{answerToCheck}' to '{correctedAnswer}'");
      }
      else
      {
        _log.Debug("Did not need to apply Compound Bmi Value fix.");
      }
    }

    private void FixDXSTemplateV1()
    {
      if (Source == SourceSystem.DXS)
      {
        _log.Debug("Applying DXS Template Fixes");
        
        //1 - Hypertension answer bleeds into Height Question
        //Look for line ending with 'Height:'
        int heightQuestionLine = LineEndingWith("Height:");
        if (heightQuestionLine == -1) {
          _log.Debug("DXS: Could not split the Height question as the " +
            "question text not be found.");
        } else
        {
          SplitBeforeText(heightQuestionLine, "Height:");
        }
        //2 -Separate the BMI answer from the subsequent following junk data
        int bmiQuestionLine = LineContaining("Value of last BMI " +
          "(within 24 months):");
        if (bmiQuestionLine == -1)
        {
          _log.Debug("DXS: Could not fix the BMI question as the " +
            "question text 'Value of last BMI " +
          "(within 24 months):' could not be found.");
        }
        else
        {
          DocumentContent.Insert(bmiQuestionLine+2, "DXS_Terminator1:");
          //Remove extra text from BMI value
          if (DocumentContent[bmiQuestionLine + 1].Contains("kg"))
          {
            SplitBeforeText(bmiQuestionLine + 1, "kg");
            DocumentContent[bmiQuestionLine + 2] = "DXS_Terminator2:";
            _log.Debug("Removed extra text from BMI question and " +
              "terminated the answer");
          }
        }
        //3 - Address Fix
        //Addresses on DXS referrals are received in a set of rows following
        //the question 'Address:', ending with the post code.
        //This intervention will attempt to identify this block by either
        //looking for the postcode, or the next question 'Home Tel. No:' as a
        //marker.  The first line will be the new 'Address 1:' question,
        //followed by 'Address 2:' and all remaining lines (apart from the
        //postcode), separated by commas, is 'Address 3'.
        _log.Debug("DXS:Fixing Address");
        int addressQuestionLine = LineContaining("Address:");
        int postCodeQuestionLine = LineContaining("Postcode:");
        int homeTelNoQuestionLine = LineContaining("Home Tel. No.:");
        bool foundEverything = true;
        if (addressQuestionLine == -1)
        {
          _log.Debug("DXS: Could not fix the Address question as the " +
            "question text 'Address:' could not be found.");
          foundEverything = false;
        }
        if (postCodeQuestionLine == -1)
        {
          _log.Debug("DXS: Could not fix the Address question as the " +
            "question text 'Postcode:' could not be found.");
          foundEverything = false;
        }
        if (homeTelNoQuestionLine == -1)
        {
          _log.Debug("DXS: Could not fix the Address question as the " +
            "question text 'Home Tel. No.:' could not be found.");
          foundEverything = false;
        }
        if (foundEverything)
        {
          //If the postcode can be used as a terminator, this will be removed
          //from the address.  The postcode answer, if not provided, will
          //contain some question text instead.
          string postCodeAnswer = DocumentContent[postCodeQuestionLine + 1];
          if (DocumentContent[homeTelNoQuestionLine - 1] == postCodeAnswer)
          {
            _log.Debug("DXS:PostCode removed from address");
            DocumentContent[homeTelNoQuestionLine - 1] = string.Empty;
          }
          //Transform 'Address:' question to read 'Address 1:'
          DocumentContent[addressQuestionLine] = "Address 1:";
          if (addressQuestionLine + 2 < homeTelNoQuestionLine)
          {
            _log.Debug($"Processing Address line 2: '" +
              $"{DocumentContent[addressQuestionLine + 2]}'");
            //Replace address line 2
            DocumentContent.Add("Address 2:");
            DocumentContent.Add(DocumentContent[addressQuestionLine + 2]);
            DocumentContent[addressQuestionLine + 2] = string.Empty;
          }
          if (addressQuestionLine + 3 < homeTelNoQuestionLine)
          {
            _log.Debug($"Processing Address line 3: '" +
              $"{DocumentContent[addressQuestionLine + 2]}'");
            //Replace address line 3
            DocumentContent.Add("Address 3:");
            DocumentContent.Add(DocumentContent[addressQuestionLine + 3]);
            DocumentContent[addressQuestionLine + 3] = string.Empty;
          }
          for (int i = addressQuestionLine + 4; i < homeTelNoQuestionLine; i++)
          {
            if (string.IsNullOrEmpty(DocumentContent[i]) == false)
            {
              DocumentContent.Add($", {DocumentContent[i].Trim()}");
              DocumentContent[i] = string.Empty;
            }
          }
        }
        //4 - SMI answer bleeds into PD Question
        //Look for line ending containing
        //'Severe Mental Illness Y or N must be selected:'
        int smiQuestionLine = 
          LineContaining("Severe Mental Illness Y or N must be selected:");
        if (smiQuestionLine == -1)
        {
          _log.Debug("DXS: Could not split the SMI question as the " +
            "question text not be found.");
        }
        else
        {
          string smiAnswer = TruncateFrom(
            DocumentContent[smiQuestionLine + 1], "Physical Disability");
          if (smiAnswer != DocumentContent[smiQuestionLine + 1])
          {
            _log.Debug($"Changing SMI answer from " +
              $"'{DocumentContent[smiQuestionLine + 1]}' to '{smiAnswer}'");
            DocumentContent[smiQuestionLine + 1] = smiAnswer;
          }
          else
          {
            _log.Debug($"Did not change SMI answer from '{smiAnswer}'");
          }
        }
        //5 - Learning Disability fix
        int ldQuestionLine = 
          LineContaining("Literacy Difficulties. Y or N must be selected:");
        if (ldQuestionLine == -1)
        {
          _log.Debug("DXS: Could not split the LD question as the " +
            "question text not be found.");
        }
        else
        {
          //Insert a breakpoint when a long line is encountered.  This will
          //allow for 'yes' or 'no' but little else.
          for (int i = ldQuestionLine+1; i<DocumentContent.Count; i++)
          {
            if (DocumentContent[i].Length >= 3)
            {
              _log.Debug($"DXS: Ending LD question at " +
                $"'{DocumentContent[i-1]}'");
              DocumentContent.Insert(i, "DXS_Terminator5:");
              break;
            }
          }
        }
        //6 - Fix AIS superfluous form entries
        int aisQuestionLine =
          LineStartingWith("Accessible Information Needs (AIS):");
        if (aisQuestionLine == -1)
        {
          _log.Debug("DXS: Could not find 'Accessible Information Needs " +
            "(AIS) section");
        }
        else
        {
          DocumentContent[aisQuestionLine] = "Accessible Information Needs:";
          _log.Debug("DXS: Truncated AIS Question");
        }

        //7 - Fix LD question if the red text is misplaced by a tiny amount
        ldQuestionLine =
          LineContaining("Identified as a Vulnerable Adult");
        if (ldQuestionLine == -1)
        {
          _log.Debug("DXS: Could not find 'Identified as a Vulnerable " +
            "Adult truncated question.");
        }
        else
        {
          DocumentContent[ldQuestionLine] = "Identified as a Vulnerable Adult:";
          if (DocumentContent.Count > ldQuestionLine)
          {
            if (DocumentContent[ldQuestionLine + 1] == 
              "Y or N must be selected:")
            {
              _log.Debug("DXS: Fixing truncated Vulnerable Adult question.");
              DocumentContent[ldQuestionLine + 1] = "#IGNORE";
            }
          }        
        }
      }
    }
    
    private void FixDXSTemplateV2()
    {
      if (Source == SourceSystem.DXS)
      {
        _log.Debug("Performing DXS V2 Interventions");

        //There is an issue with the PDF formatting which is causing answers
        //to a question, and the next question to be merged onto the same line.
        //On page 1, the following are the start text of the questions where
        //this is an issue.  If those questions are changed, the wording here
        //should be altered to reflect this.
        //Page 1 Items:
        const string LD_QUESTION_START =
          "Does this person have a learning disability?";
        const string SMI_QUESTION_START =
          "Does this person have a severe mental illness?";
        const string VULN_QUESTION_START =
          "Instead of receiving a SMS text message,";
        //Page 2 Items:
        const string ETHNICITY_QUESTION = "Ethnicity:";
        const string DIABETES2_QUESTION = "Diabetes Type 2:";
        const string HYPERTENSION_QUESTION = "Hypertension:";
        const string HEIGHT_QUESTION = "Height:";

        int lineToCheck = LinePartiallyContaining(LD_QUESTION_START);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, LD_QUESTION_START);
        }
        lineToCheck =
          LinePartiallyContaining(SMI_QUESTION_START);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, SMI_QUESTION_START);
        }
        lineToCheck =
          LinePartiallyContaining(VULN_QUESTION_START);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, VULN_QUESTION_START);
          DocumentContent.Insert(lineToCheck + 4, "PageBreak:");
          _log.Debug("Added page break separator to DXS V2 Document Page 1");
        }

        lineToCheck = LineEndingWith(ETHNICITY_QUESTION);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, ETHNICITY_QUESTION);
        }
        lineToCheck = LineEndingWith(DIABETES2_QUESTION);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, DIABETES2_QUESTION);
        }
        lineToCheck = LineEndingWith(HYPERTENSION_QUESTION);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, HYPERTENSION_QUESTION);
        }
        lineToCheck = LineEndingWith(HEIGHT_QUESTION);
        if (lineToCheck > 0)
        {
          SplitLineFromText(lineToCheck, HEIGHT_QUESTION);
        }
      }
    }

    void SplitLineFromText(
      int lineToSplit, 
      string fromText)
    {
      int position = DocumentContent[lineToSplit].IndexOf(fromText);
      if (position > 0)
      {
        string preceedingText = 
          DocumentContent[lineToSplit].Substring(0, position);
        string trailingText = 
          DocumentContent[lineToSplit].Substring(position);
        _log.Debug($"Splitting answer '{DocumentContent[lineToSplit]}' into" +
          $" '{preceedingText}' and '{trailingText}'.");
        DocumentContent.Insert(lineToSplit + 1, trailingText);
        DocumentContent[lineToSplit] = preceedingText;
        
      }
    }


    /// <summary>
    /// Detects whether or not a given answer contains the date followed by
    /// a weight
    /// </summary>
    /// <param name="answer"></param>
    /// <returns></returns>
    static bool ContainsCompoundWeightDateAnswers(string answer)
    {
      bool result = false;

      if (DateFromCompoundWeightDateAnswer(answer) != "")
      {
        if (WeightFromCompoundWeightDateAnswer(answer) != "")
        {
          result = true;
        }
      }

      return result;
    }

    static string WeightFromCompoundWeightDateAnswer(string answer)
    {
      string result = "";
      //Normalise the answer, removing colons and commas which could confuse
      //things
      answer = answer.Trim().ToUpper().Replace(':', ' ').Replace(',', ' ');

      string[] splitAnswer = answer.Split(' ');
      for (int i = 0; i < splitAnswer.Length; i++)
      {
        if (splitAnswer[i] == "IDEAL")
        {
          //ignore Ideal Body Weight here - skip past the next number found
          for (int j = i + 1; j < splitAnswer.Length; j++)
          {
            if (decimal.TryParse(splitAnswer[j], out _) == true)
            {
              i = j + 1;
              break;
            }
            //Now look for a number ending with Kg
            if (splitAnswer[j].Length > 2)
            {
              if (splitAnswer[j]
                .Substring(splitAnswer[j].Length - 2, 2) == "KG")
              {
                i = j + 1;
                break;
              }
            }
          }
        }
        if (i < splitAnswer.Length)
        {
          if (decimal.TryParse(splitAnswer[i], out _) == true)
          {
            return splitAnswer[i];
          }
          //Now look for a number ending with Kg
          if (splitAnswer[i].Length > 2)
          {
            if (splitAnswer[i]
              .Substring(splitAnswer[i].Length - 2, 2) == "KG")
            {
              return splitAnswer[i].Substring(0, splitAnswer[i].Length - 2);
            }
          }
        }
        if (result != "") break;
      }

      return result;
    }

    private void FixVisionTemplate()
    {
      if (Source == SourceSystem.Vision)
      {

        int questionLine;

        //All dropdowns leave artifacts behind.  Remove these.

        //Diabetes Type 1
        questionLine = LineContaining("Diabetes Type 1:");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision Diabetes T1 answer because the " +
            "question could not be found.");
        }
        else
        {
          //If the next two lines start Y or N then remove the first character
          StripInitialYNFromLines(questionLine + 1, questionLine + 2);
        }
        //Diabetes Type 1
        questionLine = LineContaining("Diabetes Type 2:");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision Diabetes T2 answer because the " +
            "question could not be found.");
        }
        else
        {
          //If the next two lines start Y or N then remove the first character
          StripInitialYNFromLines(questionLine + 1, questionLine + 2);
        }
        //Hypertension
        questionLine = LineContaining("Hypertension:");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision Hypertension answer because the " +
            "question could not be found.");
        }
        else
        {
          //If the next two lines start Y or N then remove the first character
          StripInitialYNFromLines(questionLine + 1, questionLine + 2);
        }

        //Identified as a Vulnerable Adult
        questionLine = LineStartingWith("Identified as a Vulnerable Adult Y");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision Vulnerable Adult answer because " +
            "the question could not be found.");
        }
        else
        {
          //If the next two lines start Y or N then remove the first character
          StripInitialYNFromLines(questionLine + 2, questionLine + 3);
        }

        //SMI
        questionLine = LineContaining("Severe Mental Illness Y or N must " +
          "be selected:");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision SMI answer because the " +
            "question could not be found.");
        }
        else
        {
          //If the next two lines start Y or N then remove the first character
          StripInitialYNFromLines(questionLine + 1, questionLine + 2);
        }

        //PD
        int pdQuestionLine = LineStartingWith("Physical Disability including");
        if (pdQuestionLine == -1)
        {
          _log?.Debug("Did not fix Vision PD answer because " +
            "the question could not be found.");
        }
        else
        {
          //If the next line start Y or N then remove the first character
          DocumentContent[pdQuestionLine] = "PHYSICAL DISABILITY:";
          DocumentContent[pdQuestionLine + 1] = "";
          DocumentContent[pdQuestionLine + 2] = "";
          DocumentContent[pdQuestionLine + 3] = "";
        }

        //LD
        questionLine = LineStartingWith("Learning Disability including");
        if (questionLine == -1)
        {
          _log?.Debug("Did not fix Vision LD answer because " +
            "the question could not be found.");
        }
        else
        {
          //If the next line start Y or N then remove the first character
          if (questionLine + 5 < DocumentContent.Count)
          {
            DocumentContent[questionLine] = "LEARNING DISABILITY:";
            DocumentContent[questionLine + 1] = "";
            DocumentContent[questionLine + 2] = "";
            StripInitialYNFromLines(questionLine + 3, questionLine + 5);
            //Move the PD answer to the correct place
            DocumentContent[pdQuestionLine + 1] =
              DocumentContent[questionLine + 4];
            DocumentContent[questionLine + 4] = "";

          }
        }
      }
    }
 
    private void StripInitialYNFromLines(
      int startLine,
      int endLine)
    {
      for (int answerLine = startLine; answerLine <= endLine;
            answerLine++)
      {
        if (DocumentContent[answerLine].Length > 0)
        {
          string startAnswer = 
            DocumentContent[answerLine].Substring(0, 1).ToUpper();
          if (startAnswer == "Y" || startAnswer == "N")
          {
            if (DocumentContent[answerLine].Length == 1)
            {
              DocumentContent[answerLine] = "";
            }
            else
            {
              DocumentContent[answerLine] =
                DocumentContent[answerLine].Substring(1);
            }
          }
        }
      }
    }

    static string DateFromCompoundWeightDateAnswer(string answer)
    {
      string result = "";
      answer = answer.Trim().ToUpper().Replace(':', ' ').Replace(',', ' ');

      string[] splitAnswer = answer.Split(' ');
      //Dates will contain two dashes with numbers preceeding the first
      //dash and numbers after the last
      for (int i = 0; i < splitAnswer.Length; i++)
      {
        int numberDashes = splitAnswer[i].Count(f => (f == '-'));
        int numberSlashes = splitAnswer[i].Count(f => (f == '/'));
        if (numberDashes == 2 || numberSlashes == 2)
        {
          string dateCandidate = splitAnswer[i];
          string[] splitDate = null;
          if (numberDashes == 2) splitDate = dateCandidate.Split("-");
          if (numberSlashes == 2) splitDate = dateCandidate.Split("/");
          if (splitDate.Length == 3)
          {
            if (int.TryParse(splitDate[0], out _) == true &&
              int.TryParse(splitDate[2], out _) == true)
            {
              result = dateCandidate;
            }
          }
        }
        if (result != "") break;
      }

      return result;
    }

    /// <summary>
    /// Returns the first line containing any given text
    /// </summary>
    /// <param name="textToFind">The text to find. 
    /// This is not case sensitive</param>
    /// <returns>The index of the row, or -1 if the item is not found</returns>
    private int LineContaining(string textToFind)
    {
      textToFind = textToFind.Trim().ToUpper();
      for (int i = 0; i < DocumentContent.Count; i++)
      {
        if (DocumentContent[i].Trim().ToUpper() == textToFind)
        {
          return i;
        }
      }
      return -1;
    }

    private int LineEndingWith(string textToFind)
    {
      textToFind = textToFind.Trim().ToUpper();
      for (int i = 0; i < DocumentContent.Count; i++)
      {
        //Ignore lines which are too short
        string normalisedLine = DocumentContent[i].Trim().ToUpper();
        if (normalisedLine.Length >= textToFind.Length)
        {
          string segment = normalisedLine.Substring(
            normalisedLine.Length - textToFind.Length, textToFind.Length);
          if (segment == textToFind)
          {
            return i;
          }
        }
      }
      return -1;
    }

    private int LineStartingWith(string textToFind)
    {
      if (string.IsNullOrWhiteSpace(textToFind))
      {
        return -1;
      }
      textToFind = textToFind.Trim().ToUpper();
      for (int i = 0; i < DocumentContent.Count; i++)
      {
        //Ignore lines which are too short
        string normalisedLine = DocumentContent[i].Trim().ToUpper();
        if (normalisedLine.Length >= textToFind.Length)
        {
          string segment = normalisedLine.Substring(0, textToFind.Length);
          if (segment == textToFind)
          {
            return i;
          }
        }
      }
      return -1;
    }


    private void SplitBeforeText(
      int row, 
      string text)
    {
      if (row <0 || row >= DocumentContent.Count)
      {
        _log.Debug($"Could not split row at index {row}, as the last index" +
          $" available is {DocumentContent.Count - 1}!");
        return;
      } else
      {
        string textline1 = DocumentContent[row];
        int position = textline1.ToUpper().LastIndexOf(text.ToUpper());
        string textline2 = textline1.Substring(position);
        textline1 = textline1.Substring(0, position);
        _log.Debug($"Split line from '{DocumentContent[row]}' into " +
          $"'{textline1}' and '{textline2}'.");
        DocumentContent[row] = textline1;
        DocumentContent.Insert(row + 1, textline2);
      }
    }

    /// <summary>
    /// Returns the first line with any given text included as part of the line
    /// </summary>
    /// <param name="textToFind">The text to find.
    /// This is not case sensitive</param>
    /// <returns>The index of the row, or -1 if the item is not found</returns>
    private int LinePartiallyContaining(string textToFind)
    {
      textToFind = textToFind.Trim().ToUpper();
      for (int i = 0; i < DocumentContent.Count; i++)
      {
        if (DocumentContent[i].Trim().ToUpper().Contains(textToFind))
        {
          return i;
        }
      }
      return -1;
    }


    /// <summary>
    /// This will apply the global mappings to a block of the document.
    /// This may be necessary to normalise blocks of document data
    /// while applying a transformation.
    /// </summary>
    /// <param name="startIndex">The index of the start of the block</param>
    /// <param name="endIndex">The index of the end of the block</param>
    private void ApplyMappingToBlock(
      int startIndex, 
      int endIndex)
    {
      if (startIndex < 0 || endIndex < 0 ||
        startIndex >= DocumentContent.Count ||
        endIndex >= DocumentContent.Count)
      {
        throw new IndexOutOfRangeException($"The start and end indexes must " +
          $"be within the range of 0 - {DocumentContent.Count - 1}.");
      }
      if (startIndex > endIndex)
      {
        throw new IndexOutOfRangeException($"The start index must be lower " +
          $"than the end index");
      }
      for (int i = startIndex; i <= endIndex; i++)
      {
        DocumentContent[i] = 
          _answerMap.MappedItem(DocumentContent[i]);
      }

    }

    /// <summary>
    /// Removes any lines which has been marked '#IGNORE' by the mapping
    /// process.  This may be necessary to simplify blocks of document data
    /// while applying a transformation.
    /// </summary>
    /// <param name="startIndex">The index of the start of the block</param>
    /// <param name="endIndex">The index of the end of the block</param>
    private void RemoveIgnoredAndBlankLinesFromBlock(
      int startIndex, 
      int endIndex)
    {
      if (startIndex < 0 || endIndex < 0 ||
        startIndex >= DocumentContent.Count ||
        endIndex >= DocumentContent.Count)
      {
        throw new IndexOutOfRangeException($"The start and end indexes must " +
          $"be within the range of 0 - {DocumentContent.Count - 1}.");
      }
      if (startIndex > endIndex)
      {
        throw new IndexOutOfRangeException($"The start index must be lower " +
          $"than the end index");
      }
      for (int i = endIndex; i >= startIndex; i--)
      {
        if (DocumentContent[i] == "#IGNORE" ||
          string.IsNullOrWhiteSpace(DocumentContent[i]))
        {
          DocumentContent.RemoveAt(i);
        };
      }
    }

    /// <summary>
    /// This is a diagnostic routine for use during development
    /// </summary>
    /// <param name="headerText"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    private void SendDocumentSectionToConsole(
      string headerText,
      int startIndex,
      int endIndex)
    {
      if (startIndex < 0 || endIndex < 0 ||
        startIndex >= DocumentContent.Count ||
        endIndex >= DocumentContent.Count)
      {
        throw new IndexOutOfRangeException($"The start and end indexes must " +
          $"be within the range of 0 - {DocumentContent.Count - 1}.");
      }
      if (startIndex > endIndex)
      {
        throw new IndexOutOfRangeException($"The start index must be lower " +
          $"than the end index");
      }
      Console.WriteLine($"{headerText} Start : " +
        $"Lines {startIndex} to {endIndex} -----");
      for (int i = startIndex; i <= endIndex; i++)
      {
        Console.WriteLine(DocumentContent[i]);
      }
      Console.WriteLine($"{headerText} End : " +
        $"Lines {startIndex} to {endIndex} -----");
    }

    private void FixDiabetesHypertensionWithDates()
    {
      int questionLine, answerLine;

      questionLine = LineContaining("Diabetes Type 2:");
      if (questionLine == -1)
      {
        _log?.Debug("FixDiabetesHypertensionWithDates could not find " +
          "question 'Diabetes Type 2:'");
      }
      else
      {
        answerLine = questionLine + 1;
        DocumentContent[answerLine] =
          StripDateFromStart(DocumentContent[answerLine]);
      }
      questionLine = LineContaining("Diabetes Type 1:");
      if (questionLine == -1)
      {
        _log?.Debug("FixDiabetesHypertensionWithDates could not find " +
          "question 'Diabetes Type 1:'");
      }
      else
      {
        answerLine = questionLine + 1;
        DocumentContent[answerLine] =
          StripDateFromStart(DocumentContent[answerLine]);
      }
      if (questionLine == -1)
      {
        _log?.Debug("FixDiabetesHypertensionWithDates could not find " +
          "question 'Hypertension:'");
      }
      else
      {
        questionLine = LineContaining("Hypertension:");
        answerLine = questionLine + 1;
        DocumentContent[answerLine] =
          StripDateFromStart(DocumentContent[answerLine]);
      }
    }

    /// <summary>
    /// This method captures items which are unmappable
    /// </summary>
    private void FixUnmappableItems()
    {
      //Remove 'Must be Completed' from form where it contains an ellipsis

      int questionLine = LineContaining("Weight:");
      if (questionLine >0 && questionLine < DocumentContent.Count - 2)
      {
        if (DocumentContent[questionLine+2].StartsWith("MUST BE COMPLETED"))
        {
          DocumentContent[questionLine + 2] = "#IGNORE";
          _log.Debug("Removed unparsable line 'MUST BE COMPLETED' with" +
            " non-parsable decoration.");
        }
      }
    }

    /// <summary>
    /// Remove multiple answers from the Hypertension and Diabetes quesitons
    /// </summary>
    public void FixMultipleDiabetesHypertensionAnswers()
    {
      RemoveDuplicatedMappedYesOrNoAnswer("Diabetes Type 2:");
      RemoveDuplicatedMappedYesOrNoAnswer("Diabetes Type 1:");
      RemoveDuplicatedMappedYesOrNoAnswer("Hypertension:");
    }

    /// <summary>
    /// Where a question has the same answer on the two lines following,
    /// remove the second answer.
    /// </summary>
    /// <param name="question">The question to check for a duplicated
    /// answer</param>
    void RemoveDuplicatedMappedYesOrNoAnswer(string question)
    {
      bool foundMultipleAnswer;

      int questionLine = LineContaining(question);
      if (questionLine == -1)
      {
        _log.Verbose($"Did not find duplicate answer to '{question}' " +
          $"as the question was not found.");
      }
      else
      {
        int answerLine = questionLine + 1;
        //All of the answers to these questions have mappings attached.
        string firstAnswer =
          _answerMap.MappedItem(DocumentContent[answerLine]);
        if (firstAnswer == "true" || firstAnswer == "false")
        {
          do
          {
            foundMultipleAnswer = false;
            if (answerLine + 1 >= DocumentContent.Count) break;
            string secondAnswer =
              _answerMap.MappedItem(DocumentContent[answerLine + 1]);
            if (secondAnswer == firstAnswer)
            {
              _log.Debug($"Found multiple mapped answers to question " +
                $"'{question}'.  Keeping '{DocumentContent[answerLine]}' " +
                $"from index {answerLine} and discarding " +
                $"'{DocumentContent[answerLine + 1]}' from index " +
                $"{answerLine + 1}.");
              foundMultipleAnswer = true;
              DocumentContent.RemoveAt(answerLine + 1);
            }
          } while (foundMultipleAnswer);
        }
      }
    }


    private void FixDuplicateWeightValues()
    {
      int questionLine = LineContaining("Weight:");
      if (questionLine == -1)
      {
        _log.Debug("Could not fix duplicate Weight answers as the" +
          " question 'Weight:' could not be found.");
        return;
      }
      int answerLine = questionLine + 1;
      decimal answer;
      if (answerLine + 2 < DocumentContent.Count) {
        if (DocumentContent[answerLine] == DocumentContent[answerLine + 1])
        {
          if (decimal.TryParse(DocumentContent[answerLine], out answer) == true)
          {
            DocumentContent.RemoveAt(answerLine + 1);
            _log.Debug($"Removed Duplicate Weight answer" +
              $" '{DocumentContent[answerLine]}'.");
          }
          else
          {
            _log.Debug("Did not fix Duplicate Weight answers, as the " +
              "answer supplied was not a number.  Found " +
              $"'{DocumentContent[answerLine]}'.");
          }
        }
        else
        {
          _log.Debug("Did not need to fix Duplicate Weight answers.");
        }
      }
      else
      {
        _log.Debug("Did not need to fix Duplicate Weight answers.");
      }
    }

  }
}
