"use strict";

$(document).ready(function () {

  var minAge = $('#MinGpReferralAge').val();
  var maxAge = $('#MaxGpReferralAge').val();
  var dateOfBirthDay = $('#DateOfBirthDay').val();
  var dateOfBirthMonth = $('#DateOfBirthMonth').val();
  var dateOfBirthYear = $('#DateOfBirthYear').val();

  $("#date-of-birth-year").datepicker({
    showOn: "both",
    buttonImage: "/images/calendar.gif",
    buttonImageOnly: true,
    buttonText: "Select date",
    changeMonth: true,
    changeYear: true,
    dateFormat: "yy-mm-dd", // equivalent to yyyy-mm-dd    
    defaultDate: new Date(
      dateOfBirthYear,
      dateOfBirthMonth - 1,
      dateOfBirthDay
    ),
    maxDate: +0,
    onSelect: function (date) {
      date = date.split(/-/g);

      $('#date-of-birth-day').val(date[2]);
      $('#date-of-birth-month').val(date[1]);
      $('#date-of-birth-year').val(date[0]);

      setAge();
    },
    yearRange: `-${maxAge}:-${minAge}`
  });
  $("#date-of-birth-day").mouseup(function () {
    $("#date-of-birth-year").datepicker('show');
  });
  $("#date-of-birth-month").mouseup(function () {
    $("#date-of-birth-year").datepicker('show');
  });

  $('#wms-date-of-birth-button').click(function () {
    $('#date-of-birth-day').val(dateOfBirthDay < 10
      ? '0' + dateOfBirthDay
      : dateOfBirthDay);
    $('#date-of-birth-month').val(dateOfBirthMonth < 10
      ? '0' + dateOfBirthMonth
      : dateOfBirthMonth);
    $('#date-of-birth-year').val(dateOfBirthYear);
    setAge();
  });

  $('#wms-date-of-birth-button button').off('click').on('click', function () {
    openDialog('wms-date-of-birth-dialog', this);
  });

  $('#cancelDateOfBirth').off('click').on('click', function () {
    return cancel_date_of_birth(this);
  });

  $('#submitDateOfBirth').off('click').on('click', function () {
    return update_date_of_birth();
  });
});

function ageIsValid() {
  var minAge = $('#MinGpReferralAge').val();
  var maxAge = $('#MaxGpReferralAge').val();
  var ageText = $('#date-of-birth-age').text();

  var age = parseInt(ageText);

  return age >= minAge && age <= maxAge;
}

function cancel_date_of_birth(control) {
  $("#date-of-birth-group").removeClass("nhsuk-form-group--error");
  $("#date-of-birth").removeClass("nhsuk-form-group--error");
  $("#date-of-birth-error").hide();
  $('#date-of-birth-day').val('');
  $('#date-of-birth-month').val('');
  $('#date-of-birth-year').val('');
  closeDialog(control);
}

function setAge() {
  var day = $('#date-of-birth-day').val();
  var month = $('#date-of-birth-month').val();
  var year = $('#date-of-birth-year').val();

  var age =
    new Date(new Date() - new Date(year, month - 1, day)).getFullYear() - 1970;

  $('#date-of-birth-age').text(age);
}

function update_date_of_birth() {
  var day = $('#date-of-birth-day').val();
  var month = $('#date-of-birth-month').val();
  var year = $('#date-of-birth-year').val();
  var dateOfBirth = `${year}-${month}-${day}`;

  var date_regex =
    /^(19|20)([0-9])([0-9])-(0[1-9]|1[0-2])-(0[1-9]|1\d|2\d|3[01])$/;

  var result = false;

  if (date_regex.test(dateOfBirth) && ageIsValid()) {
    $("#date-of-birth-error").hide();
    $("#date-of-birth-group-date").removeClass("nhsuk-form-group--error");

    $("#DateOfBirth").val(dateOfBirth);
    result = true;
  } else {
    $("#date-of-birth-error").show();
    $("#date-of-birth-group-date").addClass("nhsuk-form-group--error");

    $("#DateOfBirth").val('');
    result = false;
  }

  return result;
}
