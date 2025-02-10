$(document).ready(function () {
  var checkCount = 0;

  document.getElementById("confirm-discharge").addEventListener('click', () => {
    closeDialog(document.getElementById('close-btn'));
    checkCount = 0;
    setTimeout(checkForExportSuccess, 500);
  });

  function checkForExportSuccess() {
    console.log('checkForExportSuccess ..... ' + checkCount);
    if (document.cookie.indexOf('Wmp.Discharge.Letters') < 0) {
      checkCount += 1;
      if (checkCount < 100) {
        setTimeout(checkForExportSuccess, 500);
      }
    } else {
      document.cookie = "Wmp.Discharge.Letters=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;"
      removeExportedRows();
    }
  }

  function getConfirmationMessage() {
    document.cookie = "Wmp.Discharge.Letters=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;"

    var selectedReferrals = document.querySelectorAll("input[type='checkbox']:checked");

    if (selectedReferrals.length == 0) return;

    $('#displayMessage').text('Create discharge letters for ' + selectedReferrals.length + ' referral(s) ?');
    openDialog('wms-discharge-dialog', this);
  }

  function removeExportedRows() {

    var selectedReferrals = document.querySelectorAll("input[type='checkbox']:checked");

    if (selectedReferrals.length == 0) return;

    $(':checkbox:checked').each(function (index) {
      var parentRows = $(this).closest('tr');

      if (parentRows.length > 0) {
        var parentRowIndex = parentRows[0].rowIndex;
        document.getElementById('dischargeList').deleteRow(parentRowIndex);
      }
    })
  }

  $('#close-btn').off('click').on('click', function () {
    closeDialog(this)
  });

  $('#wms-discharge-button button').off('click').on('click', function () {
    getConfirmationMessage();
  });
});