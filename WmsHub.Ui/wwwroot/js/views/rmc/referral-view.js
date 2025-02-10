$(document).ready(function () {

  $("input:radio").change(function () {
    $('#selectedProvider').val($(this).data("provider-name"));
    isFormValid();
  });

  $("#Email").on('input', function () { isFormValid(); });

  var maxDaysToDelay = "+" + $("#MaxDaysToDelay").val() + "D";

  $("#delay-year").datepicker({
    showOn: "both",
    buttonImage: "/images/calendar.gif",
    buttonImageOnly: true,
    buttonText: "Select date",
    minDate: +1,
    maxDate: maxDaysToDelay,
    defaultDate: "+2d",
    onSelect: function (date) {
      date = date.split(/\//g);

      var day = date[1];
      var month = date[0];
      var year = date[2];

      $('#delay-day').val(day);
      $('#delay-month').val(month);
      $('#delay-year').val(year);

    }
  });

  $("#delay-day").mouseup(function () {
    $("#delay-year").datepicker('show');
  });
  $("#delay-month").mouseup(function () {
    $("#delay-year").datepicker('show');
  });

  $("#refresh_email_history_button").click(fetchAndDisplayEmailHistory);
  $("#show_email_history_button").click(fetchAndDisplayEmailHistory);

  $('#wms-rejection-button').click(function () {
    if (isBmiTooLow) {
      var bmi = '@Model.Bmi';
      var selectedEthnicity = '@Model.SelectedEthnicity';
      var minBmi = '@Model.SelectedEthnicGroupMinimumBmi';
      $('#txtStatusReason').val(`BMI of ${bmi} is below the minimum of ` +
        `${minBmi} for the selected ethnic group ${selectedEthnicity}.`);
    }
  });

  $("#email_provider_list").on("click",
    function (event) {
      event.preventDefault();
      event.stopPropagation();
      var referralId = $("#Id").val();
      var ubrn = $("#ubrn_txt").val();
      var url = "/rmc/EmailProviderListToServiceUser?Ubrn=" + ubrn + "&ReferralId=" + referralId;
      $('#loading_icon').show();
      $.ajax({
        type: "POST",
        url: url,
        success: function (result) {
          $("#email_provider_list_to_serviceuser_response")
            .text("Email has been sent to service user with provider details.");
          $("#email_provider_list_to_serviceuser_response")
            .addClass("nhsuk-heading-xs nhsblue");
          $(".forward_as_email").attr("disabled", "disabled");
          $('#loading_icon').hide();
          var url2 = "/rmc/EmailProviderListToServiceUserVerification?messageId=" + result.id;
        },
        error: function (xhr) {
          $("#email_provider_list_to_serviceuser_response")
            .text("There was a problem sending the email. " + xhr.responseText);
          $("#email_provider_list_to_serviceuser_response").addClass("nhsuk-error-message");
          $('#loading_icon').hide();
        },
        async: true
      });
    });

  $("#email_elective_care_link").on("click",
    function (event) {
      event.preventDefault();
      event.stopPropagation();
      var referralId = $("#Id").val();
      var url = "/rmc/EmailElectiveCareLinkToServiceUser?ReferralId=" + referralId;
      $('#elective_care_link_loading_icon').show();
      $.ajax({
        type: "POST",
        url: url,
        success: function (result) {
          $("#email_elective_care_link_to_serviceuser_response")
            .html("Email has been sent to service user with referral hub link:<br>" + result);
          $("#email_elective_care_link_to_serviceuser_response")
            .addClass("nhsuk-heading-xs nhsblue");
          $("#elective_care_link_loading_icon").hide();
          $("#email_elective_care_link").attr("disabled", "disabled");
        },
        error: function (xhr) {
          $("#email_elective_care_link_to_serviceuser_response")
            .text("There was a problem sending the email. " + xhr.responseText);
          $("#email_elective_care_link_to_serviceuser_response")
            .addClass("nhsuk-error-message");
          $('#elective_care_link_loading_icon').hide();
        },
        async: true
      });
    });

  var isBmiTooLow = '@Model.IsBmiTooLow' === 'True';
  if (isBmiTooLow) {
    window.location = '#div-select-ethnicity';
  }

  $('#wms-mobile-number-button').click(function () {
    var mobile = $('#Mobile').val();
    $('#mobile-number').val(mobile);
  });

  $("#SelectedServiceUserEthnicityGroup").change(
    function () {
      var ethnicityGroup = $("#SelectedServiceUserEthnicityGroup").val();
      if (ethnicityGroup) {
        var url = "/rmc/ServiceUserEthnicityGroupMembers?ethnicityGroup=" + ethnicityGroup;
        $.ajax({
          type: "GET",
          url: url,
          success: function (result) {
            populateServiceUserEthnicityOptions(result);
            toggleConfirmEthnicity();
          },
          async: true
        });
      }
   
      toggleConfirmEthnicity();
      toggleServiceUserEthnicityDropdown();
    }
  );

  $("#SelectedServiceUserEthnicity").change(
    function () {
      toggleConfirmEthnicity();
    }
  )

  toggleConfirmEthnicity();
  toggleServiceUserEthnicityDropdown();
  isFormValid();
});

function fetchAndDisplayEmailHistory(event) {
  event.preventDefault();
  event.stopPropagation();

  $("#email_history_container").show();
  $("#show_email_history_button").hide();

  var referralId = $("#Id").val();
  var url = `/rmc/ProviderDetailsEmailHistory?referralId=${referralId}`;
  $.ajax({
    type: "GET",
    url: url,
    success: function (data, textStatus, xhr) {
      if (xhr.status == 204 || data == null || data.length == 0) {
        showEmailHistoryMessage("No emails sent to this service user.", false)
      }
      else {
        populateEmailHistoryTable(JSON.parse(data));
      }
      $("#email_history_loading_icon").hide();
      $("#refresh_email_history_button").show();
    },
    error: function (xhr) {
      showEmailHistoryMessage("There was a problem retrieving the email history.", true)
      $("#email_history_loading_icon").hide();
      $("#refresh_email_history_button").show();
    },
    async: true
  });
}

function isEmailValid() {
  var error = document.getElementById('email-error');
  var emailRegex = /^(?!.*\.\.)[^@\s]+(?<!\.)@(?!\.)[^@\s]+\.[^@\s]+$/;
  var emailText = $('#Email').val();

  var validEmail = emailRegex.test(emailText);
  if (validEmail) {
    if (error) {
      error.style.display = 'none';
    }
    $("#confirmEmail").prop("disabled", false);
  } else {
    if (error) {
      error.style.display = 'block';
    }
    $("#confirmEmail").prop("disabled", true);
  }

  return validEmail;
}

function isFormValid() {

  $("#confirmProvider").prop("disabled", true);

  if (!isEmailValid()) return false;

  // check for selected provider
  var providerList = document
    .querySelectorAll("input[name='ProviderId']:checked");

  if (providerList.length == 0) return false;

  $("#confirmProvider").prop("disabled", false);
  return true;
}

function populateEmailHistoryTable(emailHistory) {
  $("#email_history_message").hide();
  $("#email_history_table tbody").remove();
  $("#email_history_table thead").after('<tbody class = "nhsuk-table__body"></tbody>');

  var headers = ['Email', 'Created', 'Sending', 'Delivered', 'Status'];
  var dateTimeHeaders = ['Created', 'Sending', 'Delivered'];

  for (row = 0; row < emailHistory.length; row++) {
    var rowString = `<tr role="row" class="nhsuk-table__row">`;
    for (headerIndex = 0; headerIndex < headers.length; headerIndex++) {
      var header = headers[headerIndex];
      var value = emailHistory[row][header];

      if (dateTimeHeaders.includes(header)) {
        value = new Date(value).toLocaleString();
      }

      rowString += `<td role="cell" class="nhsuk-table__cell">
        <span class="nhsuk-table-responsive__heading" aria-hidden="true">${header}</span>
        ${value}</td>`;
    }

    rowString += "</tr>";
    $("#email_history_table tbody").append(rowString);
  }
  $("#email_history_table").show();
}

function populateServiceUserEthnicityOptions(options) {
  resetServiceUserEthnicityOptions();
  for (i = 0; i < options.length; i++) {
    $("#SelectedServiceUserEthnicity").append($("<option>",
      {
        value: options[i],
        text: options[i]
      }));
  }
}

function resetServiceUserEthnicityOptions() {
  $("#SelectedServiceUserEthnicity option").remove();
  $("#SelectedServiceUserEthnicity").append($("<option>",
    {
      value: "",
      text: "Select Ethnicity"
    }));
}

function showEmailHistoryMessage(message, isError) {
  if (isError) {
    $("#email_history_message").removeClass("nhsuk-heading-xs nhsblue")
      .addClass("nhsuk-error-message");
  }
  else {
    $("#email_history_message").removeClass("nhsuk-error-message")
      .addClass("nhsuk-heading-xs nhsblue");
  }

  $("#email_history_message").text(message);
  $("#email_history_table").hide();
  $("#email_history_message").show();
}

function toggleConfirmEthnicity() {
  if ($("#SelectedServiceUserEthnicity").val() && $("#SelectedServiceUserEthnicityGroup").val()) {
    $("#confirmEthnicity").prop("disabled", false);
  }
  else {
    $("#confirmEthnicity").prop("disabled", true);
  }
}

function toggleServiceUserEthnicityDropdown() {
  if ($("#SelectedServiceUserEthnicityGroup").val()) {
    $("#SelectedServiceUserEthnicity").prop("disabled", false);
  }
  else {
    $("#SelectedServiceUserEthnicity").prop("disabled", true);
    resetServiceUserEthnicityOptions();
  }
}