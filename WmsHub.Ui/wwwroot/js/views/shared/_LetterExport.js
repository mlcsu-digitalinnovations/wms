$(document).ready(function () {
  document.getElementById("confirm-export").addEventListener('click', () => {
    closeDialog(document.getElementById('close-btn'));
    setTimeout(checkForExportSuccess, 500);
  });

  function checkForExportSuccess() {
    if (document.cookie.indexOf('Wmp.Export.Letters') < 0) {
      setTimeout(checkForExportSuccess, 500);
    } else {
      document.cookie = "Wmp.Export.Letters=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;"
      removeExportedRows();
    }
  }

  function getConfirmationMessage() {
    document.cookie = "Wmp.Export.Letters=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;"

    var selectedReferrals = document.querySelectorAll("input[type='checkbox']:checked");

    if (selectedReferrals.length == 0) return;

    $('#displayMessage').text('Export ' + selectedReferrals.length + ' referral(s) ?');
    openDialog('wms-export-dialog', this);
  }

  function removeExportedRows() {
    var selectedReferrals = document.querySelectorAll("input[type='checkbox']:checked");

    if (selectedReferrals.length == 0) return;

    $(':checkbox:checked').each(function (index) {
      var parentRows = $(this).closest('tr');

      if (parentRows.length > 0) {
        var parentRowIndex = parentRows[0].rowIndex;
        document.getElementById('letterList').deleteRow(parentRowIndex);
      }
    })
  }

  $('#wms-export-button button').off('click').on('click', function () {
    getConfirmationMessage();
  });

  $('#close-btn').off('click').on('click', function () {
    closeDialog(this);
  });
});
