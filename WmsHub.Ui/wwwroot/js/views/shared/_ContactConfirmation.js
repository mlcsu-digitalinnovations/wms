$(document).ready(function () {
  function confirm_unable_to_contact() {
    var isValid = true;

    if ($("#txtUnableToContactReason").val().length < 5) {
      $("#txtUnableToContactReason-error").show();
      $("#unable-to-contact-reason-group").addClass("nhsuk-form-group--error");
      isValid = false;
    } else {
      $("#txtUnableToContactReason-error").hide();
      $("#unable-to-contact-reason-group").removeClass("nhsuk-form-group--error");
    }

    if (isValid) {
      $("#StatusReason").val($("#txtUnableToContactReason").val());
      return true;
    }
    $("#StatusReason").val('');
    return false;
  }

  $('#submitUnableToContact').off('click').on('click', function () {
    return confirm_unable_to_contact();
  });

  $('#cancelCloseButton').off('click').on('click', function () {
    closeDialog(this);
  });

  $('#wms-contact-button button').off('click').on('click', function () {
    openDialog('wms-contact-dialog', this);
  });
});