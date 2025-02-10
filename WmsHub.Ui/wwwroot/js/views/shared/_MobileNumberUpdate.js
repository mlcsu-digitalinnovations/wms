$(document).ready(function () {
  function update_mobile_number() {
    var isValid = true;
    var currentMobile = $('#Mobile').val();

    var mobile = $('#mobile-number').val();
    mobile = mobile.replace(/\s/g, '');

    var mobile_regex = /^\+447(\d[0-9]{8})$/;
    if (mobile_regex.test(mobile)) {
      $("#mobile-number-error").hide();
      $("#mobile-number-group").removeClass("nhsuk-form-group--error");
    } else {
      $("#mobile-number-error").show();
      $("#mobile-number-group").addClass("nhsuk-form-group--error");
      isValid = false;
    }

    if (isValid) {
      $("#Mobile").val(mobile);
      return true;
    }

    $("#Mobile").val(currentMobile);
    return false;
  }

  function cancel_mobile_number(control) {
    $("#mobile-number-group").removeClass("nhsuk-form-group--error");
    $("#mobile-number").removeClass("nhsuk-form-group--error");
    $("#mobile-number-error").hide();
    closeDialog(control);
  }

  $('#submitMobileNumber').off('click').on('click', function () {
    return update_mobile_number();
  });

  $('#wms-mobile-number-button button').off('click').on('click', function () {
    openDialog('wms-mobile-number-dialog', this);
  });

  $('#cancel_mobile_number').off('click').on('click', function () {
    return cancel_mobile_number(this);
  });
});
