$(document).ready(function () {
  $('#closeHelpDialog').off('click').on('click', function () {
    closeDialog(this);
  });

  $('#wms-help-bar button').off('click').on('click', function () {
    openDialog('wms-help-dialog', this);
  });
});
