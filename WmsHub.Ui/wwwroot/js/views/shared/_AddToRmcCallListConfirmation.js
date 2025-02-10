$(document).ready(function () {
  $('#cancelCloseDialog').off('click').on('click', function () {
    closeDialog(this);
  });

  $('#wms-add-to-call-list-button button').off('click').on('click', function () {
    openDialog('wms-add-to-call-list-dialog', this);
  });
});
