$(document).ready(function () {

  $("input:radio").change(function () {
    $('#selectedProvider').val($(this).data("provider-name"));
    isFormValid();
  });

  $("#Email").on('input', function () { isFormValid(); });

  $("#delay-year").datepicker({
    beforeShowDay: $.datepicker.noWeekends,
    showOn: "both",
    buttonImage: "/images/calendar.gif",
    buttonImageOnly: true,
    buttonText: "Select date",
    minDate: +1,
    maxDate: "+100D",
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

  $('#wms-rejection-button').click(function () {
    if (isBmiTooLow) {
      var bmi = '@Model.Bmi';
      var selectedEthnicity = '@Model.SelectedEthnicity';
      var minBmi = '@Model.SelectedEthnicGroupMinimumBmi';
      $('#txtStatusReason').val(`BMI of ${bmi} is below the minimum of ` +
        `${minBmi} for the selected ethnic group ${selectedEthnicity}.`);
    }
  });

  var isBmiTooLow = '@Model.IsBmiTooLow' === 'True';
  if (isBmiTooLow) {
    window.location = '#div-select-ethnicity';
  }

  $('#wms-mobile-number-button').click(function () {
    var mobile = $('#Mobile').val();
    $('#mobile-number').val(mobile);
  });

  $("#SelectedEthnicity").change(function () {
    toggleConfirmEthnicity();
  });

  toggleConfirmEthnicity();
  isFormValid();
});

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

function toggleConfirmEthnicity() {
  if ($("#SelectedEthnicity").val()) {
    $("#confirmEthnicity").prop("disabled", false);
  }
  else {
    $("#confirmEthnicity").prop("disabled", true);
  }
}