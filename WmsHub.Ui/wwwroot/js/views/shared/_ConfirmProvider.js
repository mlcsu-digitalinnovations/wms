$(document).ready(function () {
  function getConfirmationMessage() {
    $('#displayMessage').text('Confirm provider ' + $('#selectedProvider').val() + ' ?');
    openDialog('wms-provider-dialog', this);
  }

  $('#wms-provider-button button#confirmProvider').off('click').on('click', function () {
    getConfirmationMessage();
  });

  $('#cancelCloseDialog').off('click').on('click', function () {
    closeDialog(this);
  });
});