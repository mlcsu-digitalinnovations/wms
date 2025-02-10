$(document).ready(function () {
  function confirm_delay() {
    var isValid = true;

    var day = $('#delay-day').val();
    var month = $('#delay-month').val();
    var year = $('#delay-year').val();

    if ($("#txtDelayReason").val().length < 5) {
      $("#txtDelayReason-error").show();
      $("#delay-reason-group").addClass("nhsuk-form-group--error");
      isValid = false;
    } else {
      $("#txtDelayReason-error").hide();
      $("#delay-reason-group").removeClass("nhsuk-form-group--error");
    }

    var date_regex = /^(0[1-9]|1[0-2])\/(0[1-9]|1\d|2\d|3[01])\/(20[2-9])([0-9])$/;

    var testDate = month + '/' + day + '/' + year;
    if (date_regex.test(testDate)) {
      $("#dateDelayReason-error").hide();
      $("#delay-reason-group-date").removeClass("nhsuk-form-group--error");

    } else {
      $("#dateDelayReason-error").show();
      $("#delay-reason-group-date").addClass("nhsuk-form-group--error");
      isValid = false;
    }

    if (isValid) {
      var maxDateToDelay = new Date($("#MaxDateToDelay").val())
      var selectedDate = new Date(year, month, day, 0, 0, 0, 0);
      if (selectedDate > maxDateToDelay) {
        isValid = false;
        $("#dateDelayReason-error").text("Latest possible delay date is "
          + maxDateToDelay.toLocaleDateString());
        $("#dateDelayReason-error").show();
        $("#delay-reason-group-date").addClass("nhsuk-form-group--error");
      }
    }

    if (isValid) {
      $("#DelayReason").val($("#txtDelayReason").val());
      $("#DelayUntil").val(year + '-' + month + '-' + day);
      return true;
    }
    $("#DelayReason").val('');
    $("#DelayUntil").val('');
    return false;
  }

  function cancel_delay(control) {
    $("#delay-reason-group").removeClass("nhsuk-form-group--error");
    $("#delay").removeClass("nhsuk-form-group--error");
    $("#txtDelayReason-error").hide();
    $("#dateDelayReason-error").hide();
    $("#txtDelayReason").val('');
    $('#delay-day').val('');
    $('#delay-month').val('');
    $('#delay-year').val('');
    closeDialog(control);
  }

  $('#submitDelay').off('click').on('click', function () {
    return confirm_delay();
  });

  $('#cancelDelay').off('click').on('click', function () {
    return cancel_delay(this);
  });

  $('#wms-delay-button button').off('click').on('click', function () {
    openDialog('wms-delay-dialog', this);
  });
});