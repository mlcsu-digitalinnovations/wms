$(document).ready(function () {
  function confirm_rejection() {
    var reasonValue = $("#choiceRejectionReason").val();
    if (reasonValue === "0" || reasonValue === "-1") {
      $("#txtStatusReason-error").show();
      $("#rejection-reason-group").addClass("nhsuk-form-group--error");
      return false;
    }
    $("#StatusReason").val($("#choiceRejectionReason option:selected").text());
    return true;
  }

  function cancel_rejection(control) {
    $("#txtStatusReason").val($("#StatusReason").val());
    $("#rejection-reason-group").removeClass("nhsuk-form-group--error");
    $("#txtStatusReason-error").hide();
    closeDialog(control);
  }

  $('#submitStatusReason').off('click').on('click', function () {
    return confirm_rejection();
  });

  $('#cancelRejection').off('click').on('click', function () {
    return cancel_rejection(this);
  });

  $('#wms-rejection-button button').off('click').on('click', function () {
    openDialog('wms-rejection-dialog', this);
  });
});
