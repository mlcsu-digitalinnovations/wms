using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models
{
  public class ProviderAuthUpdateRequest : IValidatableObject
  {

    [Required]
    public Guid ProviderId { get; set; }

    public bool KeyViaSms { get; set; }

    public bool KeyViaEmail { get; set; }
    [MaxLength(200)]
    public string MobileNumber { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string EmailContact { get; set; }

    /// <summary>
    /// IpWhitelist is a comma separated list of IPv4 addresses
    /// i.e. 192.168.0.1,127.0.0.1,::1,
    /// Or add range
    /// 192.168.0.1-5,35.27.74.95-104
    /// </summary>
    [MaxLength(200)]
    public string IpWhitelist { get; set; }
    public string StartRange { get; set; }
    public string EndRange { get; set; }

    public string IpRange
    {
      get
      {
        if (string.IsNullOrWhiteSpace(StartRange)
          || string.IsNullOrWhiteSpace(EndRange))
        {
          return string.Empty;
        }

        string[] endRangeSplit = EndRange
          .Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (endRangeSplit.Length == 4)
        {
          return $"{StartRange}-{endRangeSplit[3]}";
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (ProviderId == default)
        yield return
          new InvalidValidationResult(nameof(ProviderId), ProviderId);

      if (KeyViaSms && string.IsNullOrWhiteSpace(MobileNumber))
        yield return new InvalidValidationResult(nameof(MobileNumber),
          MobileNumber);

      if (KeyViaSms && !string.IsNullOrWhiteSpace(MobileNumber)
          && !MobileNumber.StartsWith("+"))
        yield return new InvalidValidationResult(nameof(MobileNumber),
          "Mobile Number must start with '+' followed by country code");

      if (KeyViaEmail && string.IsNullOrWhiteSpace(EmailContact))
        yield return new InvalidValidationResult(nameof(EmailContact),
          EmailContact);

      if (!KeyViaEmail && !KeyViaSms)
        yield return new InvalidValidationResult(nameof(KeyViaSms),
          $"Either SMS or EMAIL must be supplied");

      if (string.IsNullOrWhiteSpace(IpWhitelist)
          && (string.IsNullOrWhiteSpace(StartRange)
              || string.IsNullOrWhiteSpace(EndRange)))
        yield return new InvalidValidationResult(nameof(IpWhitelist),
          IpWhitelist);

      if (!string.IsNullOrWhiteSpace(StartRange)
         && string.IsNullOrWhiteSpace(EndRange))
        yield return new InvalidValidationResult(nameof(StartRange),
          "EndRange IPv4 address must be provided.");

      if (!string.IsNullOrWhiteSpace(EndRange)
          && string.IsNullOrWhiteSpace(StartRange))
        yield return new InvalidValidationResult(nameof(StartRange),
          "StartRange IPv4 address must be provided.");

      if (IpWhitelist != "**IGNORE_FOR_TESTING**")
      {

        Regex regex = new Regex(Constants.REGEX_IPv4_ADDRESS);
        if (!string.IsNullOrWhiteSpace(StartRange))
        {
          if (!regex.IsMatch(StartRange))
            yield return new InvalidValidationResult(nameof(StartRange),
              "StartRange IPv4 Address must be in the correct format " +
              "(i.e 192.168.0.1)");

        }

        if (!string.IsNullOrWhiteSpace(EndRange))
        {
          if (!regex.IsMatch(EndRange))
            yield return new InvalidValidationResult(nameof(EndRange),
              "EndRange IPv4 Address must be in the correct format " +
              "(i.e 192.168.0.1)");

        }

        if (!string.IsNullOrWhiteSpace(StartRange) &&
            !string.IsNullOrWhiteSpace(EndRange))
        {

          if (StartRange.Split('.')[0] != EndRange.Split('.')[0] ||
              StartRange.Split('.')[1] != EndRange.Split('.')[1] ||
              StartRange.Split('.')[2] != EndRange.Split('.')[2])
            yield return new InvalidValidationResult(nameof(EndRange),
              $"EndRange {EndRange} IPv4 Address must be in same domain " +
              $"as the StartRange {StartRange}");
        }



        if (string.IsNullOrWhiteSpace(IpWhitelist)
            && string.IsNullOrWhiteSpace(StartRange)
            && string.IsNullOrWhiteSpace(EndRange))
          yield return new InvalidValidationResult(nameof(IpWhitelist),
            "Either IpWhitelist or Start & End range must be provided");

        if (!string.IsNullOrWhiteSpace(IpWhitelist))
        {
          foreach (string ipv4 in IpWhitelist.Split(','))
          {
            if (ipv4.Contains("-"))
            {
              int first = int.Parse(ipv4.Split('-')[0].Split('.')[3]);
              int last = int.Parse(ipv4.Split('-')[1]);
              for (int i = first; i <= last; i++)
              {
                string toTest = $"{ipv4.Split('-')[0].Split('.')[0]}." +
                                $"{ipv4.Split('-')[0].Split('.')[1]}." +
                                $"{ipv4.Split('-')[0].Split('.')[2]}." +
                                $"{i}";
                if (toTest != "::1" && !regex.IsMatch(toTest))
                  yield return new InvalidValidationResult(nameof(IpWhitelist),
                    $"IpWhitelist value of {toTest} " +
                    $"is not a valid IPv4 address");
              }
            }
            else
            {
              if (ipv4 != "::1" && !regex.IsMatch(ipv4))
                yield return new InvalidValidationResult(nameof(IpWhitelist),
                  $"IpWhitelist value of {ipv4} is not a valid IPv4 address");
            }
          }
        }
      }
    }
  }
}